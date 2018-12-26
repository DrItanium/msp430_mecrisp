\ access to the AES hardware accelerator
\ taken from the data sheet
compiletoflash
$09c0 constant AES_BASE
compiletoram
: defaesreg ( value -- )
  compiletoflash 
  AES_BASE + constant
  compiletoram ;
$00 defaesreg AESACTL0 \ aes accelerator control 0
$02 defaesreg AESACTL1 \ aes accelerator control 1
$04 defaesreg AESASTAT \ aes accelerator status
$06 defaesreg AESAKEY  \ aes accelerator key
$08 defaesreg AESADIN  \ aes accelerator data in
$0a defaesreg AESADOUT \ aes accelerator data out
$0c defaesreg AESAXDIN \ aes accelerator XORed data in
$0e defaesreg AESAXIN  \ aes accelerator XORed data in (no trigger)
compiletoflash
compiletoram
