grammar LabCalculator; 
/*
* Parser Rules
*/
compileUnit : expression EOF;

expression :
	LPAREN expression RPAREN #ParenthesizedExpr
	| operatorToken=NOT expression                                       #NotExpression
	| operatorToken=(DEC|INC) expression   #DecIncExpression
	| expression operatorToken=(MULTIPLY | DIVIDE ) expression           #MultiplicativeExpr
	| expression operatorToken=(ADD | SUBTRACT) expression               #AdditiveExpr
	| expression operatorToken=(EQUAL|LESS|GREATER) expression           #BoolExpression
	| NUMBER                                                             #NumberExpr
	| IDENTIFIER                                                         #IdentifierExpr
	; 



NUMBER : INT ('.' INT)?; 
IDENTIFIER : ([A-Z]+)+[1-9][0-9]*;

DEC : '--' | 'dec';
INC : '++' | 'inc';

GREATER : '>';
LESS    : '<';
EQUAL   : '=';

NOT : '!';

MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';

INT : ('0'..'9')+;
LPAREN : '(';
RPAREN : ')';

WS : [ \t\r\n] -> channel(HIDDEN);
