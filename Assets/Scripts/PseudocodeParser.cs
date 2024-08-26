using sly.lexer;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PseudocodeParser : MonoBehaviour
{
    public interface ParseType {}
    public class Null : ParseType {}
    public class Block : ParseType { public List<ParseType> blockItems = new List<ParseType>(); }
    public class Assignment : ParseType { public string variable; public Expression arrayIndexExpression; public Expression expression; public LexerPosition variablePos; }
    public class Initialisation : ParseType { public Value.Types type; public bool isArray = false; public string variable; public Expression expression; public LexerPosition variablePos; }
    public class Expression : ParseType { public ParseType equation; public LexerPosition lPos; public LexerPosition rPos; }
    public class If : ParseType { public Expression condition; public Block ifBlock; public Block elseBlock; }
    public class For : ParseType { public Initialisation initialisation; public Expression condition; public Block forBlock; }
    public class While : ParseType { public Expression condition; public Block whileBlock; }
    public class Print : ParseType { public Expression expression; }
    public class Operation : ParseType { public enum Operators { ADD, SUBTRACT, MULTIPLY, DIVIDE, NOT, AND, OR, GREATER, GREATEREQUALS, LESS, LESSEQUALS, EQUALS, DIFFERENT, NEGATION } public Operators op; public ParseType left; public ParseType right; public LexerPosition pos; }
    public class Variable : ParseType { public string name; public LexerPosition pos; public Expression arrayIndexExpression; }
    public class Value : ParseType { public enum Types { INT, FLOAT, STRING, BOOL, INTARRAY, FLOATARRAY, BOOLARRAY, STRINGARRAY } public Types type; public int valueI; public float valueF; public string valueS; public bool valueB; }
    public class ArrayValue : ParseType { public Value.Types type; public Expression sizeExpression; }
    public class Random : ParseType { public Value.Types type; }
    public class Error { public string errorText; public LexerPosition startPosition; public LexerPosition endPosition; public Error(string errorText, LexerPosition startPosition, LexerPosition endPosition) { this.errorText = errorText; this.startPosition = startPosition; this.endPosition = endPosition; } }

    public (Block, Error, List<Error>) Parse(string program) {
        (IList<Token<PseudocodeTokens>> tokens, Error lexError) = Lex(program);
        if (lexError == null) {
            //Remove commented lines
            bool cutting = false;
            int line = 0;
            List<Token<PseudocodeTokens>> delList = new List<Token<PseudocodeTokens>>();
            foreach (Token<PseudocodeTokens> t in tokens) {
                if (cutting && line != t.Position.Line) { cutting = false; line = t.Position.Line; }
                if (t.TokenID == PseudocodeTokens.COMMENT) { cutting = true; line = t.Position.Line; }
                if (cutting) { delList.Add(t); }
            }
            foreach (Token<PseudocodeTokens> t in delList) {
                tokens.Remove(t);
            }
            //foreach (var token in tokens) { print(token); }
            List<Token<PseudocodeTokens>> tokensList = new List<Token<PseudocodeTokens>>(tokens);
            List<PseudocodeTokens> tokensIDList = new List<PseudocodeTokens>();
            foreach (Token<PseudocodeTokens> token in tokensList) {
                tokensIDList.Add(token.TokenID);
            }
            (Block AST, List<Error> errors) = ParseBlock(tokensList, tokensIDList, new List<Error>());
            return (AST, null, errors);
        } else {
            return (null, lexError, null);
        }
    }

    private (Block, List<Error>) ParseBlock(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<Error> errors) {
        Block blockObj = new Block();
        int index;
        for (index = 0; index < tokens.Count; index += 0) {
            switch (tokens[index].TokenID) {
                case PseudocodeTokens.EOF:
                    index += 1;
                    break;
                case PseudocodeTokens.IF: {
                        (int endIfIndex, int iCount) = (-1, -1);
                        for (int i = index; i < tokenIds.Count; i++) {
                            if (tokenIds[i] == PseudocodeTokens.IF) { iCount++; }
                            if (tokenIds[i] == PseudocodeTokens.ENDIF) {
                                iCount--;
                                if (iCount <= -1) { endIfIndex = i; break; }
                            }
                        }
                        if (endIfIndex != -1) {
                            (If ifObj, List<Error> err) = ParseIf(SubList(tokens, index + 1, endIfIndex), SubList(tokenIds, index + 1, endIfIndex), errors);
                            errors = err;
                            blockObj.blockItems.Add(ifObj);
                            index = endIfIndex + 1;
                            break;
                        } else {
                            // No ENDIF
                            errors.Add(new Error("No ENDIF after IF", tokens[index].Position, tokens[index].Position));
                            return (null, errors);
                        }
                    }
                case PseudocodeTokens.FOR: {
                        (int endForIndex, int iCount) = (-1, -1);
                        for (int i = index; i < tokenIds.Count; i++) {
                            if (tokenIds[i] == PseudocodeTokens.FOR) { iCount++; }
                            if (tokenIds[i] == PseudocodeTokens.ENDFOR) {
                                iCount--;
                                if (iCount <= -1) { endForIndex = i; break; }
                            }
                        }
                        if (endForIndex != -1) {
                            (For forObj, List<Error> err) = ParseFor(SubList(tokens, index + 1, endForIndex), SubList(tokenIds, index + 1, endForIndex), errors);
                            errors = err;
                            blockObj.blockItems.Add(forObj);
                            index = endForIndex + 1;
                            break;
                        } else {
                            // No ENDFOR
                            errors.Add(new Error("No ENDFOR after FOR", tokens[index].Position, tokens[index].Position));
                            return (null, errors);
                        }
                    }
                case PseudocodeTokens.WHILE: {
                        (int endWhileIndex, int iCount) = (-1, -1);
                        for (int i = index; i < tokenIds.Count; i++) {
                            if (tokenIds[i] == PseudocodeTokens.WHILE) { iCount++; }
                            if (tokenIds[i] == PseudocodeTokens.ENDWHILE) {
                                iCount--;
                                if (iCount <= -1) { endWhileIndex = i; break; }
                            }
                        }
                        if (endWhileIndex != -1) {
                            (While whileObj, List<Error> err) = ParseWhile(SubList(tokens, index + 1, endWhileIndex), SubList(tokenIds, index + 1, endWhileIndex), errors);
                            errors = err;
                            blockObj.blockItems.Add(whileObj);
                            index = endWhileIndex + 1;
                            break;
                        } else {
                            // No ENDWHILE
                            errors.Add(new Error("No ENDWHILE after WHILE", tokens[index].Position, tokens[index].Position));
                            return (null, errors);
                        }
                    }
                case PseudocodeTokens.PRINT: {
                        int semicolonIndex = SubList(tokenIds, index, tokenIds.Count).IndexOf(PseudocodeTokens.SEMICOLON);
                        if (semicolonIndex != -1) {
                            semicolonIndex += index;
                            Print printObj = new Print();
                            (Expression exp, List<Error> err) = ParseExpression(SubList(tokens, index + 1, semicolonIndex), SubList(tokenIds, index + 1, semicolonIndex), errors, tokens[index].Position);
                            printObj.expression = exp;
                            errors = err;
                            blockObj.blockItems.Add(printObj);
                            index = semicolonIndex + 1;
                            break;
                        } else {
                            //No semicolon
                            errors.Add(new Error("No semicolon after statement", tokens[index].Position, tokens[tokens.Count - 1].Position));
                            return (null, errors);
                        }
                    }
                case PseudocodeTokens.IDENTIFIER: {
                        int semicolonIndex = SubList(tokenIds, index, tokenIds.Count).IndexOf(PseudocodeTokens.SEMICOLON);
                        if (semicolonIndex != -1) {
                            semicolonIndex += index;
                            (Assignment assignmentObj, List<Error> err) = ParseAssignment(SubList(tokens, index, semicolonIndex), SubList(tokenIds, index, semicolonIndex), errors);
                            errors = err;
                            blockObj.blockItems.Add(assignmentObj);
                            index = semicolonIndex + 1;
                            break;
                        } else {
                            //No semicolon
                            errors.Add(new Error("No semicolon after statement", tokens[index].Position, tokens[tokens.Count - 1].Position));
                            return (null, errors);
                        }
                    }
                case PseudocodeTokens.BOOL:
                case PseudocodeTokens.INT:
                case PseudocodeTokens.FLOAT:
                case PseudocodeTokens.STRING: {
                        int semicolonIndex = SubList(tokenIds, index, tokenIds.Count).IndexOf(PseudocodeTokens.SEMICOLON);
                        if (semicolonIndex != -1) {
                            semicolonIndex += index;
                            (Initialisation initialisationObj, List<Error> err) = ParseInitialisation(SubList(tokens, index, semicolonIndex), SubList(tokenIds, index, semicolonIndex), errors);
                            errors = err;
                            blockObj.blockItems.Add(initialisationObj);
                            index = semicolonIndex + 1;
                            break;
                        } else {
                            //No semicolon
                            errors.Add(new Error("No semicolon after statement", tokens[index].Position, tokens[tokens.Count - 1].Position));
                            return (null, errors);
                        }
                    }
                default:
                    //Unexpected token in block
                    errors.Add(new Error("Unexpected '" + tokens[index].TokenID + "' token in block, expected: IF, FOR, WHILE, PRINT, variable, type", tokens[index].Position, tokens[index].Position));
                    return (null, errors);
            }
        }
        return (blockObj, errors);
    }
    private (If, List<Error>) ParseIf(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<Error> errors) {
        int thenIndex = tokenIds.IndexOf(PseudocodeTokens.THEN);
        if (thenIndex != -1) {
            If ifObj = new If();
            (Expression exp, List<Error> err) = ParseExpression(SubList(tokens, 0, thenIndex), SubList(tokenIds, 0, thenIndex), errors, tokens[thenIndex].Position);
            (ifObj.condition, errors) = (exp, err);
            //Find else index
            (int elseIndex, int iCount) = (-1, -1);
            for (int i = 0; i < tokenIds.Count; i++) {
                if (tokenIds[i] == PseudocodeTokens.IF) { iCount++; }
                if (tokenIds[i] == PseudocodeTokens.ENDIF) { iCount--; }
                if (tokenIds[i] == PseudocodeTokens.ELSE && iCount <= -1) { elseIndex = i; break; }
            }
            if (elseIndex != -1) {
                //Else block
                (Block block, List<Error> err2) = ParseBlock(SubList(tokens, thenIndex + 1, elseIndex), SubList(tokenIds, thenIndex + 1, elseIndex), errors);
                (ifObj.ifBlock, errors) = (block, err2);
                (Block block2, List<Error> err3) = ParseBlock(SubList(tokens, elseIndex + 1, tokens.Count), SubList(tokenIds, elseIndex + 1, tokenIds.Count), errors);
                (ifObj.elseBlock, errors) = (block2, err3);
            } else {
                //No else block
                (Block block, List<Error> err2) = ParseBlock(SubList(tokens, thenIndex + 1, tokens.Count), SubList(tokenIds, thenIndex + 1, tokenIds.Count), errors);
                (ifObj.ifBlock, errors) = (block, err2);
                ifObj.elseBlock = null;
            }
            return (ifObj, errors);
        } else {
            //No THEN
            if (tokens.Count == 0) {
                errors.Add(new Error("No THEN after IF", null, null));
            } else {
                errors.Add(new Error("No THEN after IF", tokens[0].Position, tokens[tokens.Count - 1].Position));
            }
            return (null, errors);
        }
    }
    private (For, List<Error>) ParseFor(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<Error> errors) {
        int doIndex = tokenIds.IndexOf(PseudocodeTokens.DO);
        if (doIndex != -1) {
            List<PseudocodeTokens> doTokens = SubList(tokenIds, 0, doIndex);
            int toIndex = doTokens.IndexOf(PseudocodeTokens.TO);
            if (toIndex != -1) {
                For forObj = new For();
                (Initialisation initialise, List<Error> err) = ParseInitialisation(SubList(tokens, 0, toIndex), SubList(tokenIds, 0, toIndex), errors);
                (forObj.initialisation, errors) = (initialise, err);
                (Expression exp, List<Error> err2) = ParseExpression(SubList(tokens, toIndex + 1, doIndex), SubList(tokenIds, toIndex + 1, doIndex), errors, tokens[toIndex].Position);
                (forObj.condition, errors) = (exp, err2);
                (Block block, List<Error> err3) = ParseBlock(SubList(tokens, doIndex + 1, tokens.Count), SubList(tokenIds, doIndex + 1, tokenIds.Count), errors);
                (forObj.forBlock, errors) = (block, err3);
                return (forObj, errors);
            } else {
                //No TO
                errors.Add(new Error("No TO after FOR", tokens[0].Position, tokens[doIndex - 1].Position));
                return (null, errors);
            }
        } else {
            //No DO
            if (tokens.Count == 0) {
                errors.Add(new Error("No DO after FOR", null, null));
            } else {
                errors.Add(new Error("No DO after FOR", tokens[0].Position, tokens[tokens.Count - 1].Position));
            }
            return (null, errors);
        }
    }
    private (While, List<Error>) ParseWhile(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<Error> errors) {
        int doIndex = tokenIds.IndexOf(PseudocodeTokens.DO);
        if (doIndex != -1) {
            While whileObj = new While();
            (Expression exp, List<Error> err) = ParseExpression(SubList(tokens, 0, doIndex), SubList(tokenIds, 0, doIndex), errors, tokens[0].Position);
            (whileObj.condition, errors) = (exp, err);
            (Block block, List<Error> err2) = ParseBlock(SubList(tokens, doIndex + 1, tokens.Count), SubList(tokenIds, doIndex + 1, tokenIds.Count), errors);
            (whileObj.whileBlock, errors) = (block, err2);
            return (whileObj, errors);
        } else {
            //No DO
            if (tokens.Count == 0) {
                errors.Add(new Error("No DO after WHILE", null, null));
            } else {
                errors.Add(new Error("No DO after WHILE", tokens[0].Position, tokens[tokens.Count - 1].Position));
            }
            return (null, errors);
        }
    }
    private (Initialisation, List<Error>) ParseInitialisation(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<Error> errors) {
        Initialisation initialisationObj = new Initialisation();
        List<PseudocodeTokens> types = new List<PseudocodeTokens>() { PseudocodeTokens.BOOL, PseudocodeTokens.INT, PseudocodeTokens.FLOAT, PseudocodeTokens.STRING };
        if (tokens.Count == 2 && types.Contains(tokenIds[0]) && tokenIds[1] == PseudocodeTokens.IDENTIFIER) {
            //Initialisation without setting value
            switch (tokenIds[0]) {
                case PseudocodeTokens.BOOL:
                    initialisationObj.type = Value.Types.BOOL; break;
                case PseudocodeTokens.INT:
                    initialisationObj.type = Value.Types.INT; break;
                case PseudocodeTokens.FLOAT:
                    initialisationObj.type = Value.Types.FLOAT; break;
                case PseudocodeTokens.STRING:
                    initialisationObj.type = Value.Types.STRING; break;
            }
            initialisationObj.variable = tokens[1].Value;
            initialisationObj.variablePos = tokens[1].Position;
            initialisationObj.expression = null;
            return (initialisationObj, errors);
        } else if (tokens.Count >= 3 && types.Contains(tokenIds[0]) && tokenIds[1] == PseudocodeTokens.IDENTIFIER && tokenIds[2] == PseudocodeTokens.ASSIGN) {
            //Initialisation and setting value
            switch (tokenIds[0]) {
                case PseudocodeTokens.BOOL:
                    initialisationObj.type = Value.Types.BOOL; break;
                case PseudocodeTokens.INT:
                    initialisationObj.type = Value.Types.INT; break;
                case PseudocodeTokens.FLOAT:
                    initialisationObj.type = Value.Types.FLOAT; break;
                case PseudocodeTokens.STRING:
                    initialisationObj.type = Value.Types.STRING; break;
            }
            initialisationObj.variable = tokens[1].Value;
            initialisationObj.variablePos = tokens[1].Position;
            (Expression exp, List<Error> err) = ParseExpression(SubList(tokens, 3, tokens.Count), SubList(tokenIds, 3, tokenIds.Count), errors, tokens[2].Position);
            (initialisationObj.expression, errors) = (exp, err);
            return (initialisationObj, errors);
        } else if (tokens.Count == 4 && types.Contains(tokenIds[0]) && tokenIds[1] == PseudocodeTokens.LSQUARE && tokenIds[2] == PseudocodeTokens.RSQUARE && tokenIds[3] == PseudocodeTokens.IDENTIFIER) {
            //Initialisation of array without setting value
            switch (tokenIds[0]) {
                case PseudocodeTokens.BOOL:
                    initialisationObj.type = Value.Types.BOOL; break;
                case PseudocodeTokens.INT:
                    initialisationObj.type = Value.Types.INT; break;
                case PseudocodeTokens.FLOAT:
                    initialisationObj.type = Value.Types.FLOAT; break;
                case PseudocodeTokens.STRING:
                    initialisationObj.type = Value.Types.STRING; break;
            }
            initialisationObj.isArray = true;
            initialisationObj.variable = tokens[3].Value;
            initialisationObj.variablePos = tokens[3].Position;
            initialisationObj.expression = null;
            return (initialisationObj, errors);
        } else if (tokens.Count >= 5 && types.Contains(tokenIds[0]) && tokenIds[1] == PseudocodeTokens.LSQUARE && tokenIds[2] == PseudocodeTokens.RSQUARE && tokenIds[3] == PseudocodeTokens.IDENTIFIER && tokenIds[4] == PseudocodeTokens.ASSIGN) {
            //Initialisation of array with setting size and default values
            switch (tokenIds[0]) {
                case PseudocodeTokens.BOOL:
                    initialisationObj.type = Value.Types.BOOL; break;
                case PseudocodeTokens.INT:
                    initialisationObj.type = Value.Types.INT; break;
                case PseudocodeTokens.FLOAT:
                    initialisationObj.type = Value.Types.FLOAT; break;
                case PseudocodeTokens.STRING:
                    initialisationObj.type = Value.Types.STRING; break;
            }
            initialisationObj.isArray = true;
            initialisationObj.variable = tokens[3].Value;
            initialisationObj.variablePos = tokens[3].Position;
            if (tokens.Count >= 8 && tokenIds[5] == PseudocodeTokens.LSQUARE && tokenIds[tokens.Count - 1] == PseudocodeTokens.RSQUARE) {
                (Expression exp, List<Error> err) = ParseExpression(SubList(tokens, 6, tokens.Count - 1), SubList(tokenIds, 6, tokenIds.Count - 1), errors, tokens[5].Position);
                (initialisationObj.expression, errors) = (exp, err);
                return (initialisationObj, errors);
            } else {
                //Initialisation of array requires size declaration '[size]'
                errors.Add(new Error("Initialisation of array requires size declaration '[size]'", tokens[5].Position, tokens[tokens.Count - 1].Position));
                return (null, errors);
            }
        } else {
            //Initialisation doesn't start with type and variable name followed by ; or =
            errors.Add(new Error("Initialisation must start with 'type variable' followed by ; or =", tokens[0].Position, tokens[tokens.Count - 1].Position));
            return (null, errors);
        }
    }
    private (Assignment, List<Error>) ParseAssignment(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<Error> errors) {
        Assignment assignmentObj = new Assignment();
        List<PseudocodeTokens> types = new List<PseudocodeTokens>() { PseudocodeTokens.BOOL, PseudocodeTokens.INT, PseudocodeTokens.FLOAT, PseudocodeTokens.STRING };
        //Parse index expression if array
        if (tokens.Count >= 2 && tokenIds[0] == PseudocodeTokens.IDENTIFIER && tokenIds[1] == PseudocodeTokens.LSQUARE) {
            int rsquareIndex = SubList(tokenIds, 2, tokenIds.Count).IndexOf(PseudocodeTokens.RSQUARE);
            if (rsquareIndex != -1) {
                rsquareIndex += 2;
                (Expression exp, List<Error> err) = ParseExpression(SubList(tokens, 2, rsquareIndex), SubList(tokenIds, 2, rsquareIndex), errors, tokens[1].Position);
                (assignmentObj.arrayIndexExpression, errors) = (exp, err);
                (tokens, tokenIds) = (ReplaceSublist(tokens, 1, rsquareIndex, null), ReplaceSublist(tokenIds, 1, rsquareIndex, PseudocodeTokens.NULL));
            } else {
                //Array value assignment doesn't have closing square bracket ']'
                errors.Add(new Error("Array value assignment doesn't have closing square bracket ']'", tokens[1].Position, tokens[tokens.Count - 1].Position));
                return (null, errors);
            }
        }
        //Check structure and parse assigned expression
        if (tokens.Count >= 2 && tokenIds[0] == PseudocodeTokens.IDENTIFIER && tokenIds[1] == PseudocodeTokens.ASSIGN) {
            assignmentObj.variable = tokens[0].Value;
            (Expression exp, List<Error> err) = ParseExpression(SubList(tokens, 2, tokens.Count), SubList(tokenIds, 2, tokenIds.Count), errors, tokens[1].Position);
            (assignmentObj.expression, errors) = (exp, err);
            assignmentObj.variablePos = tokens[0].Position;
            return (assignmentObj, errors);
        } else if (tokens.Count >= 3 && tokenIds[0] == PseudocodeTokens.IDENTIFIER && tokenIds[1] == PseudocodeTokens.PLUS && tokenIds[2] == PseudocodeTokens.ASSIGN) {
            assignmentObj.variable = tokens[0].Value;
            List<Token<PseudocodeTokens>> tokenList = SubList(tokens, 3, tokens.Count);
            tokenList.Insert(0, new Token<PseudocodeTokens>(PseudocodeTokens.LPAREN, "(", tokens[2].Position));
            tokenList.Insert(0, new Token<PseudocodeTokens>(PseudocodeTokens.PLUS, "+", tokens[1].Position));
            tokenList.Insert(0, tokens[0]);
            tokenList.Add(new Token<PseudocodeTokens>(PseudocodeTokens.RPAREN, ")", tokens[2].Position));
            List<PseudocodeTokens> tokenidsList = SubList(tokenIds, 3, tokenIds.Count);
            tokenidsList.Insert(0, PseudocodeTokens.LPAREN);
            tokenidsList.Insert(0, PseudocodeTokens.PLUS);
            tokenidsList.Insert(0, PseudocodeTokens.IDENTIFIER);
            tokenidsList.Add(PseudocodeTokens.RPAREN);
            (Expression exp, List<Error> err) = ParseExpression(tokenList, tokenidsList, errors, tokens[2].Position);
            (assignmentObj.expression, errors) = (exp, err);
            assignmentObj.variablePos = tokens[0].Position;
            return (assignmentObj, errors);
        } else if (tokens.Count >= 3 && tokenIds[0] == PseudocodeTokens.IDENTIFIER && tokenIds[1] == PseudocodeTokens.MINUS && tokenIds[2] == PseudocodeTokens.ASSIGN) {
            assignmentObj.variable = tokens[0].Value;
            List<Token<PseudocodeTokens>> tokenList = SubList(tokens, 3, tokens.Count);
            tokenList.Insert(0, new Token<PseudocodeTokens>(PseudocodeTokens.LPAREN, "(", tokens[2].Position));
            tokenList.Insert(0, new Token<PseudocodeTokens>(PseudocodeTokens.MINUS, "-", tokens[1].Position));
            tokenList.Insert(0, tokens[0]);
            tokenList.Add(new Token<PseudocodeTokens>(PseudocodeTokens.RPAREN, ")", tokens[2].Position));
            List<PseudocodeTokens> tokenidsList = SubList(tokenIds, 3, tokenIds.Count);
            tokenidsList.Insert(0, PseudocodeTokens.LPAREN);
            tokenidsList.Insert(0, PseudocodeTokens.MINUS);
            tokenidsList.Insert(0, PseudocodeTokens.IDENTIFIER);
            tokenidsList.Add(PseudocodeTokens.RPAREN);
            (Expression exp, List<Error> err) = ParseExpression(tokenList, tokenidsList, errors, tokens[2].Position);
            (assignmentObj.expression, errors) = (exp, err);
            assignmentObj.variablePos = tokens[0].Position;
            return (assignmentObj, errors);
        } else if (tokens.Count >= 3 && tokenIds[0] == PseudocodeTokens.IDENTIFIER && tokenIds[1] == PseudocodeTokens.TIMES && tokenIds[2] == PseudocodeTokens.ASSIGN) {
            assignmentObj.variable = tokens[0].Value;
            List<Token<PseudocodeTokens>> tokenList = SubList(tokens, 3, tokens.Count);
            tokenList.Insert(0, new Token<PseudocodeTokens>(PseudocodeTokens.LPAREN, "(", tokens[2].Position));
            tokenList.Insert(0, new Token<PseudocodeTokens>(PseudocodeTokens.TIMES, "*", tokens[1].Position));
            tokenList.Insert(0, tokens[0]);
            tokenList.Add(new Token<PseudocodeTokens>(PseudocodeTokens.RPAREN, ")", tokens[2].Position));
            List<PseudocodeTokens> tokenidsList = SubList(tokenIds, 3, tokenIds.Count);
            tokenidsList.Insert(0, PseudocodeTokens.LPAREN);
            tokenidsList.Insert(0, PseudocodeTokens.TIMES);
            tokenidsList.Insert(0, PseudocodeTokens.IDENTIFIER);
            tokenidsList.Add(PseudocodeTokens.RPAREN);
            (Expression exp, List<Error> err) = ParseExpression(tokenList, tokenidsList, errors, tokens[2].Position);
            (assignmentObj.expression, errors) = (exp, err);
            assignmentObj.variablePos = tokens[0].Position;
            return (assignmentObj, errors);
        } else if (tokens.Count >= 3 && tokenIds[0] == PseudocodeTokens.IDENTIFIER && tokenIds[1] == PseudocodeTokens.DIVIDE && tokenIds[2] == PseudocodeTokens.ASSIGN) {
            assignmentObj.variable = tokens[0].Value;
            List<Token<PseudocodeTokens>> tokenList = SubList(tokens, 3, tokens.Count);
            tokenList.Insert(0, new Token<PseudocodeTokens>(PseudocodeTokens.LPAREN, "(", tokens[2].Position));
            tokenList.Insert(0, new Token<PseudocodeTokens>(PseudocodeTokens.DIVIDE, "/", tokens[1].Position));
            tokenList.Insert(0, tokens[0]);
            tokenList.Add(new Token<PseudocodeTokens>(PseudocodeTokens.RPAREN, ")", tokens[2].Position));
            List<PseudocodeTokens> tokenidsList = SubList(tokenIds, 3, tokenIds.Count);
            tokenidsList.Insert(0, PseudocodeTokens.LPAREN);
            tokenidsList.Insert(0, PseudocodeTokens.DIVIDE);
            tokenidsList.Insert(0, PseudocodeTokens.IDENTIFIER);
            tokenidsList.Add(PseudocodeTokens.RPAREN);
            (Expression exp, List<Error> err) = ParseExpression(tokenList, tokenidsList, errors, tokens[2].Position);
            (assignmentObj.expression, errors) = (exp, err);
            assignmentObj.variablePos = tokens[0].Position;
            return (assignmentObj, errors);
        } else {
            //Assignment doesn't start with variable name followed by = sign
            errors.Add(new Error("Assignment doesn't start with 'variable =' or similar", tokens[0].Position, tokens[tokens.Count - 1].Position));
            return (null, errors);
        }
    }
    private (Expression, List<Error>) ParseExpression(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<Error> errors, LexerPosition lastPosition) {
        Expression expressionObj = new Expression();
        if (tokens.Count == 0) {
            errors.Add(new Error("Expression is empty", lastPosition, lastPosition));
            return (expressionObj, errors);
        }
        expressionObj.lPos = tokens[0].Position;
        expressionObj.rPos = tokens[tokens.Count - 1].Position;
        //Handle brackets
        if (tokenIds.Contains(PseudocodeTokens.LPAREN) || tokenIds.Contains(PseudocodeTokens.RPAREN)) {
            (ParseType pType, List<Error> err) = ParseExpressionBrackets(tokens, tokenIds, errors, tokens[0].Position);
            (expressionObj.equation, errors) = (pType, err);
        } else {
            //No brackets
            List<ParseType> parsedSections = new List<ParseType>();
            foreach (Token<PseudocodeTokens> token in tokens) { parsedSections.Add(new Null()); }
            (ParseType pType, List<Error> err) = ParseEquation(tokens, tokenIds, parsedSections, errors, lastPosition);
            (expressionObj.equation, errors) = (pType, err);
        }
        return (expressionObj, errors);
    }
    private (ParseType, List<Error>) ParseExpressionBrackets(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<Error> errors, LexerPosition lastPosition) {
        List<ParseType> parsedSections = new List<ParseType>();
        foreach (Token<PseudocodeTokens> token in tokens) { parsedSections.Add(new Null()); }
        if (tokenIds.Contains(PseudocodeTokens.LPAREN) || tokenIds.Contains(PseudocodeTokens.RPAREN)) {
            Stack<int> lparenStack = new Stack<int>();
            for (int i = 0; i < tokenIds.Count; i++) {
                if (tokenIds[i] == PseudocodeTokens.LPAREN) {
                    lparenStack.Push(i);
                } else if (tokenIds[i] == PseudocodeTokens.RPAREN) {
                    if (lparenStack.Count > 0) {
                        int lparenIndex = lparenStack.Pop();
                        if (lparenStack.Count == 0) {
                            //Reached end of bracket section, can be parsed and replaced
                            (ParseType parsedSection, List<Error> err) = ParseExpressionBrackets(SubList(tokens, lparenIndex + 1, i), SubList(tokenIds, lparenIndex + 1, i), errors, lastPosition);
                            errors = err;
                            parsedSections = ReplaceSublist(parsedSections, lparenIndex, i, parsedSection);
                            Token<PseudocodeTokens> parsedToken = new Token<PseudocodeTokens>();
                            parsedToken.TokenID = PseudocodeTokens.PARSED;
                            (tokens, tokenIds) = (ReplaceSublist(tokens, lparenIndex, i, parsedToken), ReplaceSublist(tokenIds, lparenIndex, i, PseudocodeTokens.PARSED));
                            i = 0;
                        }
                    } else {
                        //Closing bracket has no opening bracket before
                        errors.Add(new Error("Closing bracket has no opening bracket before it", tokens[i].Position, tokens[i].Position));
                        return (null, errors);
                    }
                }
            }
            if (lparenStack.Count == 0) {
                return ParseEquation(tokens, tokenIds, parsedSections, errors, tokens[0].Position);
            } else {
                //Opening bracket has no closing bracket
                int bracketIndex = lparenStack.Pop();
                errors.Add(new Error("Opening bracket has no closing bracket after it", tokens[bracketIndex].Position, tokens[bracketIndex].Position));
                return (null, errors);
            }
        } else {
            //No brackets, parse as equation
            return ParseEquation(tokens, tokenIds, parsedSections, errors, lastPosition);
        }
    }
    private (ParseType, List<Error>) ParseEquation(List<Token<PseudocodeTokens>> tokens, List<PseudocodeTokens> tokenIds, List<ParseType> parseTypeSections, List<Error> errors, LexerPosition lastPosition) {
        List<List<PseudocodeTokens>> precedence = new List<List<PseudocodeTokens>> {
            new List<PseudocodeTokens>() { PseudocodeTokens.OR },
            new List<PseudocodeTokens>() { PseudocodeTokens.AND },
            new List<PseudocodeTokens>() { PseudocodeTokens.NOT },
            new List<PseudocodeTokens>() { PseudocodeTokens.LESSER, PseudocodeTokens.LESSEREQUALS, PseudocodeTokens.GREATER, PseudocodeTokens.GREATEREQUALS, PseudocodeTokens.EQUALS, PseudocodeTokens.DIFFERENT },
            new List<PseudocodeTokens>() { PseudocodeTokens.PLUS, PseudocodeTokens.MINUS },
            new List<PseudocodeTokens>() { PseudocodeTokens.TIMES, PseudocodeTokens.DIVIDE }
        };
        foreach (List<PseudocodeTokens> operators in precedence) {
            for (int i = tokenIds.Count - 1; i >= 0; i--) {
                if (operators.Contains(tokenIds[i])) {
                    Operation op = new Operation();
                    op.pos = tokens[i].Position;
                    switch (tokenIds[i]) {
                        case PseudocodeTokens.TIMES:
                            op.op = Operation.Operators.MULTIPLY; break;
                        case PseudocodeTokens.DIVIDE:
                            op.op = Operation.Operators.DIVIDE; break;
                        case PseudocodeTokens.PLUS:
                            op.op = Operation.Operators.ADD; break;
                        case PseudocodeTokens.MINUS:
                            op.op = Operation.Operators.SUBTRACT; break;
                        case PseudocodeTokens.LESSER:
                            op.op = Operation.Operators.LESS; break;
                        case PseudocodeTokens.GREATER:
                            op.op = Operation.Operators.GREATER; break;
                        case PseudocodeTokens.EQUALS:
                            op.op = Operation.Operators.EQUALS; break;
                        case PseudocodeTokens.DIFFERENT:
                            op.op = Operation.Operators.DIFFERENT; break;
                        case PseudocodeTokens.LESSEREQUALS:
                            op.op = Operation.Operators.LESSEQUALS; break;
                        case PseudocodeTokens.GREATEREQUALS:
                            op.op = Operation.Operators.GREATEREQUALS; break;
                        case PseudocodeTokens.NOT:
                            op.op = Operation.Operators.NOT; break;
                        case PseudocodeTokens.AND:
                            op.op = Operation.Operators.AND; break;
                        case PseudocodeTokens.OR:
                            op.op = Operation.Operators.OR; break;
                    }
                    if (tokenIds[i] == PseudocodeTokens.MINUS && (i == 0 || IsOperator(tokenIds[i - 1]))) {
                        op.op = Operation.Operators.NEGATION;
                        if (i + 1 < tokenIds.Count && IsValueVariableParsed(tokenIds[i + 1])) {
                            (ParseType pType, List<Error> err) = ParseEquation(SubList(tokens, i + 1, i + 2), SubList(tokenIds, i + 1, i + 2), SubList(parseTypeSections, i + 1, i + 2), errors, tokens[i].Position);
                            op.right = pType;
                            errors = err;
                            Token<PseudocodeTokens> parsedToken = new Token<PseudocodeTokens>();
                            parsedToken.TokenID = PseudocodeTokens.PARSED;
                            return ParseEquation(ReplaceSublist(tokens, i, i + 1, parsedToken), ReplaceSublist(tokenIds, i, i + 1, PseudocodeTokens.PARSED), ReplaceSublist(parseTypeSections, i, i + 1, op), errors, tokens[i].Position);
                        } else {
                            ParseEquation(new List<Token<PseudocodeTokens>>(), new List<PseudocodeTokens>(), null, errors, tokens[i].Position);
                        }
                    } else if (tokenIds[i] == PseudocodeTokens.NOT) {
                        (ParseType pType, List<Error> err) = ParseEquation(SubList(tokens, i + 1, tokens.Count), SubList(tokenIds, i + 1, tokens.Count), SubList(parseTypeSections, i + 1, tokens.Count), errors, tokens[i].Position);
                        op.right = pType;
                        op.left = null;
                        errors = err;
                    } else {
                        (ParseType pType, List<Error> err) = ParseEquation(SubList(tokens, 0, i), SubList(tokenIds, 0, i), SubList(parseTypeSections, 0, i), errors, tokens[i].Position);
                        (op.left, errors) = (pType, err);
                        (ParseType pType2, List<Error> err2) = ParseEquation(SubList(tokens, i + 1, tokens.Count), SubList(tokenIds, i + 1, tokens.Count), SubList(parseTypeSections, i + 1, tokens.Count), errors, tokens[i].Position);
                        (op.right, errors) = (pType2, err2);
                    }
                    return (op, errors);
                }
            }
        }
        //No operators found, return as variable, value, or nothing
        if (tokenIds.Count == 0) {
            //Empty brackets or no value on one side of an operation
            errors.Add(new Error("Empty brackets or no value on one side of an operation", lastPosition, lastPosition));
            return (null, errors);
        } else if (tokenIds.Count == 1 && (IsValueVariableParsed(tokenIds[0]))) {
            switch (tokenIds[0]) {
                case PseudocodeTokens.IDENTIFIER:
                    Variable variable = new Variable();
                    variable.name = tokens[0].Value;
                    variable.pos = tokens[0].Position;
                    return (variable, errors);
                case PseudocodeTokens.INTVALUE:
                    Value valueI = new Value();
                    valueI.type = Value.Types.INT;
                    valueI.valueI = int.Parse(tokens[0].Value);
                    return (valueI, errors);
                case PseudocodeTokens.FLOATVALUE:
                    Value valueF = new Value();
                    valueF.type = Value.Types.FLOAT;
                    valueF.valueF = float.Parse(tokens[0].Value);
                    return (valueF, errors);
                case PseudocodeTokens.STRINGVALUE:
                    Value valueS = new Value();
                    valueS.type = Value.Types.STRING;
                    valueS.valueS = tokens[0].Value.Substring(1, tokens[0].Value.Length - 2);
                    return (valueS, errors);
                case PseudocodeTokens.TRUE: {
                        Value valueB = new Value();
                        valueB.type = Value.Types.BOOL;
                        valueB.valueB = true;
                        return (valueB, errors);
                    }
                case PseudocodeTokens.FALSE: {
                        Value valueB = new Value();
                        valueB.type = Value.Types.BOOL;
                        valueB.valueB = false;
                        return (valueB, errors);
                    }
                case PseudocodeTokens.PARSED:
                    return (parseTypeSections[0], errors);
            }
            return (null, errors);
        } else if ((tokenIds.Count == 2) && (tokenIds[0] == PseudocodeTokens.RANDOM) && (new List<PseudocodeTokens>() { PseudocodeTokens.INT, PseudocodeTokens.FLOAT, PseudocodeTokens.BOOL, PseudocodeTokens.STRING }.Contains(tokenIds[1]))) {
            Random rand = new Random();
            switch (tokenIds[1]) {
                case PseudocodeTokens.INT:
                    rand.type = Value.Types.INT; break;
                case PseudocodeTokens.FLOAT:
                    rand.type = Value.Types.FLOAT; break;
                case PseudocodeTokens.BOOL:
                    rand.type = Value.Types.BOOL; break;
                case PseudocodeTokens.STRING:
                    rand.type = Value.Types.STRING; break;
            }
            return (rand, errors);
        } if (tokenIds.Count >= 3 && tokenIds[0] == PseudocodeTokens.IDENTIFIER && tokenIds[1] == PseudocodeTokens.LSQUARE && tokenIds[tokenIds.Count - 1] == PseudocodeTokens.RSQUARE) {
            if (tokenIds.Count >= 4) {
                (Expression index, List<Error> err) = ParseExpression(SubList(tokens, 2, tokens.Count - 1), SubList(tokenIds, 2, tokens.Count - 1), errors, tokens[2].Position);
                Variable var = new Variable();
                (var.arrayIndexExpression, errors) = (index, err);
                var.name = tokens[0].Value;
                var.pos = tokens[0].Position;
                return (var, errors);
            } else {
                //Indexer has no index specified
                errors.Add(new Error("Indexer has no index specified, expected [intvalue]", tokens[1].Position, tokens[tokens.Count - 1].Position));
                return (null, errors);
            }
        } else if (tokenIds.Count >= 3 && (new List<PseudocodeTokens>() { PseudocodeTokens.INT, PseudocodeTokens.FLOAT, PseudocodeTokens.BOOL, PseudocodeTokens.STRING }.Contains(tokenIds[0])) && tokenIds[1] == PseudocodeTokens.LSQUARE && tokenIds[tokenIds.Count - 1] == PseudocodeTokens.RSQUARE) {
            if (tokenIds.Count >= 4) {
                ArrayValue val = new ArrayValue();
                (val.sizeExpression, errors) = ParseExpression(SubList(tokens, 2, tokens.Count - 1), SubList(tokenIds, 2, tokens.Count - 1), errors, tokens[2].Position);
                switch (tokenIds[0]) {
                    case PseudocodeTokens.INT:
                        val.type = Value.Types.INTARRAY; break;
                    case PseudocodeTokens.FLOAT:
                        val.type = Value.Types.FLOATARRAY; break;
                    case PseudocodeTokens.BOOL:
                        val.type = Value.Types.BOOLARRAY; break;
                    case PseudocodeTokens.STRING:
                        val.type = Value.Types.STRINGARRAY; break;
                }
                return (val, errors);
            } else {
                //Array declaration has no size value
                errors.Add(new Error("Array declaration has no size value, expected type[intvalue]", tokens[1].Position, tokens[tokens.Count - 1].Position));
                return (null, errors);
            }
        } else {
            PrintList(tokenIds);
            //Invalid equation
            errors.Add(new Error("Invalid equation", tokens[0].Position, tokens[tokens.Count - 1].Position));
            return (null, errors);
        }
    }

    private bool IsOperator(PseudocodeTokens token) {
        List<PseudocodeTokens> opList = new List<PseudocodeTokens>() { PseudocodeTokens.TIMES, PseudocodeTokens.DIVIDE, PseudocodeTokens.PLUS, 
            PseudocodeTokens.MINUS, PseudocodeTokens.LESSER, PseudocodeTokens.GREATER, PseudocodeTokens.EQUALS, PseudocodeTokens.DIFFERENT, 
            PseudocodeTokens.LESSEREQUALS, PseudocodeTokens.GREATEREQUALS, PseudocodeTokens.NOT, PseudocodeTokens.AND, PseudocodeTokens.OR };
        return opList.Contains(token);
    }
    private bool IsValueVariableParsed(PseudocodeTokens token) {
        List<PseudocodeTokens> varList = new List<PseudocodeTokens>() { PseudocodeTokens.IDENTIFIER, PseudocodeTokens.INTVALUE, PseudocodeTokens.FLOATVALUE, PseudocodeTokens.STRINGVALUE, PseudocodeTokens.TRUE, PseudocodeTokens.FALSE, PseudocodeTokens.PARSED };
        return varList.Contains(token);
    }

    private (IList<Token<PseudocodeTokens>>, Error) Lex(string source) {
        LexerResult<PseudocodeTokens> result = null;
        try {
            //LexerBuilder is from the CSLY library, which can be found at https://github.com/b3b00/csly
            ILexer<PseudocodeTokens> lexer = LexerBuilder.BuildLexer<PseudocodeTokens>().Result;
            result = lexer.Tokenize(source);
        }
        catch (Exception) {
            return (null, new Error("Unexpected lexer error", null, null));
        }
        if (result.IsOk) {
            IList<Token<PseudocodeTokens>> tokens = result.Tokens.Tokens;
            return (tokens, null);
        } else {
            if (result.IsError) {
                print("LEX ERROR: " + result.Error.ErrorType + " at line " + result.Error.Line + ", column " + result.Error.Column);
                print(result.Error.UnexpectedChar);
                LexerPosition lexPos = new LexerPosition(0, result.Error.Line, result.Error.Column);
                return (null, new Error(result.Error.ErrorType + " '" + result.Error.UnexpectedChar + "' at (" + result.Error.Line + "," + result.Error.Column, null, null));
            }
        }
        LexerPosition lexPos2 = new LexerPosition(0, 0, 0);
        return (null, new Error("Lexer error", lexPos2, lexPos2));
    }

    //Returns a sublist from start index (inclusive) to end index (exclusive)
    private List<Token<PseudocodeTokens>> SubList(List<Token<PseudocodeTokens>> tokens, int start, int end) {
        if (start == end || (tokens.Count - (start + (tokens.Count - end)) < 0)) { return (new List<Token<PseudocodeTokens>>()); }
        return tokens.GetRange(start, tokens.Count - (start + (tokens.Count - end)));
    }
    private List<PseudocodeTokens> SubList(List<PseudocodeTokens> tokens, int start, int end) {
        if (start == end || (tokens.Count - (start + (tokens.Count - end)) < 0)) { return (new List<PseudocodeTokens>()); }
        return tokens.GetRange(start, tokens.Count - (start + (tokens.Count - end)));
    }
    private List<ParseType> SubList(List<ParseType> tokens, int start, int end) {
        if (tokens == null) {
            //return null;
            return new List<ParseType>();
        } else {
            if (start == end || (tokens.Count - (start + (tokens.Count - end)) < 0)) { return (new List<ParseType>()); }
            return tokens.GetRange(start, tokens.Count - (start + (tokens.Count - end)));
        }
    }
    //Cuts out section of list between start index (inclusive to cut) and end index (inclusive to cut)
    private List<Token<PseudocodeTokens>> ReplaceSublist(List<Token<PseudocodeTokens>> tokens, int start, int end, Token<PseudocodeTokens> replacement) {
        List<Token<PseudocodeTokens>> subTokens = SubList(tokens, 0, start);
        if (replacement != null) { subTokens.Add(replacement); }
        subTokens.AddRange(SubList(tokens, end + 1, tokens.Count));
        return subTokens;
    }
    private List<PseudocodeTokens> ReplaceSublist(List<PseudocodeTokens> tokens, int start, int end, PseudocodeTokens replacement) {
        List<PseudocodeTokens> subTokens = SubList(tokens, 0, start);
        if (replacement != PseudocodeTokens.NULL) { subTokens.Add(replacement); }
        subTokens.AddRange(SubList(tokens, end + 1, tokens.Count));
        return subTokens;
    }
    private List<ParseType> ReplaceSublist(List<ParseType> tokens, int start, int end, ParseType replacement) {
        List<ParseType> subTokens = SubList(tokens, 0, start);
        if (replacement != null) { subTokens.Add(replacement); }
        subTokens.AddRange(SubList(tokens, end + 1, tokens.Count));
        return subTokens;
    }

    private void PrintList(List<PseudocodeTokens> tokens) {
        print("Printing tokens:");
        foreach (PseudocodeTokens token in tokens) {
            print(token);
        }
    }
    private void PrintList(List<Token<PseudocodeTokens>> tokens) {
        print("Printing tokens:");
        foreach (Token<PseudocodeTokens> token in tokens) {
            print(token);
        }
    }
    private void PrintList(List<ParseType> tokens) {
        print("Printing tokens:");
        foreach (ParseType token in tokens) {
            print(token);
        }
    }
}