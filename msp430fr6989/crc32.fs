\ access to the hardware crc32 mechanism

\ crc32 
compiletoflash
$0980 constant CRC32_BASE
compiletoram
: defcrc32reg ( offset -- )
  compiletoflash 
  CRC32_BASE + constant 
  compiletoram ;

$00 defcrc32reg CRC32DIW0 \ crc32 data input word 0
$02 defcrc32reg CRC32DIW1 \ crc32 data input word 1
$04 defcrc32reg CRC32DIRBW1 \ crc32 data input reverse word 1
$06 defcrc32reg CRC32DIRBW0 \ crc32 data input reverse word 0
$08 defcrc32reg CRC32INIRESW0 \ crc32 initialization and result word 0
$0a defcrc32reg CRC32INIRESW1 \ crc32 initialization and result word 1
$0c defcrc32reg CRC32RESRW1 \ crc32 initialization and result word 1
$0e defcrc32reg CRC32RESRW0 \ crc32 initialization and result word 0

$10 defcrc32reg CRC16DIW0 \ crc16 data input
$16 defcrc32reg CRC16DIRBW0	\ crc16 data input reverse
$18 defcrc32reg CRC16INIRESW0	\ crc16 initialization and result word 0
$1e defcrc32reg CRC16RESRW1 \ crc16 result reverse word 0
compiletoflash

: crc32:seed ( dseed -- ) CRC32INIRESW1 ! 
						  CRC32INIRESW0 ! ;
: crc32@ ( -- dword ) CRC32INIRESW0 @ 
                      CRC32INIRESW1 @ ;
: crc32@r ( -- dword ) CRC32RESRW0 @ 
					   CRC32RESRW1 @ ;
\ : crc32! ( dword -- ) 
: crc16:seed ( seed -- ) CRC16INIRES ! ;
: crc16@ ( -- word ) CRC16RESRW1 @ ;
\ : crc16@r ( -- word ) CRC16RES

compiletoram
