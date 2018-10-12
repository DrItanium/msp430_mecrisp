\ extra routines for my purposes
compiletoflash
\ addresses taken from data sheets
$200 constant P1IN
$201 constant P2IN
$202 constant P1OUT
$203 constant P2OUT
$204 constant P1DIR
$205 constant P2DIR
$220 constant P3IN
$221 constant P4IN
$222 constant P3OUT
$223 constant P4OUT
$224 constant P3DIR
$225 constant P4DIR
$240 constant P5IN
$241 constant P6IN
$242 constant P5OUT
$243 constant P6OUT
$244 constant P5DIR
$245 constant P6DIR
$260 constant P7IN
$261 constant P8IN
$262 constant P7OUT
$263 constant P8OUT
$264 constant P7DIR
$265 constant P8DIR
$280 constant P9IN
$281 constant P10IN
$282 constant P9OUT
$283 constant P10OUT
$284 constant P9DIR
$285 constant P10DIR
$80 constant Function7
$40 constant Function6
$20 constant Function5
$10 constant Function4
$08 constant Function3
$04 constant Function2
$02 constant Function1
$01 constant Function0


\ taken from the blinky examples
: led2-enable ( -- ) Function7 P9DIR c! ;
: led1-enable ( -- ) Function0 P1DIR c! ;
: pin-toggle ( func addr -- ) cxor! ;
: led1-toggle ( -- ) Function0 P1OUT pin-toggle ;
: led2-toggle ( -- ) Function7 P9OUT pin-toggle ;
compiletoram
