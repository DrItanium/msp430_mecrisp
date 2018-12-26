\ access to the hardware crc32 mechanism

\ crc32 
compiletoflash
$0980 constant CRC32_BASE
compiletoram
: defcrc32reg ( offset -- )
  compiletoflash 
  CRC32_BASE + constant 
  compiletoram ;

$00 defcrc32reg CRC32DIW0 \ crc32 data input
$06 defcrc32reg CRC32DIRBW0 \ crc32 data input reverse
$08 defcrc32reg CRC32INIRESW0 \ crc32 initialization and result word 0
$0a defcrc32reg CRC32INIRESW1 \ crc32 initialization and result word 1
$0c defcrc32reg CRC32RESRW1 \ crc32 initialization and result word 1
$0e defcrc32reg CRC32RESRW0 \ crc32 initialization and result word 0
compiletoflash

compiletoram
