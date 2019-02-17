\ device descriptor table
compiletoflash
$1A00 constant INFOBLKBASE
\ info block
INFOBLKBASE 1+ constant INFOBLKLENGTH 
INFOBLKBASE 2+ constant INFOBLKCRCVALUE
INFOBLKBASE 4 + constant INFOBLKDEVICEID
INFOBLKBASE 6 + constant INFOBLKHWREV 
INFOBLKBASE 7 + constant INFOBLKFWREV
INFOBLKBASE 8 + constant DIERECBASE

\ die record
DIERECBASE constant DIERECTAG
DIERECBASE 1+ constant DIERECLEN
DIERECBASE 2+ constant DIERECLOTID
DIERECBASE 6 + constant DIERECXPOS
DIERECBASE 8 + constant DIERECYPOS
DIERECBASE 10 + constant DIERECTESTRES
\ todo insert addresses for other parts
\ random number
$01a2e constant RANDNUMBASE
RANDNUMBASE constant RANDNUMTAG
RANDNUMBASE 1+ constant RANDNUMLEN
RANDNUMBASE 2+ constant RANDNUMVAL


: hwddtrandnum16 ( -- num ) 
  \ pull from the DDT's random number field, different per unit
  RANDNUMVAL @ ;
: hwddtrandnum32 ( -- d )
  RANDNUMVAL @ 
  RANDNUMVAL 2+ @ ;


compiletoram
