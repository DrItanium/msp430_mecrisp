\ Iris cpu emulation
\ extra routines for my purposes
compiletoflash
\ missing functions that I think are really neat
: even? ( value -- f ) dup even = ;
: odd? ( value -- f ) dup even <> ;
: lower-half ( value -- l ) $00FF and ;
: upper-half ( value -- h ) $FF00 and 8 rshift ;
: d1+ ( d -- d+1 ) 1 s>d d+ ;
: d1- ( d -- d-1 ) 1 s>d d- ;
: save-base ( -- ) postpone base postpone @ postpone >r immediate ; 
: restore-base ( -- ) postpone r> postpone base postpone !  immediate ;
: 3>r ( a b c -- ) 
  postpone rot ( b c a )
  postpone >r ( b c )
  postpone 2>r ( )
  immediate ;
: 3r> ( -- a b c )
  postpone 2r> ( b c )
  postpone r>  ( b c a )
  postpone -rot 
  immediate ;

\ constants for the MSP430FR6989 taken from the data sheet
\ end address is inclusive
$3FFF $2 2constant CodeMemoryEnd
$4400 $0 2constant CodeMemoryStart

$FFFF constant InterruptVectorsEnd
$FF80 constant InterruptVectorsStart

$23FF constant RAMEnd
$1C00 constant RAMStart


\ instruction decoding routines, assume that the double word setup is handled externally
: 2imm8s ( v -- hi lo ) 
  dup 
  upper-half swap 
  lower-half ;
: decode-3reg ( d1 -- src2 src1 dest op ) >r 2imm8s r> 2imm8s ;
: decode-2reg-with-imm ( d1 -- imm8 src1 dest op ) decode-reg3 ;
: decode-2reg ( d1 -- src1 dest op ) >r lower-half r> 2imm8s ;
: decode-imm16 ( d1 -- imm16 dest op ) 2imm8s ;
\ core structure contents
true variable CoreExec
true variable CoreIncrementNext
0 variable CoreIP
256 constant CoreRegisterCount
2 constant BytesPerWord
2 constant WordsPerInstruction
BytesPerWord WordsPerInstruction * constant BytesPerInstruction
$FFF 1+ constant MemorySize
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

CoreRegisterCount 1- constant RegisterMask
$1FFF constant TextMask \ each instruction is 4 bytes wide so we have 8192 instruction words total
$1FFF constant DataMask 
$1FFF constant StackMask 
: print-hex-double ( d -- ) ." 0x" hex ud. ;
: print-hex-range ( dend dstart -- ) print-hex-double ." - " print-hex-double ;
: print-memory-map ( -- )
  ." Text: " TextEnd TextStart print-hex-range cr
  ." Data: " DataEnd DataStart print-hex-range cr
  ." Stack: " StackEnd StackStart print-hex-range cr 
  ." Registers: " RegistersEnd RegistersStart print-hex-range cr
  ." Remaining Space: " UnusedMemoryEnd UnusedMemoryStart print-hex-range cr 
  ;

$FF constant StackPointer
$FE constant ConditionRegister
: compute-addr-mask ( value mask num-words -- daddr ) >r and r> lshift s>d ;
: generic& ( value mask num-words dbase -- daddr ) 
  3>r and r> lshift s>d 
  2r> d+ ;
: register& ( offset -- addr ) RegisterMask 1 RegistersStart generic& ; 
: data& ( offset -- addr ) DataMask 1 DataStart generic& ;
: stack& ( offset -- addr ) StackMask 1 StackStart generic& ;
: register@ ( offset -- value ) register& x@ ;
: register! ( value offset -- ) register& x! ;
: data@ ( offset -- value ) data& x@ ;
: data! ( value offset -- ) data& x! ;
: stack@ ( offset -- value ) stack& x@ ;
: stack! ( value offset -- ) stack& x! ;
\ text is special, the xx@ and xx! operations do 20 bit saves and restores so 
\ we have to handle this a little differently
: text& ( offset -- addr-hi addr-lo ) 
  TextMask and \ make sure we are looking at the right location
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


: stp@ ( -- value ) StackPointer register@ ;
: stp! ( value -- ) StackPointer register! ;
: cond@ ( -- value ) ConditionRegister register@ ;
: cond! ( value -- ) ConditionRegister register! ;
: 0register! ( offset -- ) 0 swap register! ;
: 0text! ( value offset -- ) 0 s>d rot text! ;
: 0data! ( value offset -- ) 0 swap data! ;
: 0stack! ( value offset -- ) 0 swap stack! ;
: iris-text-address ( a -- fa ) TextMask and ;
: iris-stack-address ( a -- fa ) StackMask and ;
: iris-data-address ( a -- fa ) DataMask and ;


: print-text-cell ( address -- )
  save-base
  dup 
  ." 0x" iris-text-address hex. ." : 0x" text@ hex ud. cr 
  restore-base ;
: print-word-cell ( value address -- )
  save-base
  ." 0x" hex. ." : 0x" u. cr
  restore-base ;
: print-data-cell ( address -- ) dup data@ swap iris-data-address print-word-cell ;
: print-stack-cell ( address -- ) dup stack@ swap iris-stack-address print-word-cell ;
: print-register ( address -- ) 
  save-base
  dup register@ swap RegisterMask and ." Register r" decimal u. ." : 0x" hex. cr 
  restore-base ;
: print-text-cell-range ( end start -- ) do i print-text-cell loop ;
: print-data-cell-range ( end start -- ) do i print-data-cell loop ;
: print-stack-cell-range ( end start -- ) do i print-stack-cell loop ;
: print-register-range ( end start -- ) do i print-register loop ;
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
: ip! ( value -- ) TextMask and CoreIP ! ;
: ip1+ ( -- ) ip@ 1+ ip! ;
: push-word ( word -- ) 
  stp@ \ load the stack pointer address
  1- StackMask and \ decrement and then go next
  swap over ( masked-stack value masked-stack )
  stack! \ stash to disk
  stp! ( save back to registers ) ;
: pop-word ( -- word )
  stp@ StackMask and dup \ load the sp, mask it, and make a copy
  stack@ swap \ load from the stack and then switch back to the sp address
  1+ StackMask and \ increment then mask
  stp! \ update the stack pointer
  ;
: load.data ( value dest -- ) swap data@ swap register! ;
: store.data ( value dest -- ) register@ data! ;
: move.data ( src dest -- )
  swap register@ data@ swap ( data-src-contents dest -- )
  store.data ; 
: stash-dest ( dest -- ) postpone >r immediate ;
: update-dest ( -- dest ) postpone r> postpone register! immediate ;
: unpack-2src ( s2 s1 -- r1 r2 ) 
  postpone register@
  postpone swap
  postpone register@ immediate ;
: 3arg-begin ( s2 s1 d -- r1 r2 )
  postpone stash-dest 
  postpone unpack-2src immediate ;
: 3arg-end ( v -- ) postpone update-dest immediate ;
: 2arg-begin ( s1 d -- r1 )
  postpone stash-dest
  postpone register@
  immediate ;
: 2arg-end ( s1 -- ) 
    postpone update-dest 
    immediate ;
: def3arg <builds , does> 3arg-begin rot @ execute 3arg-end ;
: def2arg <builds , does> 2arg-begin swap @ execute 2arg-end ;
: move ( src dest -- ) swap register@ swap register! ;
: goto ( value -- ) 
  $1FFF and CoreIP ! 
  false CoreIncrementNext !  ;
: get-next-address ( -- addr )
  CoreIP @ 1+ $1FFF and ;
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



compiletoram

