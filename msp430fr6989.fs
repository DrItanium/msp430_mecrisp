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

: core:sysinit ( -- )
  lcd-init
  led-init
  buttons-init
  led1-off
  led2-off
  ;



compiletoram
