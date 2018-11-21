\ depends on msp430fr6989.fs
compiletoflash
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