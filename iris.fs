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
: 2imm8s ( v -- hi lo ) halve swap ;
: decode-3reg ( d1 -- src2 src1 dest op ) 
  swap ( hi lo ) 
  quarter swap ( src1 src2 dest op )
  2>r swap ( src2 src1 )
  2r> ( src2 src1 dest op ) 
  ;
: decode-2reg-with-imm ( d1 -- imm8 src1 dest op ) decode-3reg ;
: decode-2reg ( d1 -- src1 dest op ) 
  swap ( hi lo )
  quarter swap ( src1 nil dest op ) 
  rot drop ( src1 dest op ) 
  ;
: decode-imm16 ( d1 -- imm16 dest op ) 
  swap ( imm16 lo )
  2imm8s ( imm16 dest op ) 
  ;

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
StackPointer iris:defreg@ stp@ 
StackPointer iris:defreg! stp!
ConditionRegister iris:defreg@ cond@
ConditionRegister iris:defreg! cond!
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

: iris:init-core ( -- ) 
  resume-execution
  yes-increment-next
  0ip

  CoreRegisterCount 0 do
  i 0register!
  loop

  $2000 0 do
  i 0text!
  i 0data!
  i 0stack!
  loop
  ;
: iris:shutdown-core ( -- )
  \ todo zero out memory as we see fit
  CoreRegisterCount 0 do
  i 0register! 
  loop
  $2000 0 do
  i 0text!
  i 0data!
  i 0stack!
  loop
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
: goto ( value -- ) 
  ip! 
  false CoreIncrementNext !  ;
: get-next-address ( -- addr )
  ip@ 1+ mask-text& ;
: update-link-register ( link -- ) 
  get-next-address swap register! ;
: next-address-to-stack ( -- ) get-next-address push-word ;
: goto-and-link ( value register -- ) 
  update-link-register goto ;
: branch ( register -- ) register@ goto ; 
: branch-and-link ( dest link -- ) update-link-register branch ;
: calli ( addr -- ) next-address-to-stack goto ;
: callr ( register -- ) register@ calli ;
: return ( -- ) pop-word goto ; 

\ conditional operations
: unpack-cond ( -- value ) cond@ 0<> ;
: pack-cond ( value -- ) 0<> cond! ;
: goto-if-true ( value -- ) 
  unpack-cond 
  if 
    goto 
  else 
    drop 
  then ;
: goto-if-true-and-link ( v l -- ) 
  unpack-cond 
  if 
     goto-and-link 
  else 
     2drop 
  then ;
: branch-if-true ( v -- ) 
  unpack-cond 
  if 
    branch 
  else 
    drop 
  then ;
: branch-if-true-and-link ( v l -- ) 
  unpack-cond 
  if 
    branch-and-link 
  else 
    2drop 
  then ;
: calli-if-true ( v -- ) 
  unpack-cond 
  if 
    calli 
  else 
    drop 
  then ;
: callr-if-true ( v -- ) 
  unpack-cond
  if 
    callr 
  else 
    drop 
  then ;
: return-if-true ( -- ) 
  unpack-cond
  if 
    return 
  then ;

: goto-if-false ( value -- ) not goto-if-true ;
: goto-if-false-and-link ( v l -- ) not goto-if-true-and-link ;
: branch-if-false ( v -- ) not branch-if-true ;
: branch-if-false-and-link ( v l -- ) not branch-if-true-and-link ;
: callr-if-false ( v -- ) not callr-if-true ;
: calli-if-false ( v -- ) not calli-if-true ;
: return-if-false ( -- ) not return-if-true ;

\ memory and register manipulation operations
  
: lddat ( addr dest -- ) swap register@ data@ swap register! ;
: stdat ( value dest -- ) register@ swap register@ swap data! ;
\ setters and move commands
: defpartialset ( function "name" -- ) 
  <builds , 
  does>
  swap >r ( value fn )
  @ execute r> register! ;

['] mask-lowest-int4 defpartialset set4
['] mask-lower-half defpartialset set8
['] mask-lower-12 defpartialset set12
: set16  ( imm dest -- ) register! ;
: move.reg ( src dest -- ) swap register@ swap register! ;

: 1reg>2reg ( address -- lo hi ) 
  2* \ make it immediately even by zeroing the contents
  mask-register-index 
  dup 1+ ;
\ comparison operations, implied conditional operator is destination
: iris:defcompareop ( operator "name" -- )
  <builds , 
  does> ( a2 a1 -- )
  -rot ( addr a2 a1 )
  register@ swap register@ ( addr r1 r2 )
  rot @ execute ( outcome ) 
  pack-cond ;

['] =   iris:defcompareop eqo   
['] =   iris:defcompareop eqi
['] <>  iris:defcompareop neqo 
['] <>  iris:defcompareop neqi
['] u<= iris:defcompareop leo 
['] <=  iris:defcompareop lei
['] u>= iris:defcompareop geo 
['] >=  iris:defcompareop gei
['] u<  iris:defcompareop lto 
['] <   iris:defcompareop lti
['] u>  iris:defcompareop gto 
['] >   iris:defcompareop gti

\ arithmetic operators
: iris:defarithop ( op "name" -- )
  <builds , 
  does> ( r2 r1 dest addr -- )
  swap >r -rot 
  register@ swap register@ 
  rot @ execute 
  r> register! ;
: iris:defarithimmop ( op "name" -- )
  <builds , 
  does> ( imm8 r1 dest addr -- )
  swap >r -rot ( addr imm8 r1 )
  register@ swap ( addr v1 imm8 )
  rot @ execute 
  r> register! ;

['] +      iris:defarithop addo  
['] +      iris:defarithop addi
['] -      iris:defarithop subo  
['] -      iris:defarithop subi
['] *      iris:defarithop mulo  
['] *      iris:defarithop muli
['] /      iris:defarithop divo  
['] /      iris:defarithop divi
['] mod    iris:defarithop remo  
['] mod    iris:defarithop remi
['] rshift iris:defarithop shro  
['] rshift iris:defarithop shri
['] lshift iris:defarithop shlo  
['] lshift iris:defarithop shli
['] and    iris:defarithop ando  
['] and    iris:defarithop andi
['] or     iris:defarithop oro   
['] or     iris:defarithop ori
['] xor    iris:defarithop xoro  
['] xor    iris:defarithop xori
['] umin   iris:defarithop mino  
['] min    iris:defarithop mini
['] umax   iris:defarithop maxo  
['] max    iris:defarithop maxi

['] +      iris:defarithimmop addom  
['] +      iris:defarithimmop addim
['] -      iris:defarithimmop subom  
['] -      iris:defarithimmop subim
['] *      iris:defarithimmop mulom  
['] *      iris:defarithimmop mulim
['] /      iris:defarithimmop divom  
['] /      iris:defarithimmop divim
['] mod    iris:defarithimmop remom  
['] mod    iris:defarithimmop remim
['] rshift iris:defarithimmop shrom  
['] rshift iris:defarithimmop shrim
['] lshift iris:defarithimmop shlom  
['] lshift iris:defarithimmop shlim
['] and    iris:defarithimmop andom  
['] and    iris:defarithimmop andim
['] or     iris:defarithimmop orom   
['] or     iris:defarithimmop orim
['] xor    iris:defarithimmop xorom  
['] xor    iris:defarithimmop xorim
['] umin   iris:defarithimmop minom  
['] min    iris:defarithimmop minim
['] umax   iris:defarithimmop maxom  
['] max    iris:defarithimmop maxim

\ two argument operations
: iris:def2arg ( op "name" -- )
  <builds , 
  does> ( src dest addr -- )
  rot ( dest addr src )
  register@  swap ( dest contents addr )
  @ execute ( dest value )
  swap register! ;

: iris:def2immarg ( op "name" -- )
  <builds ,
  does> ( imm dest addr -- )
  swap >r ( imm addr ) 
  @ execute ( result )
  r> ( result dest )
  register! ;

['] 1+     iris:def2arg inco    
['] 1+     iris:def2arg inci
['] 1-     iris:def2arg deco    
['] 1-     iris:def2arg deci
['] negate iris:def2arg inverto 
['] negate iris:def2arg inverti 
['] not    iris:def2arg noto    
['] not    iris:def2arg noti
['] abs    iris:def2arg absi
['] 1+     iris:def2immarg incom    
['] 1+     iris:def2arg incim
['] 1-     iris:def2immarg decom    
['] 1-     iris:def2arg decim
['] negate iris:def2immarg invertom 
['] negate iris:def2arg invertim
['] not    iris:def2immarg notom    
['] not    iris:def2immarg notim
['] abs    iris:def2immarg absim

: op:illegal ( s2 s1 dest -- )
  halt-execution
  ." Illegal Instruction" cr
  ip@ u.lcd
  ;


compiletoram

