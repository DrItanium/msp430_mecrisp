\ provides access to the hardware multiplier for double nums
\ hardware multiplier support, taken from the datasheet
$04c0 constant HWMULT_BASE
compiletoram 
: defhwmulreg ( offset -- ) 
  compiletoflash
  HWMULT_BASE + constant 
  compiletoram ;
$00 defhwmulreg HWMULT_MPY \ 16-bit operand 1 - multiply
$02 defhwmulreg HWMULT_MPYS \ 16-bit operand 1 - signed multiply
$04 defhwmulreg HWMULT_MAC \ 16-bit operand 1 - multiply accumulate
$06 defhwmulreg HWMULT_MACS \ 16-bit operand 1 - signed multiply accumulate
$08 defhwmulreg HWMULT_OP2  \ 16-bit operand 2 
$0A defhwmulreg HWMULT_RESLO \ 16 x 16 result low word
$0C defhwmulreg HWMULT_RESHI \ 16 x 16 result high word
$0E defhwmulreg HWMULT_SUMEXT \ 16 x 16 sum extension
$10 defhwmulreg HWMULT_MPY32L \ 32-bit operand 1 - multiply low word
$12 defhwmulreg HWMULT_MPY32H \ 32-bit operand 1 - multiply high word
$14 defhwmulreg HWMULT_MPYS32L \ 32-bit operand 1 - signed multiply low word
$16 defhwmulreg HWMULT_MPYS32H \ 32-bit operand 1 - signed multiply high word
$18 defhwmulreg HWMULT_MAC32L \ 32-bit operand 1 - multiply accumulate low word
$1A defhwmulreg HWMULT_MAC32H \ 32-bit operand 1 - multiply accumulate high word
$1C defhwmulreg HWMULT_MACS32L \ 32-bit operand 1 - signed multiply accumulate low word
$1E defhwmulreg HWMULT_MACS32H \ 32-bit operand 1 - signed multiply accumulate high word
$20 defhwmulreg HWMULT_OP2L \ 32-bit operand 2 - low word 
$22 defhwmulreg HWMULT_OP2H \ 32-bit operand 2 - high word
$24 defhwmulreg HWMULT_RES0 \ 32 x 32 result 0 - least significant word
$26 defhwmulreg HWMULT_RES1 \ 32 x 32 result 1 
$28 defhwmulreg HWMULT_RES2 \ 32 x 32 result 2
$2A defhwmulreg HWMULT_RES3 \ 32 x 32 result 3 - most significant word
$2C defhwmulreg HWMULT_MPY32CTL0 \ MPY32 control 0

compiletoflash 
: hwmult:16x16accum-clear ( -- ) 
  0 HWMULT_RESLO !
  0 HWMULT_RESHI ! ;
: hwmult:32x32accum-clear ( -- ) 
  0 HWMULT_RES0 ! 
  0 HWMULT_RES1 ! 
  0 HWMULT_RES2 ! 
  0 HWMULT_RES3 ! ;
: hwmult:16x16accum ( -- d ) 
  HWMULT_RESLO @
  HWMULT_RESHI @ ;
: hwmult:32x32accum-low-dword ( -- d )
  HWMULT_RES0 @
  HWMULT_RES1 @ ;
: hwmult:32x32accum-high-dword ( -- d )
  HWMULT_RES2 @
  HWMULT_RES3 @ ;
: hwmult:32x32accum ( -- dlo dhi )
  hwmult:32x32accum-low-dword
  hwmult:32x32accum-high-dword ;
: hwmult:16x16sum-ext ( -- se ) HWMULT_SUMEXT @ ;
: hwmult:def_16bit_operation ( operation "name" -- )
  <builds , 
  does> ( a b -- d ) 
  @ ! 
  HWMULT_OP2 ! 
  hwmult:16x16accum ;
HWMULT_MPY hwmult:def_16bit_operation umac 
HWMULT_MPYS hwmult:def_16bit_operation mac 
HWMULT_MAC hwmult:def_16bit_operation su*>d 
HWMULT_MACS hwmult:def_16bit_operation s*>d 

: hwmult:def_32bit_operation_out64 ( opl oph "name" -- )
  <builds , , 
  does> ( d0 d1 -- q )
  -rot ( d0 addr d1 )
  HWMULT_OP2H !
  HWMULT_OP2L !
  tuck ( d0l addr d0h addr )
  @ ! 
  cell+ 
  @ !
  hwmult:32x32accum ;
HWMULT_MPY32L HWMULT_MPY32H hwmult:def_32bit_operation_out64 o32*o32->o64
HWMULT_MPYS32L HWMULT_MPYS32H hwmult:def_32bit_operation_out64 i32*i32->i64
HWMULT_MAC32L HWMULT_MAC32H hwmult:def_32bit_operation_out64 o32*o32+o64->o64
HWMULT_MACS32L HWMULT_MACS32H hwmult:def_32bit_operation_out64 i32*i32+i64->i64
: d* ( d0 d1 -- d ) i32*i32->i64 2drop ;
: ud* ( d0 d1 -- d ) o32*o32->o64 2drop ;
: d*>q ( d0 d1 -- q ) i32*i32->i64 ;
: ud*>q ( d0 d1 -- q ) o32*o32->o64 ;
: d*+ ( d0 d1 -- q ) i32*i32+i64->i64 2drop ;
: ud*+ ( d0 d1 -- q ) o32*o32+o64->o64 2drop ;
