\ use external buttons from arduino kits
compiletoflash 

PIN3 constant button3-pin
$8 constant button3-iv
buttons-pins @ button3-pin or buttons-pins !

: init-button-s3 ( -- ) 1 button3-pin button-port button-init ;
: init-buttons ( -- ) init-buttons init-button-s3 ;
: button-s3-pressed? ( ifg -- flag ) button3-iv button-pressed? ;

compiletoram
