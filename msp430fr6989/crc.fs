\ crc hardware module access (16-bit)
\ taken from the data sheet
compiletoflash
$150 constant CRC_BASE
compiletoram 
: defcrcreg ( offset -- ) 
  compiletoflash
  CRC_BASE + constant 
  compiletoram ;

$00 defcrcreg CRCDI // CRC DATA INPUT
$02 defcrcreg CRCDIRB // CRC data input reverse byte
$04 defcrcreg CRCINIRES // CRC initialization and result
$06 defcrcreg CRCRESR // CRC result reverse byte
compiletoflash
\ Taken from the documentation for the module
\ The CRC generator is first initialized by writing a 16-bit word 
\ (seed) to the CRC Initialization and Result (CRCINIRES) register.
\ Any data that should be included into the CRC calculation must
\ be written to the CRC Data Input (CRCDI or CRCDIRB) register in
\ the same order that the ordiginal CRC signature was calculated.
\ The actual signature can be read from CRCINIRES register to compare
\ the computed checksum with the expected checksum.
\ 
\ Signature generation describes a method of how the result of a 
\ signature operation can be calculated. The calcuated signature,
\ which is computed by an external tool, is called checksum in
\ the following text. The checksum is stored in the product's memory
\ and is used to check the correctness of the CRC operation result.

\ simple words to access the crc engine
: crc:seed ( seed -- ) CRCINIRES ! ;
: crc! ( value -- ) CRCDI ! ;
: crc!r ( value -- ) CRCDIRB ! ;
: crc@ ( -- value ) CRCINIRES @ ;
: crc@r ( -- value ) CRCRESR @ ;



compiletoram
