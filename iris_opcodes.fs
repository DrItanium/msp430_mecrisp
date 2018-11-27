\ depends on common.fs and msp430fr6989.fs
compiletoflash
\ instruction opcodes 

create iris:dispatch-table
['] op:illegal ,
\ keep installing more operations here

compiletoram
