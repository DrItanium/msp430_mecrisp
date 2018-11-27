\ depends on msp430fr6989.fs
\ depends on common.fs
\ depends on iris_opcodes.fs
\ depends on iris.fs
compiletoflash

\ debugging features

: iris:button-handlers ( -- ) 
  \ edit this word to 
  buttons-pressed@ dup
  button-s1-pressed? 
  if 
     halt-execution
	 ." Halting Execution" cr
  then
  button-s2-pressed?
  if 
     toggle-debug
     ." Toggling debugging " debug[ ." on" else ." off" ]debug cr
  then
  reset-buttons-isr ;

: iris:sysinit ( -- )
  lcd-init
  led-init
  configure-buttons
  led1-off
  led2-off
  s" iris" typelcd
  \ setup the button handlers
  ['] iris:button-handlers irq-port1 !
  eint
  iris:init-core ;
: iris:sysdown ( -- )
  iris:shutdown-core ;

: iris:decode-nil ( instruction -- s2 s1 d op )
  quarter
  debug[ dup ." Opcode: " hex. cr ]debug ;
: iris:dispatch-nil ( s2 s1 d op -- )
  2drop 2drop ;
\ double indirect dispatch to prevent constant fram erasure as it's time consuming
['] iris:decode-nil variable CoreDecodeMethod
['] iris:dispatch-nil variable CoreDispatchMethod
: iris:dispatch ( s2 s1 d op -- ) CoreDispatchMethod @ execute ;
: iris:decode ( instruction -- s2 s1 d op ) CoreDecodeMethod @ execute ;
: iris:execution-loop ( -- ) 
  resume-execution
  begin 
    ip@ 
    debug[ dup u.lcd ]debug
    text@ 
    iris:decode ( s2 s1 d op )
    iris:dispatch
    increment-next? if ip1+ then
    yes-increment-next
    executing? not
  until
  ;


compiletoram

