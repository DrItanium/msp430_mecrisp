\ Iris cpu emulation
\ extra routines for my purposes
compiletoflash
\ constants for the MSP430FR6989 taken from the data sheet
\ end address is inclusive
$3FFF $2 2constant CodeMemoryEnd
$4400 $0 2constant CodeMemoryStart

$FFFF constant InterruptVectorsEnd
$FF80 constant InterruptVectorsStart

$23FF constant RAMEnd
$1C00 constant RAMStart


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
\ core structure contents
true variable CoreExec
true variable CoreIncrementNext
0 variable CoreIP

256 constant CoreRegisterCount
2 constant BytesPerWord
2 constant WordsPerInstruction
BytesPerWord WordsPerInstruction * constant BytesPerInstruction
\ it seems we have access to the upper ~80kb of memory for storage purposes
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

: generate-addr-func ( base-double-address shift-amount mask-func "name" -- )
  <builds , , , ,
  does> ( value implied-addr -- )
  dup cell+ >r \ save a copy of the address
  @ execute \ mask function called
  r> dup cell+ >r  \ goto the next cell and stash that to the return stack
  @ lshift s>d \ shift left by the specified amount then make double number
  r> dup \ extract the current cell value
  @ swap \ load the lower half 
  cell+ @ \ lower the upper half
  d+ \ combine 
  ;

RegistersStart 1 ['] mask-register-index generate-addr-func register&
DataStart 1 ['] mask-data& generate-addr-func data&
StackStart 1 ['] mask-stack& generate-addr-func stack&

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

$FF constant StackPointer
$FE constant ConditionRegister
: defreg@ ( loc "name" -- ) <builds , does> ( -- value ) @ register@ ;
: defreg! ( loc "name" -- ) <builds , does> ( value -- ) @ register! ;
StackPointer defreg@ stp@ 
StackPointer defreg! stp!
ConditionRegister defreg@ cond@
ConditionRegister defreg! cond!
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

: init-core ( -- ) 
  true CoreExec !
  true CoreIncrementNext !
  0 CoreIP !

  CoreRegisterCount 0 do
  i 0register!
  loop

  $2000 0 do
  i 0text!
  i 0data!
  i 0stack!
  loop
  ;
: shutdown-core ( -- )
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
: ip@ ( -- value ) CoreIP @ ;
: ip! ( value -- ) mask-text& CoreIP ! ;
: ip1+ ( -- ) ip@ 1+ ip! ;
: push-word ( word -- ) 
  stp@ \ load the stack pointer address
  1- mask-stack& \ decrement and then go next
  swap over ( masked-stack value masked-stack )
  stack! \ stash to disk
  stp! ( save back to registers ) ;
: pop-word ( -- word )
  stp@ mask-stack& dup \ load the sp, mask it, and make a copy
  stack@ swap \ load from the stack and then switch back to the sp address
  1+ mask-stack& \ increment then mask
  stp! \ update the stack pointer
  ;
: load.data ( value dest -- ) swap data@ swap register! ;
: store.data ( value dest -- ) register@ data! ;
: move.data ( src dest -- )
  swap register@ data@ swap ( data-src-contents dest -- )
  store.data ; 
: stash-dest ( dest -- ) postpone >r immediate ;
: update-dest ( -- dest ) postpone r> postpone register! immediate ;
: def3arg ( operation "name" -- )
  <builds , 
  does> ( s2 s1 d addr -- )
  @ swap ( s2 s1 func d )
  stash-dest  ( s2 s1 func )
  -rot ( func s2 s1 )
  register@ swap register@ ( func r1 r2 )
  rot ( r1 r2 func )
  execute ( outcome )
  update-dest ;

: def2arg ( operation "name" -- )
  <builds , 
  does> ( s1 d addr -- )
  @ swap ( s1 func d )
  stash-dest ( s1 func )
  swap register@ swap ( r1 func )
  execute
  update-dest ;

: mov.reg ( src dest -- ) swap register@ swap register! ;
: set  ( value dest -- ) register! ;
: set4 ( value dest -- ) $f and set ;
: set8 ( value dest -- ) $ff and set ;
  


['] +      def3arg addo  ['] +      def3arg addi
['] -      def3arg subo  ['] -      def3arg subi
['] *      def3arg mulo  ['] *      def3arg muli
['] /      def3arg divo  ['] /      def3arg divi
['] mod    def3arg remo  ['] mod    def3arg remi
['] rshift def3arg shro  ['] rshift def3arg shri
['] lshift def3arg shlo  ['] lshift def3arg shli
['] and    def3arg ando  ['] and    def3arg andi
['] or     def3arg oro   ['] or     def3arg ori
['] xor    def3arg xoro  ['] xor    def3arg xori
['] 1+     def2arg inco  ['] 1+     def2arg inci
['] 1-     def2arg deco  ['] 1-     def2arg deci
['] umin   def3arg mino  ['] min    def2arg mini
['] umax   def3arg maxo  ['] max    def2arg maxi
['] abs    def2arg absi  
['] negate def2arg noto  ['] negate def2arg noti 
['] u<=    def3arg leo   ['] <=     def3arg lei
['] u>=    def3arg geo   ['] >=     def3arg gei
['] u<     def3arg lto   ['] <      def3arg lti
['] u>     def3arg gto   ['] >      def3arg gti
['] =      def3arg eqo   ['] =      def3arg eqi
['] <>     def3arg neqo  ['] <>     def3arg neqi


: goto ( value -- ) 
  mask-text& CoreIP ! 
  false CoreIncrementNext !  ;
: get-next-address ( -- addr )
  CoreIP @ 1+ mask-text& ;
: goto-and-link ( value register -- ) 
  get-next-address swap register! goto ;

compiletoram

