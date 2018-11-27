\ depends on common.fs and msp430fr6989.fs
compiletoflash
\ instruction opcodes 
$00 {constseq
    constseq1+: op:illegal
    constseq1+: op:addi
    constseq1+: op:subi
    constseq1+: op:muli
    constseq1+: op:divi
    constseq1+: op:remi
    constseq1+: op:addo
    constseq1+: op:subo
    constseq1+: op:mulo
    constseq1+: op:divo
    constseq1+: op:remo
    constseq}

compiletoram
