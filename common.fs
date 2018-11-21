\ common forth routines that I find very useful
compiletoflash
: even? ( value -- f ) dup even = ;
: odd? ( value -- f ) dup even <> ;
\ missing functions that I think are really neat
: even? ( value -- f ) dup even = ;
: odd? ( value -- f ) dup even <> ;
: lower-half ( value -- l ) $00FF and ;
: upper-half ( value -- h ) $FF00 and 8 rshift ;
: d1+ ( d -- d+1 ) 1 s>d d+ ;
: d1- ( d -- d-1 ) 1 s>d d- ;
: save-base ( -- ) postpone base postpone @ postpone >r immediate ; 
: restore-base ( -- ) postpone r> postpone base postpone !  immediate ;
: 3>r ( a b c -- ) 
  postpone rot ( b c a )
  postpone >r ( b c )
  postpone 2>r ( )
  immediate ;
: 3r> ( -- a b c )
  postpone 2r> ( b c )
  postpone r>  ( b c a )
  postpone -rot 
  immediate ;
compiletoram
