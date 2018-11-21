compiletoflash
\ missing functions that I think are really neat
\ addresses taken from data sheets
: &pasel0 ( port-base -- addr ) $0a + ;
: &pasel1 ( port-base -- addr ) $0c + ;
: &padir  ( port-base -- addr ) $04 + ;
: &paren  ( port-base -- addr ) $06 + ;
: &pain   ( port-base -- addr ) ;
: &paout  ( port-base -- addr ) $02 + ;
: &paiv   ( port-base -- addr ) $0e + ;
: &paselc ( port-base -- addr ) $16 + ;
: &paies  ( port-base -- addr ) $18 + ;
: &paie   ( port-base -- addr ) $1a + ;
: &paifg  ( port-base -- addr ) $1c + ;
$200 constant P1BASE
$201 constant P2BASE
$220 constant P3BASE
$221 constant P4BASE
$240 constant P5BASE
$241 constant P6BASE
$260 constant P7BASE
$261 constant P8BASE
$280 constant P9BASE
$281 constant P10BASE
$80 constant Pin7
$40 constant Pin6
$20 constant Pin5
$10 constant Pin4
$08 constant Pin3
$04 constant Pin2
$02 constant Pin1
$01 constant Pin0
compiletoram
: def-port-regs ( base "constants" -- ) 
  compiletoflash
  dup constant \ port-id
  dup &pain constant 
  dup &paout constant 
  dup &padir constant 
  dup &paren constant 
  dup &pasel0 constant
  dup &pasel1 constant
  dup &paiv constant 
  dup &paselc constant 
  dup &paies constant
  dup &paie constant
  dup &paifg constant 
  compiletoram
  drop ;
p1base def-port-regs port1 p1in p1out p1dir p1ren p1sel0 p1sel1 p1iv p1selc p1ies p1ie p1ifg
p2base def-port-regs port2 p2in p2out p2dir p2ren p2sel0 p2sel1 p2iv p2selc p2ies p2ie p2ifg
p3base def-port-regs port3 p3in p3out p3dir p3ren p3sel0 p3sel1 p3iv p3selc p3ies p3ie p3ifg
p4base def-port-regs port4 p4in p4out p4dir p4ren p4sel0 p4sel1 p4iv p4selc p4ies p4ie p4ifg
p5base def-port-regs port5 p5in p5out p5dir p5ren p5sel0 p5sel1 p5iv p5selc p5ies p5ie p5ifg
p6base def-port-regs port6 p6in p6out p6dir p6ren p6sel0 p6sel1 p6iv p6selc p6ies p6ie p6ifg
p7base def-port-regs port7 p7in p7out p7dir p7ren p7sel0 p7sel1 p7iv p7selc p7ies p7ie p7ifg
p8base def-port-regs port8 p8in p8out p8dir p8ren p8sel0 p8sel1 p8iv p8selc p8ies p8ie p8ifg
p9base def-port-regs port9 p9in p9out p9dir p9ren p9sel0 p9sel1 p9iv p9selc p9ies p9ie p9ifg
p10base def-port-regs port10 p10in p10out p10dir p10ren p10sel0 p10sel1 p10iv p10selc p10ies p10ie p10ifg
compiletoram
