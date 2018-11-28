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
compiletoram 
: {registers ( -- $FF ) $FF {constseq ;
: register: ( n -- n-1 ) 
  compiletoflash
  constseq1-:
  compiletoram ;
: registers} ( n -- ) constseq} ;

{registers
		register: StackPointer
		register: ControlRegister0
		register: ControlRegister1
		register: ControlRegister2
		register: ControlRegister3
		register: ControlRegister4
		register: ControlRegister5
		register: ControlRegister6
		register: ControlRegister7
		register: ConditionRegister
        register: DefaultLinkRegister
registers}
compiletoflash

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
: iris:perform-goto ( s2 s1 d -- ) imm16-only-form goto ;
: iris:perform-goto-and-link ( s2 s1 d -- ) 2arg-imm16-form update-link-register goto ;
: iris:perform-branch ( s2 s1 d -- ) 1arg-form register@ goto ;
: iris:perform-branch-and-link ( s2 s1 d -- ) 
  2arg-form ( dest link )
  update-link-register ( dest )
  register@ goto ;
: iris:perform-return ( s2 s1 d -- ) 0arg-form pop-word goto ;
: iris:perform-calli ( s2 s1 d -- ) imm16-only-form callg ;
: iris:perform-callr ( s2 s1 d -- ) 1arg-form register@ callg ;

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

['] iris:perform-goto iris:defcondop iris:perform-goto-if-true
['] iris:perform-goto iris:defcondop-false iris:perform-goto-if-false
['] iris:perform-goto-and-link iris:defcondop iris:perform-goto-if-true-and-link
['] iris:perform-goto-and-link iris:defcondop iris:perform-goto-if-false-and-link
['] iris:perform-branch iris:defcondop iris:perform-branch-if-true
['] iris:perform-branch iris:defcondop-false iris:perform-branch-if-false
['] iris:perform-branch-and-link iris:defcondop iris:perform-branch-if-true-and-link
['] iris:perform-branch-and-link iris:defcondop iris:perform-branch-if-false-and-link
['] iris:perform-calli iris:defcondop iris:perform-calli-if-true 
['] iris:perform-calli iris:defcondop-false iris:perform-calli-if-false
['] iris:perform-callr iris:defcondop iris:perform-callr-if-true 
['] iris:perform-callr iris:defcondop-false iris:perform-callr-if-false
['] iris:perform-return iris:defcondop iris:perform-return-if-true
['] iris:perform-return iris:defcondop-false iris:perform-return-if-false
    
\ memory and register manipulation operations
  
: iris:perform-load-data ( s2 s1 dest -- ) 
  2arg-form 
  swap register@ data@ swap register! ;
: iris:perform-store-data ( s2 s1 dest -- ) 2arg-form register@ swap register@ swap data! ;

\ setters and move commands
: iris:perform-set16 ( h l dest -- ) 2arg-imm16-form register! ;
: iris:perform-set12 ( h l dest -- ) 2arg-imm16-form swap mask-lower-12 swap register! ;
: iris:perform-set8 ( h l dest -- ) 2arg-form swap mask-lower-half swap register! ;
: iris:perform-set4 ( h l dest -- ) 2arg-form swap mask-lowest-int4 swap register! ;

: iris:perform-move.reg ( src2 src dest -- ) 2arg-form swap register@ swap register! ;

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

['] =   iris:defcompareop iris:perform-eqo   ['] =   iris:defcompareop iris:perform-eqi
['] <>  iris:defcompareop iris:perform-neqo ['] <>  iris:defcompareop iris:perform-neqi
['] u<= iris:defcompareop iris:perform-leo ['] <=  iris:defcompareop iris:perform-lei
['] u>= iris:defcompareop iris:perform-geo ['] >=  iris:defcompareop iris:perform-gei
['] u<  iris:defcompareop iris:perform-lto ['] <   iris:defcompareop iris:perform-lti
['] u>  iris:defcompareop iris:perform-gto ['] >   iris:defcompareop iris:perform-gti

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

['] +      iris:defarithop iris:perform-addo  ['] +      iris:defarithop iris:perform-addi
['] -      iris:defarithop iris:perform-subo  ['] -      iris:defarithop iris:perform-subi
['] *      iris:defarithop iris:perform-mulo  ['] *      iris:defarithop iris:perform-muli
['] /      iris:defarithop iris:perform-divo  ['] /      iris:defarithop iris:perform-divi
['] mod    iris:defarithop iris:perform-remo  ['] mod    iris:defarithop iris:perform-remi
['] rshift iris:defarithop iris:perform-shro  ['] rshift iris:defarithop iris:perform-shri
['] lshift iris:defarithop iris:perform-shlo  ['] lshift iris:defarithop iris:perform-shli
['] and    iris:defarithop iris:perform-ando  ['] and    iris:defarithop iris:perform-andi
['] or     iris:defarithop iris:perform-oro   ['] or     iris:defarithop iris:perform-ori
['] xor    iris:defarithop iris:perform-xoro  ['] xor    iris:defarithop iris:perform-xori
['] umin   iris:defarithop iris:perform-mino  ['] min    iris:defarithop iris:perform-mini
['] umax   iris:defarithop iris:perform-maxo  ['] max    iris:defarithop iris:perform-maxi

['] +      iris:defarithimmop iris:perform-addom  ['] +      iris:defarithimmop iris:perform-addim
['] -      iris:defarithimmop iris:perform-subom  ['] -      iris:defarithimmop iris:perform-subim
['] *      iris:defarithimmop iris:perform-mulom  ['] *      iris:defarithimmop iris:perform-mulim
['] /      iris:defarithimmop iris:perform-divom  ['] /      iris:defarithimmop iris:perform-divim
['] mod    iris:defarithimmop iris:perform-remom  ['] mod    iris:defarithimmop iris:perform-remim
['] rshift iris:defarithimmop iris:perform-shrom  ['] rshift iris:defarithimmop iris:perform-shrim
['] lshift iris:defarithimmop iris:perform-shlom  ['] lshift iris:defarithimmop iris:perform-shlim
['] and    iris:defarithimmop iris:perform-andom  ['] and    iris:defarithimmop iris:perform-andim
['] or     iris:defarithimmop iris:perform-orom   ['] or     iris:defarithimmop iris:perform-orim
['] xor    iris:defarithimmop iris:perform-xorom  ['] xor    iris:defarithimmop iris:perform-xorim
['] umin   iris:defarithimmop iris:perform-minom  ['] min    iris:defarithimmop iris:perform-minim
['] umax   iris:defarithimmop iris:perform-maxom  ['] max    iris:defarithimmop iris:perform-maxim

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


['] 1+     iris:def2arg iris:perform-inco    ['] 1+     iris:def2arg iris:perform-inci
['] 1-     iris:def2arg iris:perform-deco    ['] 1-     iris:def2arg iris:perform-deci
['] negate iris:def2arg iris:perform-inverto ['] negate iris:def2arg iris:perform-inverti 
['] not    iris:def2arg iris:perform-noto    ['] not    iris:def2arg iris:perform-noti
['] abs    iris:def2arg iris:perform-absi

: iris:perform-illegal ( s2 s1 dest -- )
  halt-execution
  ." Illegal Instruction" cr
  ip@ u.lcd
  ;

: iris:perform-push-word ( s2 s1 dest -- ) 1arg-form register@ push-word ;
: iris:perform-pop-word ( s2 s1 dest -- ) 1arg-form pop-word swap register! ;

create iris:dispatch-table
['] iris:perform-illegal ,
['] iris:perform-addi , ['] iris:perform-addo , ['] iris:perform-subi , 
['] iris:perform-subo , ['] iris:perform-muli , 
['] iris:perform-mulo , ['] iris:perform-divi , ['] iris:perform-divo , 
['] iris:perform-remi , ['] iris:perform-remo ,
['] iris:perform-shli , ['] iris:perform-shlo , ['] iris:perform-shri , 
['] iris:perform-shro , ['] iris:perform-andi ,
['] iris:perform-ando , ['] iris:perform-ori , ['] iris:perform-oro , 
['] iris:perform-xori , ['] iris:perform-xoro ,
['] iris:perform-mini , ['] iris:perform-mino , ['] iris:perform-maxi , 
['] iris:perform-maxo , ['] iris:perform-addim ,
['] iris:perform-addom , ['] iris:perform-subim , ['] iris:perform-subom , 
['] iris:perform-mulim , ['] iris:perform-mulom ,
['] iris:perform-divim , ['] iris:perform-divom , ['] iris:perform-remim , 
['] iris:perform-remom , ['] iris:perform-shlim ,
['] iris:perform-shlom , ['] iris:perform-shrim , ['] iris:perform-shrom , 
['] iris:perform-andim , ['] iris:perform-andom ,
['] iris:perform-orim , ['] iris:perform-orom , ['] iris:perform-xorim , 
['] iris:perform-xorom , ['] iris:perform-minim , 
['] iris:perform-minom , ['] iris:perform-maxim , ['] iris:perform-maxom , 
['] iris:perform-eqi , ['] iris:perform-eqo ,
['] iris:perform-neqi , ['] iris:perform-neqo , ['] iris:perform-lei , 
['] iris:perform-leo , ['] iris:perform-gei , 
['] iris:perform-geo , ['] iris:perform-lti , ['] iris:perform-lto , 
['] iris:perform-gti , ['] iris:perform-gto ,
['] iris:perform-goto , ['] iris:perform-goto-and-link , 
['] iris:perform-branch , ['] iris:perform-branch-and-link ,
['] iris:perform-calli , ['] iris:perform-callr , 
['] iris:perform-return , ['] iris:perform-goto-if-true ,
['] iris:perform-goto-if-false , ['] iris:perform-goto-if-true-and-link ,
['] iris:perform-goto-if-false-and-link , ['] iris:perform-branch-if-true ,
['] iris:perform-branch-if-false , ['] iris:perform-branch-if-true-and-link ,
['] iris:perform-branch-if-false-and-link , ['] iris:perform-callr-if-true ,
['] iris:perform-callr-if-false , ['] iris:perform-calli-if-true , 
['] iris:perform-calli-if-false ,
['] iris:perform-return-if-true , ['] iris:perform-return-if-false , 
['] iris:perform-push-word , ['] iris:perform-pop-word , 
['] iris:perform-set16 , ['] iris:perform-set12 , ['] iris:perform-set8 , 
['] iris:perform-set4 , ['] iris:perform-move.reg ,
['] iris:perform-load-data , ['] iris:perform-store-data ,
['] iris:perform-inci , ['] iris:perform-inco , ['] iris:perform-deci , 
['] iris:perform-deco , ['] iris:perform-inverti , ['] iris:perform-inverto ,
['] iris:perform-noti , ['] iris:perform-noto , ['] iris:perform-absi , 
\ replace iris:perform-illegal addresses with new operations starting here
['] iris:perform-illegal , ['] iris:perform-illegal , 
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal , 
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,
['] iris:perform-illegal , ['] iris:perform-illegal ,

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
\ opcodes
compiletoram
\ the generators need to be in ram
: {opcodes ( -- 0 ) 0 {constseq ;
: opcode: ( n "name" -- n+1 ) 
  compiletoflash
  constseq1+:
  compiletoram ;
: opcodes} ( n -- ) constseq} ;
{opcodes
    opcode: opcode:illegal
    opcode: opcode:addi  
    opcode: opcode:addo  
    opcode: opcode:subi  
    opcode: opcode:subo  
    opcode: opcode:muli  
    opcode: opcode:mulo  
    opcode: opcode:divi  
    opcode: opcode:divo  
    opcode: opcode:remi  
    opcode: opcode:remo 
    opcode: opcode:shli  
    opcode: opcode:shlo  
    opcode: opcode:shri  
    opcode: opcode:shro  
    opcode: opcode:andi 
    opcode: opcode:ando  
    opcode: opcode:ori  
    opcode: opcode:oro  
    opcode: opcode:xori  
    opcode: opcode:xoro 
    opcode: opcode:mini  
    opcode: opcode:mino  
    opcode: opcode:maxi  
    opcode: opcode:maxo  
    opcode: opcode:addim 
    opcode: opcode:addom  
    opcode: opcode:subim  
    opcode: opcode:subom  
    opcode: opcode:mulim  
    opcode: opcode:mulom 
    opcode: opcode:divim  
    opcode: opcode:divom  
    opcode: opcode:remim  
    opcode: opcode:remom  
    opcode: opcode:shlim 
    opcode: opcode:shlom  
    opcode: opcode:shrim  
    opcode: opcode:shrom  
    opcode: opcode:andim  
    opcode: opcode:andom 
    opcode: opcode:orim  
    opcode: opcode:orom  
    opcode: opcode:xorim  
    opcode: opcode:xorom  
    opcode: opcode:minim  
    opcode: opcode:minom  
    opcode: opcode:maxim  
    opcode: opcode:maxom  
    opcode: opcode:eqi  
    opcode: opcode:eqo 
    opcode: opcode:neqi  
    opcode: opcode:neqo  
    opcode: opcode:lei  
    opcode: opcode:leo  
    opcode: opcode:gei  
    opcode: opcode:geo  
    opcode: opcode:lti  
    opcode: opcode:lto  
    opcode: opcode:gti  
    opcode: opcode:gto 
    opcode: opcode:goto  
    opcode: opcode:goto-and-link  
    opcode: opcode:branch  
    opcode: opcode:branch-and-link 
    opcode: opcode:calli  
    opcode: opcode:callr  
    opcode: opcode:return  
    opcode: opcode:goto-if-true 
    opcode: opcode:goto-if-false  
    opcode: opcode:goto-if-true-and-link 
    opcode: opcode:goto-if-false-and-link  
    opcode: opcode:branch-if-true 
    opcode: opcode:branch-if-false  
    opcode: opcode:branch-if-true-and-link 
    opcode: opcode:branch-if-false-and-link  
    opcode: opcode:callr-if-true 
    opcode: opcode:callr-if-false  
    opcode: opcode:calli-if-true  
    opcode: opcode:calli-if-false 
    opcode: opcode:return-if-true  
    opcode: opcode:return-if-false  
    opcode: opcode:push-word  
    opcode: opcode:pop-word  
    opcode: opcode:set16  
    opcode: opcode:set12  
    opcode: opcode:set8  
    opcode: opcode:set4  
    opcode: opcode:move.reg 
    opcode: opcode:load-data  
    opcode: opcode:store-data 
    opcode: opcode:inci  
    opcode: opcode:inco  
    opcode: opcode:deci  
    opcode: opcode:deco  
    opcode: opcode:inverti  
    opcode: opcode:inverto 
    opcode: opcode:noti  
    opcode: opcode:noto  
    opcode: opcode:absi  
opcodes}
compiletoflash

compiletoram

