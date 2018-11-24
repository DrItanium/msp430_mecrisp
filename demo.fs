\ depends on msp430fr6989.fs
compiletoflash

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
