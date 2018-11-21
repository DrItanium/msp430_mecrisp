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


\ addresses taken from data sheets
: &pasel0 ( port-base -- addr ) $0a + ;
: &pasel1 ( port-base -- addr ) $0c + ;
: &padir  ( port-base -- addr ) $04 + ;
: &paren  ( port-base -- addr ) $06 + ;
: &pain   ( port-base -- addr ) ;
: &paout  ( port-base -- addr ) $02 + ;
: &paiv   ( port-base -- addr ) $0e + ;
: &paselc ( port-base -- addr ) $16 + ;
: &paies  ( port-base -- addr ) $18 + ;
: &paie   ( port-base -- addr ) $1a + ;
: &paifg  ( port-base -- addr ) $1c + ;
$200 constant P1BASE
$201 constant P2BASE
$220 constant P3BASE
$221 constant P4BASE
$240 constant P5BASE
$241 constant P6BASE
$260 constant P7BASE
$261 constant P8BASE
$280 constant P9BASE
$281 constant P10BASE
\ constants for the MSP430FR6989 taken from the data sheet
\ end address is inclusive
$3FFF $2 2constant CodeMemoryEnd
$4400 $0 2constant CodeMemoryStart

$FFFF constant InterruptVectorsEnd
$FF80 constant InterruptVectorsStart

$23FF constant RAMEnd
$1C00 constant RAMStart

compiletoram
: def-port-regs ( base "constants" -- ) 
  compiletoflash
  dup constant \ port-id
  dup &pain constant 
  dup &paout constant 
  dup &padir constant 
  dup &paren constant 
  dup &pasel0 constant
  dup &pasel1 constant
  dup &paiv constant 
  dup &paselc constant 
  dup &paies constant
  dup &paie constant
  dup &paifg constant 
  compiletoram
  drop ;
p1base def-port-regs port1 p1in p1out p1dir p1ren p1sel0 p1sel1 p1iv p1selc p1ies p1ie p1ifg
p2base def-port-regs port2 p2in p2out p2dir p2ren p2sel0 p2sel1 p2iv p2selc p2ies p2ie p2ifg
p3base def-port-regs port3 p3in p3out p3dir p3ren p3sel0 p3sel1 p3iv p3selc p3ies p3ie p3ifg
p4base def-port-regs port4 p4in p4out p4dir p4ren p4sel0 p4sel1 p4iv p4selc p4ies p4ie p4ifg
p5base def-port-regs port5 p5in p5out p5dir p5ren p5sel0 p5sel1 p5iv p5selc p5ies p5ie p5ifg
p6base def-port-regs port6 p6in p6out p6dir p6ren p6sel0 p6sel1 p6iv p6selc p6ies p6ie p6ifg
p7base def-port-regs port7 p7in p7out p7dir p7ren p7sel0 p7sel1 p7iv p7selc p7ies p7ie p7ifg
p8base def-port-regs port8 p8in p8out p8dir p8ren p8sel0 p8sel1 p8iv p8selc p8ies p8ie p8ifg
p9base def-port-regs port9 p9in p9out p9dir p9ren p9sel0 p9sel1 p9iv p9selc p9ies p9ie p9ifg
p10base def-port-regs port10 p10in p10out p10dir p10ren p10sel0 p10sel1 p10iv p10selc p10ies p10ie p10ifg
compiletoflash
$80 constant Pin7
$40 constant Pin6
$20 constant Pin5
$10 constant Pin4
$08 constant Pin3
$04 constant Pin2
$02 constant Pin1
$01 constant Pin0




\ taken from the lcd examples verbatim
\ -----------------------------------------------------------------------------
\ LCD driver for MSP430FR6989 Launchpad
\ -----------------------------------------------------------------------------

\ Pins wirings internal:

\   P1.0 Red LED
\   P9.7 Green LED
\   P3.4 TXD
\   P3.5 RXD
\   P1.1 Button S1
\   P1.2 Button S2

$A00 constant LCDCCTL0
$A02 constant LCDCCTL1
$A04 constant LCDCBLKCTL
$A06 constant LCDCMEMCTL
$A08 constant LCDCVCTL
$A0A constant LCDCPCTL0
$A0C constant LCDCPCTL1
$A0E constant LCDCPCTL2
$A12 constant LCDCCPCTL
$A1E constant LCDCIV

$A20 constant LCDMEM \ Byte access LCD memory 0 ($A20) to LCD memory 43 ($A4A)

: lcd-init ( -- )

  0 LCDCCTL0 !  \ Turn off LCD

  $FFFF LCDCPCTL0 !  \ L0~L21 & L26~L43 pins selected
  $FC3F LCDCPCTL1 !
  $0FFF LCDCPCTL2 !

   8  9 lshift     \ Voltage for display drive
   1  3 lshift or   \ Charge pump enable
  LCDCVCTL !

   1 15 lshift
  LCDCCPCTL !     \ Charge pump clock synchronisation

  2 LCDCMEMCTL !  \ Clear LCD memory

  4 8 lshift      \ Prescaler /16
  3 3 lshift or    \ 4-Mux mode
  1 2 lshift or     \ Segments on
  1 0 lshift or      \ LCD on
  LCDCCTL0 !
;

\ -----------------------------------------------------------------------------
\ Artwork for a 14-Segment Font
\ -----------------------------------------------------------------------------

create lcdchars           \ Additional artwork welcome !

$0000 ,  \ 32: Space
$B090 ,  \ 33 !
$6000 ,  \ 34 " 
$5073 ,  \ 35 # 
$50B7 ,  \ 36 $
$2824 ,  \ 37 %
$A29A ,  \ 38 &
$4000 ,  \ 39 ' 
$2200 ,  \ 40 ( 
$8800 ,  \ 41 ) 
$FA03 ,  \ 42 * 
$5003 ,  \ 43 + 
$1000 ,  \ 44 , 
$0003 ,  \ 45 - 
$0100 ,  \ 46 . 
$2800 ,  \ 47 / 
$28FC ,  \ 48 0 
$2060 ,  \ 49 1 
$00DB ,  \ 50 2 
$00F3 ,  \ 51 3 
$0067 ,  \ 52 4 
$00B7 ,  \ 53 5 
$00BF ,  \ 54 6 
$00E4 ,  \ 55 7 
$00FF ,  \ 56 8 
$00F7 ,  \ 57 9 
$5000 ,  \ 58 :
$4800 ,  \ 59 ;
$2810 ,  \ 60 < 
$0013 ,  \ 61 = 
$8210 ,  \ 62 > 
$10C5 ,  \ 63 ?

$40DD ,  \ 64 @
$00EF ,  \ 65 A
$50F1 ,  \ 66 B
$009C ,  \ 67 C
$50F0 ,  \ 68 D
$009F ,  \ 69 E
$008F ,  \ 70 F
$00BD ,  \ 71 G
$006F ,  \ 72 H
$5090 ,  \ 73 I
$0078 ,  \ 74 J
$220E ,  \ 75 K
$001C ,  \ 76 L
$A06C ,  \ 77 M
$826C ,  \ 78 N
$00FC ,  \ 79 O
$00CF ,  \ 80 P
$02FC ,  \ 81 Q
$02CF ,  \ 82 R
$00B7 ,  \ 83 S
$5080 ,  \ 84 T
$007C ,  \ 85 U
$280C ,  \ 86 V
$0A6C ,  \ 87 W
$AA00 ,  \ 88 X
$B000 ,  \ 89 Y
$2890 ,  \ 90 Z
$009C ,  \ 91 [
$8200 ,  \ 92 \
$00F0 ,  \ 93 ]
$2040 ,  \ 94 ^
$0010 ,  \ 95 _
 
$8000 ,  \ 96  `
$00EF ,  \ 97  a
$50F1 ,  \ 98  b
$009C ,  \ 99  c
$50F0 ,  \ 100 d
$009F ,  \ 101 e
$008F ,  \ 102 f
$00BD ,  \ 103 g
$006F ,  \ 104 h
$5090 ,  \ 105 i
$0078 ,  \ 106 j
$220E ,  \ 107 k
$001C ,  \ 108 l
$A06C ,  \ 109 m
$826C ,  \ 110 n
$00FC ,  \ 111 o
$00CF ,  \ 112 p
$02FC ,  \ 113 q
$02CF ,  \ 114 r
$00B7 ,  \ 115 s
$5080 ,  \ 116 t
$007C ,  \ 117 u
$280C ,  \ 118 v
$0A6C ,  \ 119 w
$AA00 ,  \ 120 x
$B000 ,  \ 121 y
$2890 ,  \ 122 z
$8892 ,  \ 123 {
$5000 ,  \ 124 |
$2291 ,  \ 125 }
$A004 ,  \ 126 ~
$0000 ,  \ 127 DEL

: lcdchar ( c -- x ) \ Translates ASCII to LCD-Bitpatterns.                     
  32 umax 127 umin
  32 - cells lcdchars + @
1-foldable ;
 
\ -----------------------------------------------------------------------------
\ Types your strings !
\ -----------------------------------------------------------------------------

: get-first-char ( addr len -- addr   len c ) over c@ ;
: cut-first-char ( addr len -- addr+1 len-1 ) 1- swap 1+ swap ;

0 variable lcdposition

: unaligned-bis! ( x c-addr -- )
  over >< over 1+ cbis!
  cbis!
;

: lcd! ( x -- ) \ Display a character bitmap on given position
  lcdposition @ case
                  0 of lcdmem  9 + unaligned-bis! endof
                  1 of lcdmem  5 + unaligned-bis! endof
                  2 of lcdmem  3 + unaligned-bis! endof
                  3 of lcdmem 18 +           bis! endof
                  4 of lcdmem 14 +           bis! endof
                  5 of lcdmem  7 + unaligned-bis! endof
                drop
                endcase  
;

: clearlcd ( -- ) lcdmem 2+ 18 0 fill ;

: typelcd ( addr len -- )
  clearlcd
  0 lcdposition !

  dup 0<> if get-first-char 45 =  \ Is this a "-" ? The display has a special segment for a minus at the beginning.
             if 4 lcdmem 10 + cbis! cut-first-char then
          then

  begin
    dup 0<>
  while
    get-first-char lcdchar lcd! cut-first-char
    dup 0<> if get-first-char 46 = \ Is this a "." ? Segments for decimal dots available !
               if $100 lcd! cut-first-char then
            then
    1 lcdposition +!
  repeat
  2drop
;

:  .lcd ( n -- ) s>d tuck dabs <# # # # # # # rot sign #> typelcd ;
: u.lcd ( u -- )   0           <# # # # # # #          #> typelcd ;

\ interact with buttons and such
: bit-set? ( value bit -- f ) and 0<> ;
\ taken from the blinky examples
: led-init ( -- ) 
  Pin0 P1DIR cbis!
  Pin7 P9DIR cbis! ;
: led1-off ( -- ) Pin0 P1OUT cbic! ;
: led2-off ( -- ) Pin7 P9OUT cbic! ;
: led1-toggle ( -- ) Pin0 P1OUT cxor! ;
: led2-toggle ( -- ) Pin7 P9OUT cxor! ;
\ gpio interaction routines taken from gpio.c of the examples
$00 constant gpio-low-to-high
$01 constant gpio-high-to-low
: gpio-digitize-pin-value ( value -- ) 0<> if 1 else 0 then ;
: gpio-set-as-output-pin ( pins port -- ) 
  2dup &pasel0 cbic!
  2dup &pasel1 cbic!
  &padir cbis! ;
: gpio-set-as-input-pin ( pins port -- )
  2dup &pasel0 cbic!
  2dup &pasel1 cbic!
  2dup &padir cbic!
  &paren cbic! ;
: gpio-set-output-high-on-pin ( pins port -- ) &paout cbis! ;
: gpio-set-output-low-on-pin ( pins port -- ) &paout cbic! ;
: gpio-toggle-output-on-pin ( pins port -- ) &paout cxor! ;
: gpio-set-as-input-pin-with-pull-down-resistor ( pins port -- ) 
  2dup &pasel0 cbic! 
  2dup &pasel1 cbic!
  2dup &padir  cbic!
  2dup &paren  cbis!
  &paout  cbic!
  ;
: gpio-set-as-input-pin-with-pull-up-resistor ( pins port -- ) 
  2dup &pasel0 cbic! 
  2dup &pasel1 cbic!
  2dup &padir  cbic!
  2dup &paren  cbis!
  &paout  cbis!  ;

: gpio-input-pin@ ( port -- v ) &pain c@ ;
: gpio-iv@ ( port -- v ) &paiv c@ ;
: gpio-enable-interrupt ( pins port -- ) &paie cbis! ;
: gpio-disable-interrupt ( pins port -- ) &paie cbic! ;
: gpio-interrupt-status@ ( pins port -- v ) &paifg c@ and ;  
: gpio-clear-interrupt ( pins port -- ) &paifg cbic! ;
: gpio-select-interrupt-edge ( edge pins port -- )
  &paies ( edge pins port.ies )
  rot ( pins port.ies edge ) 
  0= 
  if \ low to high
  	cbic! 
  else  \ high to low
  	cbis!
  then ;
\ button s1 is mapped to p1.1
pin1 constant button-s1
pin2 constant button-s2
$4 constant button-s1-iv
$6 constant button-s2-iv
button-s1 button-s2 or constant buttons-s1-s2
  \ taken from the outofbox example for msp430fr6989
: configure-button ( edge pins port -- )
  2dup 2>r
  gpio-select-interrupt-edge 
  2r> 
  2dup gpio-set-as-input-pin-with-pull-up-resistor
  2dup gpio-clear-interrupt
  gpio-enable-interrupt ;
  
: configure-button-s1 ( -- ) gpio-high-to-low pin1 port1 configure-button ;
: configure-button-s2 ( -- ) gpio-high-to-low pin2 port1 configure-button ;
: configure-buttons ( -- ) configure-button-s1 configure-button-s2 ;
: button-s1-pressed? ( ifg -- f ) button-s1-iv = ;
: button-s2-pressed? ( ifg -- f ) button-s2-iv = ;

: buttons-toggle-leds ( -- ) 
  \ simple routine that can be easily installed to an ISR 
  \ It's objective is to toggle leds when buttons are pressed
  port1 gpio-iv@ dup 
  button-s1-pressed? if led1-toggle then
  button-s2-pressed? if led2-toggle then
  buttons-s1-s2 port1 gpio-clear-interrupt ;
: demo-led ( -- )
  lcd-init
  led-init
  configure-buttons
  led1-off
  led2-off
  s" demo" typelcd
  ['] buttons-toggle-leds irq-port1 !
  eint ;
: l ( -- ) 0 parse typelcd ;
\ iris routines and such
compiletoflash
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
  2>r ( value mask num-words )
  >r and r> lshift s>d 
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
  rot
  >r \ stash the lower half of the number to the return stack
  text& ( upper addr-hi addr-lo ) 
  2>r \ stash the lower address
  x!  \ stash the upper half to memory
  2r> 
  r> \ restore the lo address followed by lower half of the value
  -rot x! \ move lower half to the proper location and store
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


  
: print-registers ( -- )
  save-base
  decimal
  ." Registers" cr
  CoreRegisterCount 0 do
    ." Register r" i u. ." : " i register@ hex. cr
  loop
  restore-base ;
  

\ todo allocate data structures
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
: def3arg <builds , does> 
  3arg-begin rot 
  @ execute 
  3arg-end ;
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

compiletoram

