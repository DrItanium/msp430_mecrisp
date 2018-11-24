\ depends on msp430fr6989.fs
compiletoflash

: buttons-toggle-leds ( -- ) 
  \ simple routine that can be easily installed to an ISR 
  \ It's objective is to toggle leds when buttons are pressed
  buttons-pressed@ dup
  button-s1-pressed? if led1-toggle then
  button-s2-pressed? if led2-toggle then
  reset-buttons-isr ;

: demo-led ( -- )
  lcd-init
  led-init
  configure-buttons
  led1-off 
  led2-off
  s" demo" typelcd
  ['] buttons-toggle-leds irq-port1 !
  eint ;

compiletoram
