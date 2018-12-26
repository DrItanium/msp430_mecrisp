\ Digital IO interaction
compiletoflash
: bit-set? ( value bit -- f ) and 0<> ;
: bit-clear? ( value bit -- f ) and 0= ;
$200 constant PORT_BASE
PORT_BASE constant PORT1
PORT1 1+ constant PORT2
PORT_BASE $20 + constant PORT3
PORT3 1+ constant PORT4
PORT_BASE $40 + constant PORT5
PORT5 1+ constant PORT6
PORT_BASE $60 + constant PORT7
PORT7 1+ constant PORT8
PORT_BASE $80 + constant PORT9
PORT9 1+ constant PORT10

$80 constant PIN7
$40 constant PIN6
$20 constant PIN5
$10 constant PIN4
$08 constant PIN3
$04 constant PIN2
$02 constant PIN1
$01 constant PIN0
$00 constant PORTIN
$02 constant PORTOUT
$04 constant PORTDIR
$06 constant PORTREN
$0A constant PORTSEL0
$0C constant PORTSEL1
$16 constant PORTSELC
$18 constant PORTIES
$1A constant PORTIE
$1C constant PORTIFG
$0E constant PORTIV
$0E constant PORTIV_L
$0F constant PORTIV_H
PORTIV constant P1IV
PORTIV_L constant P1IV_L
PORTIV_H constant P1IV_H
PORTIV $10 + constant P2IV
PORTIV_L $10 + constant P2IV_L
PORTIV_H $10 + constant P2IV_H
PORTIV $20 + constant P3IV
PORTIV_L $20 + constant P3IV_L
PORTIV_H $20 + constant P3IV_H
PORTIV $30 + constant P4IV
PORTIV_L $30 + constant P4IV_L
PORTIV_H $30 + constant P4IV_H

: pin-set? ( pin input -- f ) bit-set? ;
: pin-clear? ( pin input -- f ) bit-clear? ;
: portin@ ( port -- pin-status ) PORTIN + c@ ;
: portout@ ( port -- pin-status ) PORTOUT + c@ ;
: portout-hi! ( pins port -- ) PORTOUT + cbis! ;
: portout-lo! ( pins port -- ) PORTOUT + cbic! ;
: portout-toggle! ( pins port -- ) PORTOUT + cxor! ;
: portdir@ ( port -- pin-status ) PORTDIR + c@ ;
: portdir-in! ( pins port -- ) PORTDIR + cbic! ;
: portdir-out! ( pins port -- ) PORTDIR + cbis! ;
: portdir-toggle! ( pins port -- ) PORTDIR + cxor! ;
: portren@ ( port -- pin-status ) PORTREN + c@ ;
: portren-enable! ( pins port -- ) PORTREN + cbis! ;
: portren-disable! ( pins port -- ) PORTREN + cbic! ;
: portren-toggle! ( pins port -- ) PORTREN + cxor! ;
: portsel0@ ( port -- pin-status ) PORTSEL0 + c@ ;
: portsel0-hi! ( pins port -- ) PORTSEL0 + cbis! ;
: portsel0-lo! ( pins port -- ) PORTSEL0 + cbic! ;
: portsel0-toggle! ( pins port -- ) PORTSEL0 + cxor! ;
: portsel1@ ( port -- pin-status ) PORTSEL1 + c@ ;
: portsel1-hi! ( pins port -- ) PORTSEL1 + cbis! ;
: portsel1-lo! ( pins port -- ) PORTSEL1 + cbic! ;
: portsel1-toggle! ( pins port -- ) PORTSEL1 + cxor! ;
: portsel@ ( port -- pin-status0 pin-status1 ) 
		   dup ( port port )
		   portsel0@ swap ( pin-status0 port )
	 	   portsel1@ ; 
: porties@ ( port -- pin-status ) PORTIES + c@ ;
: ifg-low-to-high! ( pins port -- ) PORTIES + cbic! ;
: ifg-high-to-low! ( pins port -- ) PORTIES + cbis! ;
: ifg-toggle-edge! ( pins port -- ) PORTIES + cxor! ;
: ifg-edge! ( edge pins port -- ) 
  rot 0= ( pins port edge ) 
  if 
  	ifg-low-to-high! 
  else
	ifg-high-to-low! 
  then ;
  
: portie@ ( port -- pin-status ) PORTIE + c@ ;
: enable-interrupt! ( pins port -- ) PORTIE + cbis! ;
: disable-interrupt! ( pins port -- ) PORTIE + cbic! ;
: toggle-interrupt! ( pins port -- ) PORTIE + cxor! ;
: portifg@ ( port -- pin-status ) PORTIFG + c@ ;
: clear-interrupt! ( pins port -- ) PORTIFG + cbic! ;
: set-interrupt! ( pins port -- ) PORTIFG + cbis! ;

: dir-input? ( pin port -- f ) portdir@ pin-clear? ;
: dir-output? ( pin port -- f ) portdir@ pin-set? ;
: input-pin? ( pin port -- f )
  2dup ( pin port pin port ) 
  portren@ pin-clear? -rot ( f pin port )
  dir-input? and ;
: input-with-pulldown-resistor? ( pin port -- f )
  2dup 2dup ( pin port pin port pin port )
  dir-input? -rot ( pin port f pin port )
  portren@ pin-set? and -rot ( pin port f )
  portout@ pin-clear? and ;
: input-with-pullup-resistor? ( pin port -- f )
  2dup 2dup ( pin port pin port pin port )
  dir-input?  -rot ( pin port f pin port )
  portren@ pin-set? and -rot 
  portout@ pin-set? and ;
: output-pin? ( pin port -- f ) dir-output? ; 
: gpio-selected? ( pin port -- f ) 
  2dup ( pin port pin port )
  portsel0@ pin-clear? -rot 
  portsel1@ pin-clear? and ;
: primary-module-function-selected? ( pin port -- f ) 
  2dup ( pin port pin port )
  portsel0@ pin-set? -rot 
  portsel1@ pin-clear? and ;
: secondary-module-function-selected? ( pin port -- f ) 
  2dup ( pin port pin port )
  portsel0@ pin-clear? -rot 
  portsel1@ pin-set? and ;
: tertiary-module-function-selected? ( pin port -- f ) 
  2dup ( pin port pin port )
  portsel0@ pin-set? -rot 
  portsel1@ pin-set? and ;
: interrupt-pending? ( pins port -- f ) portifg@ pin-set? ;
: ifg-flag-set-on-low-to-high? ( pins port -- f ) porties@ bit-clear? ;
: ifg-flag-set-on-high-to-low? ( pins port -- f ) porties@ bit-set? ;
: interrupt-enabled? ( pins port -- f ) portie@ pin-set? ;
\ hardware peripherals that are known to be there on the devboard
\ button constants
	
PIN1 constant button1-pin
PIN2 constant button2-pin
button1-pin button2-pin or constant buttons-pins
PORT1 constant button-port

\ led constants 
PORT1 constant led1-port
PIN0 constant led1-pin
PORT9 constant led2-port
PIN7 constant led2-pin

: digitize ( value -- ) 0<> if 1 else 0 then ;


$4 constant button1-iv
$6 constant button2-iv
: button-s1-pressed? ( ifg -- f ) button1-iv = ;
: button-s2-pressed? ( ifg -- f ) button2-iv = ;

: init-led1 ( -- ) led1-pin led1-port portdir-out! ;
: init-led2 ( -- ) led2-pin led2-port portdir-out! ;
: init-leds ( -- ) init-led1 init-led2 ;
: led1-off ( -- ) led1-pin led1-port portout-lo! ;
: led2-off ( -- ) led2-pin led2-port portout-lo! ;
: led1-toggle ( -- ) led1-pin led1-port portout-toggle! ;
: led2-toggle ( -- ) led2-pin led2-port portout-toggle! ;


\ \ interact with buttons and such
\ \ gpio interaction routines taken from gpio.c of the examples
\ : gpio:set-as-output-pin ( pins port -- ) 
\   2dup gpio:set-port-selector0-low
\   2dup gpio:set-port-selector1-low
\   gpio:set-port-direction-output ;
\ : gpio:set-as-input-pin ( pins port -- )
\   2dup gpio:set-port-selector0-low
\   2dup gpio:set-port-selector1-low
\   2dup gpio:set-port-direction-input 
\   gpio:disable-port-resistor ;
\ : gpio:set-as-input-pin-with-pull-down-resistor ( pins port -- ) 
\   2dup gpio:set-port-selector0-low
\   2dup gpio:set-port-selector1-low
\   2dup gpio:set-port-direction-input 
\   2dup gpio:enable-port-resistor
\   gpio:set-output-low-on-pin 
\   ;
\ : gpio:set-as-input-pin-with-pull-up-resistor ( pins port -- ) 
\   2dup gpio:set-port-selector0-low
\   2dup gpio:set-port-selector1-low
\   2dup gpio:set-port-direction-input 
\   2dup gpio:enable-port-resistor
\   gpio:set-output-high-on-pin ;
\ 
\ : button-init ( edge pins port -- )
\   2dup 2>r gpio:select-interrupt-edge 2r> 
\   2dup gpio:set-as-input-pin-with-pull-up-resistor
\   2dup gpio:clear-interrupt
\   gpio:enable-interrupt ;
\ 
\ : button-init-s1 ( -- ) 
\   gpio:high-to-low button1-pin button1-port button-init ;
\ : button-init-s2 ( -- ) 
\   gpio:high-to-low button2-pin button2-port button-init ;
\ : buttons-init ( -- ) 
\   button-init-s1 
\   button-init-s2 ;
\ : buttons-pressed@ ( -- mask ) 
\   buttons-port gpio:iv@ ;
\ : reset-buttons-isr ( -- ) 
\   buttons-pins buttons-port gpio:clear-interrupt ;
compiletoram
