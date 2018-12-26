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

: pin-set? ( input pin -- f ) bit-set? ;
: pin-clear? ( input pin -- f ) bit-clear? ;
: portin@ ( port -- pin-status ) PORTIN + c@ ;
: portout@ ( port -- pin-status ) PORTOUT + c@ ;
: portdir@ ( port -- pin-status ) PORTDIR + c@ ;
: portren@ ( port -- pin-status ) PORTREN + c@ ;
: portsel0@ ( port -- pin-status ) PORTSEL0 + c@ ;
: portsel1@ ( port -- pin-status ) PORTSEL1 + c@ ;
: portsel@ ( port -- pin-status0 pin-status1 ) 
		   dup ( port port )
		   portsel0@ swap ( pin-status0 port )
	 	   portsel1@ ; 
: porties@ ( port -- pin-status ) PORTIES + c@ ;
: portie@ ( port -- pin-status ) PORTIE + c@ ;
: portifg@ ( port -- pin-status ) PORTIFG + c@ ;
\ hardware peripherals that are known to be there on the devboard
\ button constants
PIN1 constant button1-pin
PIN2 constant button2-pin
PORT1 constant button1-port
PORT1 constant button2-port

\ led constants 
PORT1 constant led1-port
PIN0 constant led1-pin
PORT9 constant led2-port
PIN7 constant led2-pin


$4 constant button1-iv
: button-s1-pressed? ( ifg -- f ) button1-iv = ;
$6 constant button2-iv
: button-s2-pressed? ( ifg -- f ) button2-iv = ;
button1-pin button2-pin or constant buttons-pins
port1 constant buttons-port


\ : gpio:digitize-pin-value ( value -- ) 0<> if 1 else 0 then ;
\ input
\ : &pain   ( port-base -- addr ) 1-foldable ;
\ : gpio:input-pin@ ( port -- v ) &pain c@ ;
\ output
\ : &paout  ( port-base -- addr ) $02 + 1-foldable ;
\ : gpio:set-output-high-on-pin ( pins port -- ) &paout cbis! ;
\ : gpio:set-output-low-on-pin ( pins port -- ) &paout cbic! ;
\ : gpio:toggle-output-on-pin ( pins port -- ) &paout cxor! ;
\ direction
\ : &padir  ( port-base -- addr ) $04 + 1-foldable ;
\ : gpio:set-port-direction-input ( pins port -- ) &padir cbic! ;
\ : gpio:set-port-direction-output ( pins port -- ) &padir cbis! ;
\ : gpio:toggle-port-direction ( pins port -- ) &padir cxor! ;
\ \ resistor enable
\ : &paren  ( port-base -- addr ) $06 + 1-foldable ;
\ : gpio:enable-port-resistor ( pins port -- ) &paren cbis! ;
\ : gpio:disable-port-resistor ( pins port -- ) &paren cbic! ;
\ : gpio:toggle-port-resistor ( pins port -- ) &paren cxor! ;
\ \ selection 0
\ : &pasel0 ( port-base -- addr ) $0a + 1-foldable ;
\ : gpio:set-port-selector0-high ( pins port -- ) &pasel0 cbis! ;
\ : gpio:set-port-selector0-low ( pins port -- ) &pasel0 cbic! ;
\ : gpio:toggle-port-selector0 ( pins port -- ) &pasel0 cxor! ;
\ \ selection 1
\ : &pasel1 ( port-base -- addr ) $0c + ;
\ : gpio:set-port-selector1-high ( pins port -- ) &pasel1 cbis! ;
\ : gpio:set-port-selector1-low ( pins port -- ) &pasel1 cbic! ;
\ : gpio:toggle-port-selector1 ( pins port -- ) &pasel1 cxor! ;
\ \ interrupt vector word
\ : &paiv   ( port-base -- addr ) $0e + ;
\ : gpio:iv@ ( port -- v ) &paiv c@ ;
\ \ complement selection
\ : &paselc ( port-base -- addr ) $16 + ;
\ \ interrupt edge select
\ : &paies  ( port-base -- addr ) $18 + ;
\ : gpio:set-interrupt-edge-low-to-high ( pins port -- ) &paies cbic! ;
\ : gpio:set-interrupt-edge-high-to-low ( pins port -- ) &paies cbis! ;
\ : gpio:toggle-interrupt-edge ( pins port -- ) &paies cxor! ;
\ : gpio:select-interrupt-edge ( edge pins port -- )
\   rot ( pins port.ies edge ) 
\   gpio:low-to-high = 
\   if \ low to high
\     gpio:set-interrupt-edge-low-to-high
\   else  \ high to low
\     gpio:set-interrupt-edge-high-to-low
\   then ;
\   \ taken from the outofbox example for msp430fr6989
\ \ interrupt enable
\ : &paie   ( port-base -- addr ) $1a + ;
\ : gpio:enable-interrupt ( pins port -- ) &paie cbis! ;
\ : gpio:disable-interrupt ( pins port -- ) &paie cbic! ;
\ : gpio:toggle-interrupt ( pins port -- ) &paie cxor! ;
\ \ interrupt flag
\ : &paifg  ( port-base -- addr ) $1c + ;
\ : gpio:interrupt-status@ ( pins port -- v ) &paifg c@ and ;  
\ : gpio:clear-interrupt ( pins port -- ) &paifg cbic! ;
\ : gpio:set-interrupt ( pins port -- ) &paifg cbis! ;
\ \ interact with buttons and such
\ \ taken from the blinky examples
\ : init-led1 ( -- ) led1-pin led1-port gpio:set-port-direction-output ;
\ : init-led2 ( -- ) led2-pin led2-port gpio:set-port-direction-output ;
\ : led-init ( -- ) init-led1 init-led2 ;
\ : led1-off ( -- ) led1-pin led1-port gpio:set-output-low-on-pin ;
\ : led2-off ( -- ) led2-pin led2-port gpio:set-output-low-on-pin ;
\ : led1-toggle ( -- ) led1-pin led1-port gpio:toggle-output-on-pin ;
\ : led2-toggle ( -- ) led2-pin led2-port gpio:toggle-output-on-pin ;
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
