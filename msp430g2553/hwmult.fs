\ msp430x2xx hardware multiplier interface

compiletoflash
$0130 constant HWMULT_BASE
compiletoram
: defmultreg ( offset -- )
  compiletoflash
  HWMULT_BASE + constant
  compiletoram ;
$00 defmultreg HWMULT_MPY
$02 defmultreg HWMULT_MPYS
$04 defmultreg HWMULT_MAC
$06 defmultreg HWMULT_MACS
$08 defmultreg HWMULT_OP2
$0A defmultreg HWMULT_RESLO
$0C defmultreg HWMULT_RESHI
$0E defmultreg HWMULT_SUMEXT
compiletoflash 
: c* ( a b -- v ) 
  dint \ temporarily disable interrupts
  nop
  HWMULT_MPYS c!
  HWMULT_OP2 c!
  nop \ make sure to give the hardware more time
  HWMULT_RESLO c@ 
  eint nop ;
: uc* ( a b -- v ) 
  dint \ temporarily disable interrupts
  nop
  HWMULT_MPY c!
  HWMULT_OP2 c!
  nop \ make sure to give the hardware more time
  HWMULT_RESLO c@ 
  eint ;
: c*>s ( a b -- s )
  dint 
  nop
  HWMULT_MPYS c!
  HWMULT_OP2 c!
  nop
  HWMULT_RESLO @
  eint ;
: uc*>s ( a b -- s )
  dint 
  nop
  HWMULT_MPY c!
  HWMULT_OP2 c!
  nop
  HWMULT_RESLO @
  eint ;
: s* ( a b -- v )
  dint
  nop
  HWMULT_MPYS !
  HWMULT_OP2 !
  nop
  HWMULT_RESLO @
  eint ;
: us* ( a b -- v )
  dint
  nop
  HWMULT_MPY !
  HWMULT_OP2 !
  nop
  HWMULT_RESLO @
  eint ;

: s*>d ( a b -- d )
  dint
  nop
  HWMULT_MPYS !
  HWMULT_OP2 !
  nop
  HWMULT_RESLO @
  HWMULT_RESHI @
  eint ;
: us*>d ( a b -- d )
  dint
  nop
  HWMULT_MPY !
  HWMULT_OP2 !
  nop
  HWMULT_RESLO @
  HWMULT_RESHI @
  eint ;

compiletoram
