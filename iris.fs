\ depends on msp430fr6989.fs
\ depends on common.fs
\ depends on iris_opcodes.fs
compiletoflash

\ core structure contents
true variable CoreExec
: executing? ( -- ) CoreExec @ 0<> ; 
: halt-execution ( -- ) false CoreExec ! ;
: resume-execution ( -- ) true CoreExec ! ;
true variable CoreIncrementNext
: increment-next? ( -- ) CoreIncrementNext @ 0<> ;
: no-increment-next ( -- ) false CoreIncrementNext ! ;
: yes-increment-next ( -- ) true CoreIncrementNext ! ;
0 variable CoreIP
: 0ip ( -- ) 0 CoreIP ! ;


$3FFF $2 2constant CodeMemoryEnd
$4400 $0 2constant CodeMemoryStart

$FFFF constant InterruptVectorsEnd
$FF80 constant InterruptVectorsStart

$23FF constant RAMEnd
$1C00 constant RAMStart

256 constant CoreRegisterCount
2 constant BytesPerWord
2 constant WordsPerInstruction

BytesPerWord WordsPerInstruction * constant BytesPerInstruction

\ MEMORY MAP:
0 1 2constant TextStart
$7FFF s>d TextStart d+ 2constant TextEnd
TextEnd d1+ 2constant DataStart
$3FFF s>d DataStart d+ 2constant DataEnd
DataEnd d1+ 2constant StackStart
$3FFF s>d StackStart d+ 2constant StackEnd
StackEnd d1+ 2constant RegistersStart
CoreRegisterCount BytesPerWord * s>d RegistersStart d+ 2constant RegistersEnd \ 256 registers at 2 bytes each
RegistersEnd d1+ 2constant UnusedMemoryStart
CodeMemoryEnd 2constant UnusedMemoryEnd

CoreRegisterCount 1- construct-mask mask-register-index
$1FFF construct-mask mask-text& 
$1FFF construct-mask mask-data&
$1FFF construct-mask mask-stack&

\ register declarations
$FF {constseq 
		constseq1-: StackPointer
		constseq1-: ControlRegister0
		constseq1-: ControlRegister1
		constseq1-: ControlRegister2
		constseq1-: ControlRegister3
		constseq1-: ControlRegister4
		constseq1-: ControlRegister5
		constseq1-: ControlRegister6
		constseq1-: ControlRegister7
		constseq1-: ConditionRegister
	constseq}

\ instruction decoding routines, assume that the double word setup is handled externally
: 2arg-imm16-form ( s2 s dest -- imm16 dest )
  -rot swap ( dest l h )
  unhalve ( dest value ) 
  swap ( value dest ) ;
: imm16-only-form ( h l dest -- imm16 ) drop swap unhalve ;
: 2arg-form ( s2 s d -- s d ) rot drop ;
: 1arg-form ( s2 s d -- d ) -rot 2drop ;
: 0arg-form ( s2 s d -- ) 3drop ;

\ it seems we have access to the upper ~80kb of memory for storage purposes

: register& ( value -- d ) mask-register-index 2* s>d RegistersStart d+ ;
: data& ( value -- d ) mask-data& 2* s>d DataStart d+ ;
: stack& ( value -- d ) mask-stack& 2* s>d StackStart d+ ;

: print-hex-double ( d -- ) ." 0x" hex ud. ;
: print-hex-range ( dend dstart -- ) print-hex-double ." - " print-hex-double ;
: print-memory-map ( -- )
  ." Text: " TextEnd TextStart print-hex-range cr
  ." Data: " DataEnd DataStart print-hex-range cr
  ." Stack: " StackEnd StackStart print-hex-range cr 
  ." Registers: " RegistersEnd RegistersStart print-hex-range cr
  ." Remaining Space: " UnusedMemoryEnd UnusedMemoryStart print-hex-range cr 
  ;

: register@ ( offset -- value ) register& x@ ;
: register! ( value offset -- ) register& x! ;
: data@ ( offset -- value ) data& x@ ;
: data! ( value offset -- ) data& x! ;
: stack@ ( offset -- value ) stack& x@ ;
: stack! ( value offset -- ) stack& x! ;
\ text is special, the xx@ and xx! operations do 20 bit saves and restores so 
\ we have to handle this a little differently
: text& ( offset -- addr-hi addr-lo ) 
  mask-text& \ make sure we are looking at the right location
  WordsPerInstruction lshift \ then shift by two positions
  s>d \ convert it to a double number
  TextStart d+ \ add to base offset
  2dup 2>r     \ now we have the lo address computed so make a copy and stash it
  BytesPerWord s>d d+ \ compute the hi
  2r>                 \ restore addr-lo
  ;
: text@ ( offset -- d ) 
  text& ( addr-hi addr-lo )
  x@ >r \ get the lower half and stash it away
  x@ r> \ get the upper half and then restore the lower half
  swap  \ put it back into double format 
  ;
: text! ( d offset -- ) 
  rot >r text& 2>r \ stash the lower half of the number to the return stack
  x!  \ stash the upper half to memory
  3r> \ restore the lo address followed by lower half of the value
  x! \ move lower half to the proper location and store
  ;

: iris:defreg@ ( loc "name" -- ) <builds , does> ( -- value ) @ register@ ;
: iris:defreg! ( loc "name" -- ) <builds , does> ( value -- ) @ register! ;

StackPointer      iris:defreg@ stp@  StackPointer      iris:defreg! stp!
ConditionRegister iris:defreg@ cond@ ConditionRegister iris:defreg! cond!
ControlRegister0  iris:defreg@ cr0@  ControlRegister0  iris:defreg! cr0!
ControlRegister1  iris:defreg@ cr1@  ControlRegister1  iris:defreg! cr1!
ControlRegister2  iris:defreg@ cr2@  ControlRegister2  iris:defreg! cr2!
ControlRegister3  iris:defreg@ cr3@  ControlRegister3  iris:defreg! cr3!
ControlRegister4  iris:defreg@ cr4@  ControlRegister4  iris:defreg! cr4!
ControlRegister5  iris:defreg@ cr5@  ControlRegister5  iris:defreg! cr5!
ControlRegister6  iris:defreg@ cr6@  ControlRegister6  iris:defreg! cr6!
ControlRegister7  iris:defreg@ cr7@  ControlRegister7  iris:defreg! cr7!

: unpack-cond ( -- value ) cond@ 0<> ;
: pack-cond ( value -- ) 0<> cond! ;

: 0register! ( offset -- ) 0 swap register! ;
: 0text! ( value offset -- ) 0 s>d rot text! ;
: 0data! ( value offset -- ) 0 swap data! ;
: 0stack! ( value offset -- ) 0 swap stack! ;
: print-address-field ( addr -- ) ." 0x" hex. ." : " ;
: print-word-value ( value -- ) ." 0x" hex. ;
: print-single-word-cell ( value addr -- ) print-address-field print-word-value cr ;
: def-print-word-cell ( mask-func accessor-func "name" -- )
  <builds , , 
  does> ( address -- )
  2dup ( address internal address internal )
  cell+ @ execute >r \ perform the masking ahead of time then stash it
  @ execute r> ( value address ) \ load the value from memory 
  print-single-word-cell ;

['] mask-data& ['] data@ def-print-word-cell print-data-cell
['] mask-stack& ['] stack@ def-print-word-cell print-stack-cell

: print-register ( address -- ) 
  save-base
  dup register@ 
  swap 
  mask-register-index 
  ." Register r" decimal u. ." :" 
  print-word-value cr
  restore-base ;

: print-text-cell ( address -- )
  save-base
  dup 
  mask-text& 
  print-address-field 
  text@ ." 0x" hex ud. cr 
  restore-base ;
: def-cell-printer ( func "name" -- )
  <builds , 
  does> ( end start addr -- )
  @ -rot ( func end start )
  do
  i over execute 
  loop 
  drop ;

['] print-text-cell def-cell-printer print-text-cell-range 
['] print-data-cell def-cell-printer print-data-cell-range 
['] print-stack-cell def-cell-printer print-stack-cell-range 
['] print-register def-cell-printer print-register-range 
: print-registers ( -- ) ." Registers" cr CoreRegisterCount 0 print-register-range ;
: memdump ( -- ) 
  print-registers cr
  ." Text Memory" cr $2000 0 print-text-cell-range cr
  ." Data Memory" cr $2000 0 print-data-cell-range cr
  ." Stack Memory" cr $2000 0 print-stack-cell-range cr 
  ;
: 0memory ( -- ) 
  $2000 0 do
  i 0text!
  i 0data!
  i 0stack!
  loop ;
: 0registers ( -- )
  CoreRegisterCount 0 do
  i 0register! 
  loop 
  ;
: iris:reset-storage ( -- )
  0ip
  0registers
  0memory ;
: iris:init-core ( -- ) 
  resume-execution
  yes-increment-next
  0ip
  ;

: ip@ ( -- value ) CoreIP @ mask-text& ;
: ip! ( value -- ) mask-text& CoreIP ! ;
: ip1+ ( -- ) ip@ 1+ ip! ;
: ip1+? ( -- ) CoreIncrementNext @ if ip1+ then ;
: push-word ( word -- ) 
  stp@ \ load the stack pointer address
  1- mask-stack& \ decrement and then go next
  swap over ( masked-stack value masked-stack )
  stack! \ stash to stack
  stp! ( save back to registers ) ;
: pop-word ( -- word )
  stp@ mask-stack& dup \ load the sp, mask it, and make a copy
  stack@ swap \ load from the stack and then switch back to the sp address
  1+ mask-stack& \ increment then mask
  stp! \ update the stack pointer
  ;
\ operations!
\ branch operations
: goto ( value -- ) ip! no-increment-next ;
: get-next-address ( -- addr ) ip@ 1+ mask-text& ;
: update-link-register ( link -- ) get-next-address swap register! ;
: next-address-to-stack ( -- ) get-next-address push-word ;
: callg ( addr -- ) next-address-to-stack goto ;

\ conditional operations
: op:goto ( s2 s1 d -- ) imm16-only-form goto ;
: op:goto-and-link ( s2 s1 d -- ) 2arg-imm16-form update-link-register goto ;
: op:branch ( s2 s1 d -- ) 1arg-form register@ goto ;
: op:branch-and-link ( s2 s1 d -- ) 
  2arg-form ( dest link )
  update-link-register ( dest )
  register@ goto ;
: op:return ( s2 s1 d -- ) 0arg-form pop-word goto ;
: op:calli ( s2 s1 d -- ) imm16-only-form callg ;
: op:callr ( s2 s1 d -- ) 1arg-form register@ callg ;

: iris:defcondop ( uncond-op "name" -- ) 
  <builds ,
  does> ( s2 s1 d -- )
  ( s2 s1 d addr -- )
  unpack-cond if @ execute else 4drop then ;
: iris:defcondop-false ( uncond-op "name" -- )
  <builds ,
  does> ( s2 s1 d -- )
        ( s2 s1 d addr -- )
        unpack-cond not if @ execute else 4drop then ;

['] op:goto iris:defcondop op:goto-if-true
['] op:goto iris:defcondop-false op:goto-if-false
['] op:goto-and-link iris:defcondop op:goto-if-true-and-link
['] op:goto-and-link iris:defcondop op:goto-if-false-and-link
['] op:branch iris:defcondop op:branch-if-true
['] op:branch iris:defcondop-false op:branch-if-false
['] op:branch-and-link iris:defcondop op:branch-if-true-and-link
['] op:branch-and-link iris:defcondop op:branch-if-false-and-link
['] op:calli iris:defcondop op:calli-if-true 
['] op:calli iris:defcondop-false op:calli-if-false
['] op:callr iris:defcondop op:callr-if-true 
['] op:callr iris:defcondop-false op:callr-if-false
['] op:return iris:defcondop op:return-if-true
['] op:return iris:defcondop-false op:return-if-false
    
\ memory and register manipulation operations
  
: op:load-data ( s2 s1 dest -- ) 
  2arg-form 
  swap register@ data@ swap register! ;
: op:store-data ( s2 s1 dest -- ) 2arg-form register@ swap register@ swap data! ;

\ setters and move commands
: op:set16 ( h l dest -- ) 2arg-imm16-form register! ;
: op:set12 ( h l dest -- ) 2arg-imm16-form swap mask-lower-12 swap register! ;
: op:set8 ( h l dest -- ) 2arg-form swap mask-lower-half swap register! ;
: op:set4 ( h l dest -- ) 2arg-form swap mask-lowest-int4 swap register! ;

: op:move.reg ( src2 src dest -- ) 2arg-form swap register@ swap register! ;

\ comparison operations, implied conditional operator is destination
: 2src-extract ( s2 s1 -- r1 r2 ) register@ swap register@ ;
: iris:2reg-binary-execute ( s2 s1 addr -- * )
  -rot 
  2src-extract
  rot @ execute ;
: iris:defcompareop ( operator "name" -- )
  <builds , 
  does> ( src2 src1 dest addr -- )
  >r 2arg-form r> ( s1 dest addr )
  iris:2reg-binary-execute
  pack-cond ;

['] =   iris:defcompareop op:eqo   ['] =   iris:defcompareop op:eqi
['] <>  iris:defcompareop op:neqo ['] <>  iris:defcompareop op:neqi
['] u<= iris:defcompareop op:leo ['] <=  iris:defcompareop op:lei
['] u>= iris:defcompareop op:geo ['] >=  iris:defcompareop op:gei
['] u<  iris:defcompareop op:lto ['] <   iris:defcompareop op:lti
['] u>  iris:defcompareop op:gto ['] >   iris:defcompareop op:gti

\ arithmetic operators
: iris:defarithop ( op "name" -- )
  <builds , 
  does> ( r2 r1 dest addr -- )
  swap >r 
  iris:2reg-binary-execute
  r> register! ;

: iris:defarithimmop ( op "name" -- )
  <builds , 
  does> ( imm8 r1 dest addr -- )
  swap >r 
  -rot ( addr imm8 r1 )
  register@ swap ( addr v1 imm8 )
  rot @ execute 
  r> register! ;

['] +      iris:defarithop op:addo  ['] +      iris:defarithop op:addi
['] -      iris:defarithop op:subo  ['] -      iris:defarithop op:subi
['] *      iris:defarithop op:mulo  ['] *      iris:defarithop op:muli
['] /      iris:defarithop op:divo  ['] /      iris:defarithop op:divi
['] mod    iris:defarithop op:remo  ['] mod    iris:defarithop op:remi
['] rshift iris:defarithop op:shro  ['] rshift iris:defarithop op:shri
['] lshift iris:defarithop op:shlo  ['] lshift iris:defarithop op:shli
['] and    iris:defarithop op:ando  ['] and    iris:defarithop op:andi
['] or     iris:defarithop op:oro   ['] or     iris:defarithop op:ori
['] xor    iris:defarithop op:xoro  ['] xor    iris:defarithop op:xori
['] umin   iris:defarithop op:mino  ['] min    iris:defarithop op:mini
['] umax   iris:defarithop op:maxo  ['] max    iris:defarithop op:maxi

['] +      iris:defarithimmop op:addom  ['] +      iris:defarithimmop op:addim
['] -      iris:defarithimmop op:subom  ['] -      iris:defarithimmop op:subim
['] *      iris:defarithimmop op:mulom  ['] *      iris:defarithimmop op:mulim
['] /      iris:defarithimmop op:divom  ['] /      iris:defarithimmop op:divim
['] mod    iris:defarithimmop op:remom  ['] mod    iris:defarithimmop op:remim
['] rshift iris:defarithimmop op:shrom  ['] rshift iris:defarithimmop op:shrim
['] lshift iris:defarithimmop op:shlom  ['] lshift iris:defarithimmop op:shlim
['] and    iris:defarithimmop op:andom  ['] and    iris:defarithimmop op:andim
['] or     iris:defarithimmop op:orom   ['] or     iris:defarithimmop op:orim
['] xor    iris:defarithimmop op:xorom  ['] xor    iris:defarithimmop op:xorim
['] umin   iris:defarithimmop op:minom  ['] min    iris:defarithimmop op:minim
['] umax   iris:defarithimmop op:maxom  ['] max    iris:defarithimmop op:maxim

\ two argument operations
: iris:def2arg ( op "name" -- )
  <builds , 
  does> ( src2 src dest addr -- )
  >r ( src2 src dest )
  2arg-form r> ( src dest addr )
  rot ( dest addr src )
  register@ swap ( dest contents addr )
  @ execute ( dest value )
  swap register! ;


['] 1+     iris:def2arg op:inco    ['] 1+     iris:def2arg op:inci
['] 1-     iris:def2arg op:deco    ['] 1-     iris:def2arg op:deci
['] negate iris:def2arg op:inverto ['] negate iris:def2arg op:inverti 
['] not    iris:def2arg op:noto    ['] not    iris:def2arg op:noti
['] abs    iris:def2arg op:absi

: op:illegal ( s2 s1 dest -- )
  halt-execution
  ." Illegal Instruction" cr
  ip@ u.lcd
  ;

: op:push-word ( s2 s1 dest -- ) 1arg-form register@ push-word ;
: op:pop-word ( s2 s1 dest -- ) 1arg-form pop-word swap register! ;

create iris:dispatch-table
['] op:illegal ,
['] op:addi , ['] op:addo , ['] op:subi , ['] op:subo , ['] op:muli , 
['] op:mulo , ['] op:divi , ['] op:divo , ['] op:remi , ['] op:remo ,
['] op:shli , ['] op:shlo , ['] op:shri , ['] op:shro , ['] op:andi ,
['] op:ando , ['] op:ori , ['] op:oro , ['] op:xori , ['] op:xoro ,
['] op:mini , ['] op:mino , ['] op:maxi , ['] op:maxo , ['] op:addim ,
['] op:addom , ['] op:subim , ['] op:subom , ['] op:mulim , ['] op:mulom ,
['] op:divim , ['] op:divom , ['] op:remim , ['] op:remom , ['] op:shlim ,
['] op:shlom , ['] op:shrim , ['] op:shrom , ['] op:andim , ['] op:andom ,
['] op:orim , ['] op:orom , ['] op:xorim , ['] op:xorom , ['] op:minim , 
['] op:minom , ['] op:maxim , ['] op:maxom , ['] op:eqi , ['] op:eqo ,
['] op:neqi , ['] op:neqo , ['] op:lei , ['] op:leo , ['] op:gei , 
['] op:geo , ['] op:lti , ['] op:lto , ['] op:gti , ['] op:gto ,
['] op:goto , ['] op:goto-and-link , ['] op:branch , ['] op:branch-and-link ,
['] op:calli , ['] op:callr , ['] op:return , ['] op:goto-if-true ,
['] op:goto-if-false , ['] op:goto-if-true-and-link ,
['] op:goto-if-false-and-link , ['] op:branch-if-true ,
['] op:branch-if-false , ['] op:branch-if-true-and-link ,
['] op:branch-if-false-and-link , ['] op:callr-if-true ,
['] op:callr-if-false , ['] op:calli-if-true , ['] op:calli-if-false ,
['] op:return-if-true , ['] op:return-if-false , ['] op:push-word ,
['] op:pop-word , ['] op:set16 , ['] op:set12 , ['] op:set8 , ['] op:set4 ,
['] op:move.reg , ['] op:load-data , ['] op:store-data , ['] op:inci ,
['] op:inco , ['] op:deci , ['] op:deco , ['] op:inverti , ['] op:inverto ,
['] op:noti , ['] op:noto , ['] op:absi , 
\ replace op:illegal addresses with new operations starting here
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,
['] op:illegal , ['] op:illegal , ['] op:illegal , ['] op:illegal ,

\ debugging features
: iris:default-button-handler ( -- ) 
  buttons-pressed@ dup
  button-s1-pressed? 
  if 
     halt-execution
	 ." Halting Execution" cr
  then
  button-s2-pressed?
  if 
     toggle-debug
     ." Toggling debugging " debug[ ." on" else ." off" ]debug cr
  then ;
['] iris:default-button-handler variable iris:ButtonHandler
: iris:button-handlers ( -- ) 
  iris:ButtonHandler @ execute
  reset-buttons-isr ;

: iris:sysinit ( -- )
  core:sysinit
  s" iris" typelcd
  \ setup the button handlers
  ['] iris:button-handlers irq-port1 !
  eint
  iris:init-core ;
: iris:sysdown ( -- )
  iris:shutdown-core ;

: iris:execution-loop ( -- ) 
  resume-execution
  begin 
    ip@ 
    debug[ dup u.lcd ]debug
    text@ quarter \ decode 
    cells iris:dispatch-table + @ execute
    increment-next? if ip1+ then
    yes-increment-next
    executing? not
  until
  ;

: init ( -- )
  ." Iris Core Simulator" cr
  iris:sysinit ;
compiletoram

