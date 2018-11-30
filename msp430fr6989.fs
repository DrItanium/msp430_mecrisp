compiletoflash
\ missing functions that I think are really neat
\ addresses taken from data sheets
: bit-set? ( value bit -- f ) and 0<> ;
$00 constant gpio:low-to-high
$01 constant gpio:high-to-low
$200 constant port1
$201 constant port2
$220 constant port3
$221 constant port4
$240 constant port5
$241 constant port6
$260 constant port7
$261 constant port8
$280 constant port9
$281 constant port10
$80 constant pin7
$40 constant pin6
$20 constant pin5
$10 constant pin4
$08 constant pin3
$04 constant pin2
$02 constant pin1
$01 constant pin0
\ button s1 is mapped to p1.1
pin1 constant button1-pin
pin2 constant button2-pin
port1 constant button1-port
port1 constant button2-port
$4 constant button1-iv
: button-s1-pressed? ( ifg -- f ) button1-iv = ;
$6 constant button2-iv
: button-s2-pressed? ( ifg -- f ) button2-iv = ;
button1-pin button2-pin or constant buttons-pins
port1 constant buttons-port

\ led constants 
port1 constant led1-port
pin0 constant led1-pin
port9 constant led2-port
pin7 constant led2-pin

: gpio:digitize-pin-value ( value -- ) 0<> if 1 else 0 then ;
\ input
: &pain   ( port-base -- addr ) 1-foldable ;
: gpio:input-pin@ ( port -- v ) &pain c@ ;
\ output
: &paout  ( port-base -- addr ) $02 + 1-foldable ;
: gpio:set-output-high-on-pin ( pins port -- ) &paout cbis! ;
: gpio:set-output-low-on-pin ( pins port -- ) &paout cbic! ;
: gpio:toggle-output-on-pin ( pins port -- ) &paout cxor! ;
\ direction
: &padir  ( port-base -- addr ) $04 + 1-foldable ;
: gpio:set-port-direction-input ( pins port -- ) &padir cbic! ;
: gpio:set-port-direction-output ( pins port -- ) &padir cbis! ;
: gpio:toggle-port-direction ( pins port -- ) &padir cxor! ;
\ resistor enable
: &paren  ( port-base -- addr ) $06 + 1-foldable ;
: gpio:enable-port-resistor ( pins port -- ) &paren cbis! ;
: gpio:disable-port-resistor ( pins port -- ) &paren cbic! ;
: gpio:toggle-port-resistor ( pins port -- ) &paren cxor! ;
\ selection 0
: &pasel0 ( port-base -- addr ) $0a + 1-foldable ;
: gpio:set-port-selector0-high ( pins port -- ) &pasel0 cbis! ;
: gpio:set-port-selector0-low ( pins port -- ) &pasel0 cbic! ;
: gpio:toggle-port-selector0 ( pins port -- ) &pasel0 cxor! ;
\ selection 1
: &pasel1 ( port-base -- addr ) $0c + ;
: gpio:set-port-selector1-high ( pins port -- ) &pasel1 cbis! ;
: gpio:set-port-selector1-low ( pins port -- ) &pasel1 cbic! ;
: gpio:toggle-port-selector1 ( pins port -- ) &pasel1 cxor! ;
\ interrupt vector word
: &paiv   ( port-base -- addr ) $0e + ;
: gpio:iv@ ( port -- v ) &paiv c@ ;
\ complement selection
: &paselc ( port-base -- addr ) $16 + ;
\ interrupt edge select
: &paies  ( port-base -- addr ) $18 + ;
: gpio:set-interrupt-edge-low-to-high ( pins port -- ) &paies cbic! ;
: gpio:set-interrupt-edge-high-to-low ( pins port -- ) &paies cbis! ;
: gpio:toggle-interrupt-edge ( pins port -- ) &paies cxor! ;
: gpio:select-interrupt-edge ( edge pins port -- )
  rot ( pins port.ies edge ) 
  gpio:low-to-high = 
  if \ low to high
    gpio:set-interrupt-edge-low-to-high
  else  \ high to low
    gpio:set-interrupt-edge-high-to-low
  then ;
  \ taken from the outofbox example for msp430fr6989
\ interrupt enable
: &paie   ( port-base -- addr ) $1a + ;
: gpio:enable-interrupt ( pins port -- ) &paie cbis! ;
: gpio:disable-interrupt ( pins port -- ) &paie cbic! ;
: gpio:toggle-interrupt ( pins port -- ) &paie cxor! ;
\ interrupt flag
: &paifg  ( port-base -- addr ) $1c + ;
: gpio:interrupt-status@ ( pins port -- v ) &paifg c@ and ;  
: gpio:clear-interrupt ( pins port -- ) &paifg cbic! ;
: gpio:set-interrupt ( pins port -- ) &paifg cbis! ;
\ interact with buttons and such
\ taken from the blinky examples
: init-led1 ( -- ) led1-pin led1-port gpio:set-port-direction-output ;
: init-led2 ( -- ) led2-pin led2-port gpio:set-port-direction-output ;
: led-init ( -- ) init-led1 init-led2 ;
: led1-off ( -- ) led1-pin led1-port gpio:set-output-low-on-pin ;
: led2-off ( -- ) led2-pin led2-port gpio:set-output-low-on-pin ;
: led1-toggle ( -- ) led1-pin led1-port gpio:toggle-output-on-pin ;
: led2-toggle ( -- ) led2-pin led2-port gpio:toggle-output-on-pin ;
\ gpio interaction routines taken from gpio.c of the examples
: gpio:set-as-output-pin ( pins port -- ) 
  2dup gpio:set-port-selector0-low
  2dup gpio:set-port-selector1-low
  gpio:set-port-direction-output ;
: gpio:set-as-input-pin ( pins port -- )
  2dup gpio:set-port-selector0-low
  2dup gpio:set-port-selector1-low
  2dup gpio:set-port-direction-input 
  gpio:disable-port-resistor ;
: gpio:set-as-input-pin-with-pull-down-resistor ( pins port -- ) 
  2dup gpio:set-port-selector0-low
  2dup gpio:set-port-selector1-low
  2dup gpio:set-port-direction-input 
  2dup gpio:enable-port-resistor
  gpio:set-output-low-on-pin 
  ;
: gpio:set-as-input-pin-with-pull-up-resistor ( pins port -- ) 
  2dup gpio:set-port-selector0-low
  2dup gpio:set-port-selector1-low
  2dup gpio:set-port-direction-input 
  2dup gpio:enable-port-resistor
  gpio:set-output-high-on-pin ;

: button-init ( edge pins port -- )
  2dup 2>r gpio:select-interrupt-edge 2r> 
  2dup gpio:set-as-input-pin-with-pull-up-resistor
  2dup gpio:clear-interrupt
  gpio:enable-interrupt ;

: button-init-s1 ( -- ) 
  gpio:high-to-low button1-pin button1-port button-init ;
: button-init-s2 ( -- ) 
  gpio:high-to-low button2-pin button2-port button-init ;
: buttons-init ( -- ) 
  button-init-s1 
  button-init-s2 ;
: buttons-pressed@ ( -- mask ) 
  buttons-port gpio:iv@ ;
: reset-buttons-isr ( -- ) 
  buttons-pins buttons-port gpio:clear-interrupt ;

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

: l ( -- ) 0 parse typelcd ;
: core:sysinit ( -- )
  lcd-init
  led-init
  buttons-init
  led1-off
  led2-off
  ;
\ hardware multiplier support, taken from the datasheet
$04c0 constant HWMULT_BASE
compiletoram 
: defhwmulreg ( offset -- ) 
  compiletoflash
  HWMULT_BASE + constant 
  compiletoram ;
$00 defhwmulreg HWMULT_MPY \ 16-bit operand 1 - multiply
$02 defhwmulreg HWMULT_MPYS \ 16-bit operand 1 - signed multiply
$04 defhwmulreg HWMULT_MAC \ 16-bit operand 1 - multiply accumulate
$06 defhwmulreg HWMULT_MACS \ 16-bit operand 1 - signed multiply accumulate
$08 defhwmulreg HWMULT_OP2  \ 16-bit operand 2 
$0A defhwmulreg HWMULT_RESLO \ 16 x 16 result low word
$0C defhwmulreg HWMULT_RESHI \ 16 x 16 result high word
$0E defhwmulreg HWMULT_SUMEXT \ 16 x 16 sum extension
$10 defhwmulreg HWMULT_MPY32L \ 32-bit operand 1 - multiply low word
$12 defhwmulreg HWMULT_MPY32H \ 32-bit operand 1 - multiply high word
$14 defhwmulreg HWMULT_MPYS32L \ 32-bit operand 1 - signed multiply low word
$16 defhwmulreg HWMULT_MPYS32H \ 32-bit operand 1 - signed multiply high word
$18 defhwmulreg HWMULT_MAC32L \ 32-bit operand 1 - multiply accumulate low word
$1A defhwmulreg HWMULT_MAC32H \ 32-bit operand 1 - multiply accumulate high word
$1C defhwmulreg HWMULT_MACS32L \ 32-bit operand 1 - signed multiply accumulate low word
$1E defhwmulreg HWMULT_MACS32H \ 32-bit operand 1 - signed multiply accumulate high word
$20 defhwmulreg HWMULT_OP2L \ 32-bit operand 2 - low word 
$22 defhwmulreg HWMULT_OP2H \ 32-bit operand 2 - high word
$24 defhwmulreg HWMULT_RES0 \ 32 x 32 result 0 - least significant word
$26 defhwmulreg HWMULT_RES1 \ 32 x 32 result 1 
$28 defhwmulreg HWMULT_RES2 \ 32 x 32 result 2
$2A defhwmulreg HWMULT_RES3 \ 32 x 32 result 3 - most significant word
$2C defhwmulreg HWMULT_MPY32CTL0 \ MPY32 control 0

compiletoflash 
: hwmult:16x16accum-clear ( -- ) 
  0 HWULT_RESLO !
  0 HWULT_RESHI ! ;
: hwmult:32x32accum-clear ( -- ) 
  0 HWMULT_RES0 ! 
  0 HWMULT_RES1 ! 
  0 HWMULT_RES2 ! 
  0 HWMULT_RES3 ! ;
: hwmult:16x16accum ( -- d ) 
  HWMULT_RESLO @
  HWMULT_RESHI @ ;
: hwmult:32x32accum-low-dword ( -- d )
  HWMULT_RES0 @
  HWMULT_RES1 @ ;
: hwmult:32x32accum-high-dword ( -- d )
  HWMULT_RES2 @
  HWMULT_RES3 @ ;
: hwmult:32x32accum ( -- dlo dhi )
  hwmult:32x32accum-low-dword
  hwmult:32x32accum-high-dword ;
: hwmult:16x16sum-ext ( -- se ) HWMULT_SUMEXT @ ;
: hwmult:def_16bit_operation ( operation "name" -- )
  <builds , 
  does> ( a b -- d ) 
  @ ! 
  HWMULT_OP2 ! 
  hwmult:16x16accum ;
HWMULT_MPY hwmult:def_16bit_operation umac 
HWMULT_MPYS hwmult:def_16bit_operation mac 
HWMULT_MAC hwmult:def_16bit_operation su*>d 
HWMULT_MACS hwmult:def_16bit_operation s*>d 

: hwmult:def_32bit_operation_out64 ( opl oph "name" -- )
  <builds , , 
  does> ( d0 d1 -- q )
  -rot ( d0 addr d1 )
  HWMULT_OP2H !
  HWMULT_OP2L !
  tuck ( d0l addr d0h addr )
  @ ! 
  cell+ 
  @ !
  hwmult:32x32accum ;
HWMULT_MPY32L HWMULT_MPY32H hwmult:def_32bit_operation_out64 o32*o32->o64
HWMULT_MPYS32L HWMULT_MPYS32H hwmult:def_32bit_operation_out64 i32*i32->i64
HWMULT_MAC32L HWMULT_MAC32H hwmult:def_32bit_operation_out64 o32*o32+o64->o64
HWMULT_MACS32L HWMULT_MACS32H hwmult:def_32bit_operation_out64 i32*i32+i64->i64
: d* ( d0 d1 -- d ) i32*i32->i64 2drop ;
: ud* ( d0 d1 -- d ) o32*o32->o64 2drop ;
: d*>q ( d0 d1 -- q ) i32*i32->i64 ;
: ud*>q ( d0 d1 -- q ) o32*o32->o64 ;
: d*+ ( d0 d1 -- q ) i32*i32+i64->i64 2drop ;
: ud*+ ( d0 d1 -- q ) o32*o32+o64->o64 2drop ;

compiletoram
