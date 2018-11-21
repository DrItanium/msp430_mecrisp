\ common forth routines that I find very useful
compiletoflash
\ missing functions that I think are really neat
: even? ( value -- f ) dup even = ;
: odd? ( value -- f ) dup even <> ;
: construct-mask ( mask "name" -- ) <builds , does> ( value addr -- ) @ and ;
$000F construct-mask mask-lowest-int4
$00F0 construct-mask mask-lower-int4
$0F00 construct-mask mask-higher-int4
$F000 construct-mask mask-highest-int4
$00FF construct-mask mask-lower-half
$FF00 construct-mask mask-upper-half
: construct-extractor ( shiftamount maskfunc "name" -- )
  <builds , , 
  does> ( v a-implied -- n )
  dup cell+ @ >r \ stash the shift amount 
  @ execute \ get the mask function out and execute
  r> rshift ;

0 ['] mask-lowest-int4 construct-extractor lowest-int4
4 ['] mask-lower-int4 construct-extractor lower-int4
8 ['] mask-higher-int4 construct-extractor higher-int4
12 ['] mask-highest-int4 construct-extractor highest-int4
0 ['] mask-lower-half construct-extractor lower-half
8 ['] mask-upper-half construct-extractor upper-half

: halve ( value -- l h ) dup lower-half swap upper-half ;
: unhalve ( l h -- value ) 
  8 lshift mask-upper-half ( l h<<8 )
  swap mask-lower-half ( h<<8 lmasked )
  or ;
: quarter ( d -- lowest lower higher highest )
  >r halve ( llo lhi )
  r> halve ( llo lhi hlo hhi ) 
  ;
: unquarter ( llo lhi hlo hhi -- d )
  unhalve >r ( llo lhi )
  unhalve r> ( lo hi ) 
  ;

: d1+ ( d -- d+1 ) 1 s>d d+ ;
: d1- ( d -- d-1 ) 1 s>d d- ;
: d2+ ( d -- d+2 ) 2 s>d d+ ;
: d2- ( d -- d-2 ) 2 s>d d- ;

: save-base ( -- ) 
  postpone base 
  postpone @ 
  postpone >r 
  immediate ; 
: restore-base ( -- ) 
  postpone r> 
  postpone base 
  postpone !  
  immediate ;
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
