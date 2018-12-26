\ crc16 hardware module access
\ taken from the data sheet
compiletoflash
$150 constant CRC16_BASE
compiletoram 
: defcrc16reg ( offset -- ) 
  compiletoflash
  CRC16_BASE + constant 
  compiletoram ;

$00 defcrc16reg CRC16_DI // CRC DATA INPUT
$02 defcrc16reg CRCDIRB // CRC data input reverse byte
$04 defcrc16reg CRCINIRES // CRC initialization and result
$06 defcrc16reg CRCINIRES // CRC result reverse byte
compiletoflash


compiletoram
