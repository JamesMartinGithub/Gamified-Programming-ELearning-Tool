using sly.lexer;

public enum PseudocodeTokens
{
    #region keywords 0 -> 21

    [Keyword("IF")]
    [Keyword("if")]
    IF = 1,

    [Keyword("THEN")]
    [Keyword("then")]
    THEN = 2,

    [Keyword("ELSE")]
    [Keyword("else")]
    ELSE = 3,

    [Keyword("ENDIF")]
    [Keyword("endif")]
    ENDIF = 4,

    [Keyword("FOR")]
    [Keyword("for")]
    FOR = 5,

    [Keyword("TO")]
    [Keyword("to")]
    TO = 6,

    [Keyword("ENDFOR")]
    [Keyword("endfor")]
    ENDFOR = 7,

    [Keyword("WHILE")]
    [Keyword("while")]
    WHILE = 8,

    [Keyword("DO")]
    [Keyword("do")]
    DO = 9,

    [Keyword("ENDWHILE")]
    [Keyword("endwhile")]
    ENDWHILE = 10,

    [Keyword("TRUE")]
    [Keyword("true")]
    TRUE = 11,

    [Keyword("FALSE")]
    [Keyword("false")]
    FALSE = 12,

    [Keyword("NOT")]
    [Keyword("not")]
    NOT = 13,

    [Keyword("AND")]
    [Keyword("and")]
    AND = 14,

    [Keyword("OR")]
    [Keyword("or")]
    OR = 15,

    [Keyword("PRINT")]
    [Keyword("print")]
    PRINT = 16,

    [Keyword("BOOL")]
    [Keyword("bool")]
    BOOL = 17,

    [Keyword("INT")]
    [Keyword("int")]
    INT = 18,

    [Keyword("FLOAT")]
    [Keyword("float")]
    FLOAT = 19,

    [Keyword("STRING")]
    [Keyword("string")]
    STRING = 20,

    [Keyword("RANDOM")]
    [Keyword("random")]
    RANDOM = 21,

    #endregion

    #region literals 22 -> 29

    [AlphaId] IDENTIFIER = 22,

    [String] STRINGVALUE = 23,

    [Int] INTVALUE = 24,

    [Double] FLOATVALUE = 25,

    #endregion

    #region operators 30 -> 49

    [Sugar(">")] GREATER = 30,

    [Sugar(">=")] GREATEREQUALS = 31,

    [Sugar("<")] LESSER = 32,

    [Sugar("<=")] LESSEREQUALS = 33,

    [Sugar("==")]
    EQUALS = 34,

    [Sugar("!=")]
    DIFFERENT = 35,

    [Sugar("=")]
    ASSIGN = 36,

    [Sugar("+")] PLUS = 37,

    [Sugar("-")] MINUS = 38,

    [Sugar("*")] TIMES = 39,

    [Sugar("/")] DIVIDE = 40,

    #endregion

    #region sugar 50 ->

    [Sugar("(")] LPAREN = 50,

    [Sugar(")")] RPAREN = 51,

    [Sugar("[")] LSQUARE = 52,

    [Sugar("]")] RSQUARE = 53,

    [Sugar(";")] SEMICOLON = 54,

    [Sugar("//")]
    [Sugar("#")]
    COMMENT = 55,

    EOF = 0,

    PARSED = 56,

    NULL = 57

    #endregion
}