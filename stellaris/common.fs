\ common forth routines that I find very useful
\ missing functions that I think are really neat
: even? ( value -- f ) dup even = ;
: odd? ( value -- f ) dup even <> ;

\ Used to make generation of enumerations much more readable 
\ and easy to rearrange
: {constseq ( start -- start ) ;
: constseq1+: ( n -- n+1 ) dup constant 1+ ;
: constseq1-: ( n -- n-1 ) dup constant 1- ;
: constseq+: ( n inc -- n+inc ) over constant + ;
: constseq-: ( n dec -- n-inc ) over constant - ;
: constseq} ( n -- ) drop ;

: 3drop ( a b c -- ) 2drop drop ;
: 4drop ( a b c d -- ) 2drop 2drop ;
  
