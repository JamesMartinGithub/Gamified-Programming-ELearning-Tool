using System.Collections.Generic;
using UnityEngine;

public class PseudocodeInterpreter : MonoBehaviour
{
    public PseudocodeLevelController controller;
    public PseudocodeParser parser;
    public class ArrayValues { public int[] valuesI; public float[] valuesF; public bool[] valuesB; public string[] valuesS; }
    public class Variable { public enum Types { INT, FLOAT, BOOL, STRING, ERROR, INTARRAY, FLOATARRAY, BOOLARRAY, STRINGARRAY }; public Types type; public bool isArray = false; public bool isInitialise = true; public int valueI; public float valueF; public string valueS; public bool valueB; public ArrayValues arrayValues = new ArrayValues(); public bool hasValue; public Variable(Types type, bool hasValue = true) { this.type = type; this.hasValue = hasValue; } }
    public class Variables { public Dictionary<string, Variable> variables = new Dictionary<string, Variable>(); }

    public List<string> Interpret(string source) {
        (PseudocodeParser.Block AST, PseudocodeParser.Error lexError, List<PseudocodeParser.Error> errors) = (null, null, null);
        try {
            (AST, lexError, errors) = parser.Parse(source);
        }
        catch (System.Exception) {
            if (controller != null) { controller.ShowError(new List<PseudocodeParser.Error>() { new PseudocodeParser.Error("Unexpected parser error", null, null) }); }
        }
        if (lexError == null) {
            if (errors.Count == 0) {
                (List<string> output, Variables v, PseudocodeParser.Error err) = (null, null, null);
                try {
                    (output, v, err) = InterpretBlock(AST, new List<string>(), new Variables(), null);
                }
                catch (System.Exception) {
                    if (controller != null) { controller.ShowError(new List<PseudocodeParser.Error>() { new PseudocodeParser.Error("Unexpected interpreter error", null, null) }); }
                }
                if (err == null) {
                    if (controller != null) { controller.ShowOutput(output); }
                    return output;
                } else {
                    //Type error
                    if (controller != null) { controller.ShowError(new List<PseudocodeParser.Error>() { err }); }
                }
            } else {
                //Parse error
                if (controller != null) { controller.ShowError(errors); }
            }
        } else {
            //Lex error
            if (controller != null) { controller.ShowError(new List<PseudocodeParser.Error>() { lexError }); }
        }
        return null;
    }

    private (List<string>, Variables, PseudocodeParser.Error) InterpretBlock(PseudocodeParser.Block block, List<string> outputList, Variables variables, PseudocodeParser.Error error) {
        foreach (PseudocodeParser.ParseType line in block.blockItems) {
            (outputList, variables, error) = InterpretBlockLine(line, outputList, variables, error);
            if (error != null) { return (null, null, error); }
        }
        return (outputList, RemoveAssignments(variables), error);
    }
    private (List<string>, Variables, PseudocodeParser.Error) InterpretBlockLine(PseudocodeParser.ParseType line, List<string> outputList, Variables variables, PseudocodeParser.Error error) {
        switch (line) {
            case PseudocodeParser.If: {
                    PseudocodeParser.If ifObj = (PseudocodeParser.If)line;
                    (Variable result, PseudocodeParser.Error err) = InterpretExpression(ifObj.condition, variables, error);
                    if (err != null) { return (null, null, err); }
                    if (result.type == Variable.Types.BOOL) {
                        //Run comparison
                        if (result.valueB == true) {
                            (outputList, variables, error) = InterpretBlock(ifObj.ifBlock, outputList, ClearAssignmentTags(variables), error);
                        } else {
                            if (ifObj.elseBlock != null) {
                                (outputList, variables, error) = InterpretBlock(ifObj.elseBlock, outputList, ClearAssignmentTags(variables), error);
                            }
                            
                        }
                        return (outputList, variables, error);
                    } else {
                        return (null, null, new PseudocodeParser.Error("IF condition cannot be '" + result.type + "' type, BOOL type expected", ifObj.condition.lPos, ifObj.condition.rPos));
                    }
                }
            case PseudocodeParser.For: {
                    PseudocodeParser.For forObj = (PseudocodeParser.For)line;
                    (Variables result, PseudocodeParser.Error err1) = InterpretInitialisation(forObj.initialisation, variables, error);
                    if (err1 != null) { return (null, null, err1); }
                    variables = result;
                    (Variable condition, PseudocodeParser.Error err2) = InterpretExpression(forObj.condition, variables, error);
                    if (err2 != null) { return (null, null, err2); }
                    if (ConvertType(forObj.initialisation.type) == Variable.Types.INT && condition.type == Variable.Types.INT) {
                        //Run loop
                        int maxIterationCount = 1000;
                        for (int i = 0; i < maxIterationCount; i++) {
                            if (variables.variables[forObj.initialisation.variable].valueI == condition.valueI) { variables.variables.Remove(forObj.initialisation.variable); return (outputList, variables, error); }
                            (outputList, variables, error) = InterpretBlock(forObj.forBlock, outputList, ClearAssignmentTags(variables), error);
                            if (error != null) { return (null, null, error); }
                            //Increment variable
                            variables.variables[forObj.initialisation.variable].valueI += 1;
                        }
                        return (null, null, new PseudocodeParser.Error("FOR loop reached maximum iteration count of " + maxIterationCount, forObj.condition.lPos, forObj.condition.rPos));
                    } else {
                        return (null, null, new PseudocodeParser.Error("FOR condition must be INT type", forObj.condition.lPos, forObj.condition.rPos));
                    }
                }
            case PseudocodeParser.While: {
                    PseudocodeParser.While whileObj = (PseudocodeParser.While)line;
                    (Variable condition, PseudocodeParser.Error err) = InterpretExpression(whileObj.condition, variables, error);
                    if (err != null) { return (null, null, err); }
                    if (condition.type == Variable.Types.BOOL) {
                        //Run loop
                        int maxIterationCount = 1000;
                        for (int i = 0; i < maxIterationCount; i++) {
                            if (condition.valueB == false) { return (outputList, variables, error); }
                            (outputList, variables, error) = InterpretBlock(whileObj.whileBlock, outputList, ClearAssignmentTags(variables), error);
                            if (error != null) { return (null, null, error); }
                            (condition, error) = InterpretExpression(whileObj.condition, variables, error);
                            if (error != null) { return (null, null, error); }
                        }
                        return (null, null, new PseudocodeParser.Error("WHILE loop reached maximum iteration count of " + maxIterationCount, whileObj.condition.lPos, whileObj.condition.rPos));
                    } else {
                        return (null, null, new PseudocodeParser.Error("WHILE condition must be BOOL type, aka include ==, >, or < operations", whileObj.condition.lPos, whileObj.condition.rPos));
                    }
                }
            case PseudocodeParser.Print: {
                    PseudocodeParser.Print printObj = (PseudocodeParser.Print)line;
                    (Variable var, PseudocodeParser.Error err) = InterpretExpression(printObj.expression, variables, error);
                    if (err != null) { return (null, null, err); }
                    switch (var.type) {
                        case Variable.Types.INT:
                            outputList.Add(var.valueI.ToString()); break;
                        case Variable.Types.FLOAT:
                            outputList.Add(var.valueF.ToString()); break;
                        case Variable.Types.BOOL:
                            outputList.Add(var.valueB.ToString().ToUpper()); break;
                        case Variable.Types.STRING:
                            outputList.Add(var.valueS); break;
                        case Variable.Types.INTARRAY:
                            foreach (int i in var.arrayValues.valuesI) { outputList.Add(i.ToString()); } break;
                        case Variable.Types.FLOATARRAY:
                            foreach (float i in var.arrayValues.valuesF) { outputList.Add(i.ToString()); } break;
                        case Variable.Types.BOOLARRAY:
                            foreach (bool i in var.arrayValues.valuesB) { outputList.Add(i.ToString().ToUpper()); } break;
                        case Variable.Types.STRINGARRAY:
                            foreach (string i in var.arrayValues.valuesS) { outputList.Add(i); } break;
                    }
                    return (outputList, variables, null);
                }
            case PseudocodeParser.Assignment:
                PseudocodeParser.Assignment assignObj = (PseudocodeParser.Assignment)line;
                (variables, error) = InterpretAssignment(assignObj, variables, error);
                return (outputList, variables, error);
            case PseudocodeParser.Initialisation:
                PseudocodeParser.Initialisation initObj = (PseudocodeParser.Initialisation)line;
                (variables, error) = InterpretInitialisation(initObj, variables, error);
                return (outputList, variables, error);
        }
        return (outputList, variables, error);
    }
    private (Variables, PseudocodeParser.Error) InterpretInitialisation(PseudocodeParser.Initialisation init, Variables variables, PseudocodeParser.Error error) {
        if (!variables.variables.ContainsKey(init.variable)) {
            if (init.isArray) {
                if (init.expression == null) {
                    //Initialisation of array without setting value
                    Variable var = new Variable(ConvertToArrayType(ConvertType(init.type)), hasValue: false);
                    var.isArray = true;
                    variables.variables.Add(init.variable, var);
                    return (variables, error);
                } else {
                    //Initialisation of array with setting size and default values
                    (Variable sizeVar, PseudocodeParser.Error err) = InterpretExpression(init.expression, variables, error);
                    if (err != null) { return (null, err); }
                    if (sizeVar.type == Variable.Types.INT) {
                        if (sizeVar.valueI > 100) {
                            return (null, new PseudocodeParser.Error("Array size must be <=100, '" + sizeVar.valueI.ToString() + "' provided", init.expression.lPos, init.expression.rPos));
                        }
                        //Add to variables
                        Variable var = new Variable(ConvertToArrayType(ConvertType(init.type)), hasValue: true);
                        var.isArray = true;
                        switch (ConvertType(init.type)) {
                            case Variable.Types.INT:
                                var.arrayValues.valuesI = new int[sizeVar.valueI]; break;
                            case Variable.Types.FLOAT:
                                var.arrayValues.valuesF = new float[sizeVar.valueI]; break;
                            case Variable.Types.BOOL:
                                var.arrayValues.valuesB = new bool[sizeVar.valueI]; break;
                            case Variable.Types.STRING:
                                var.arrayValues.valuesS = new string[sizeVar.valueI]; break;
                        }
                        variables.variables.Add(init.variable, var);
                        return (variables, error);
                    } else {
                        return (null, new PseudocodeParser.Error("Array size is '" + sizeVar.type + "' type, expected INT", init.expression.lPos, init.expression.rPos));
                    }
                }
            } else {
                if (init.expression == null) {
                    //Initialisation without setting value
                    Variable var = new Variable(ConvertType(init.type), hasValue: false);
                    variables.variables.Add(init.variable, var);
                    return (variables, error);
                } else {
                    //Initialisation with setting value
                    (Variable var, PseudocodeParser.Error err) = InterpretExpression(init.expression, variables, error);
                    if (err != null) { return (null, err); }
                    if (ConvertType(init.type) == var.type) {
                        //Add to variables
                        variables.variables.Add(init.variable, var);
                        return (variables, error);
                    } else {
                        return (null, new PseudocodeParser.Error("Variable '" + init.variable + "' with '" + init.type + "' type can't have '" + var.type + "' type assigned", init.expression.lPos, init.expression.rPos));
                    }
                }
            }
        } else {
            return (null, new PseudocodeParser.Error("Variable '" + init.variable + "' already exists", init.variablePos, init.variablePos));
        }
    }
    private (Variables, PseudocodeParser.Error) InterpretAssignment(PseudocodeParser.Assignment assign, Variables variables, PseudocodeParser.Error error) {
        if (variables.variables.ContainsKey(assign.variable)) {
            (Variable var, PseudocodeParser.Error err) = InterpretExpression(assign.expression, variables, error);
            if (err != null) { return (null, err); }
            Variable indexVar = null;
            if (assign.arrayIndexExpression != null) {
                (Variable indexVarTemp, PseudocodeParser.Error err2) = InterpretExpression(assign.arrayIndexExpression, variables, error);
                if (err2 != null) { return (null, err2); }
                indexVar = indexVarTemp;
            }
            var.isInitialise = false;
            Variable.Types actualType = variables.variables[assign.variable].type;
            if (assign.arrayIndexExpression != null) { actualType = ConvertToNonArrayType(variables.variables[assign.variable].type); }
            if (actualType == var.type) {
                //Update variables
                if (variables.variables[assign.variable].isArray) {
                    if (assign.arrayIndexExpression == null) {
                        //Update whole array with other array
                        variables.variables[assign.variable].arrayValues = var.arrayValues;
                        variables.variables[assign.variable].hasValue = true;
                    } else {
                        //Update array value at index
                        if (indexVar.type == Variable.Types.INT) {
                            int length;
                            switch (var.type) {
                                case Variable.Types.INT:
                                    length = variables.variables[assign.variable].arrayValues.valuesI.Length;
                                    if (indexVar.valueI < length && indexVar.valueI >= 0) {
                                        variables.variables[assign.variable].arrayValues.valuesI[indexVar.valueI] = var.valueI; break;
                                    } else {
                                        return (null, new PseudocodeParser.Error("Array index must be positive and < array length", assign.arrayIndexExpression.lPos, assign.arrayIndexExpression.rPos));
                                    }
                                case Variable.Types.FLOAT:
                                    length = variables.variables[assign.variable].arrayValues.valuesF.Length;
                                    if (indexVar.valueI < length && indexVar.valueI >= 0) {
                                        variables.variables[assign.variable].arrayValues.valuesF[indexVar.valueI] = var.valueF; break;
                                    } else {
                                        return (null, new PseudocodeParser.Error("Array index must be positive and < array length", assign.arrayIndexExpression.lPos, assign.arrayIndexExpression.rPos));
                                    }
                                case Variable.Types.BOOL:
                                    length = variables.variables[assign.variable].arrayValues.valuesB.Length;
                                    if (indexVar.valueI < length && indexVar.valueI >= 0) {
                                        variables.variables[assign.variable].arrayValues.valuesB[indexVar.valueI] = var.valueB; break;
                                    } else {
                                        return (null, new PseudocodeParser.Error("Array index must be positive and < array length", assign.arrayIndexExpression.lPos, assign.arrayIndexExpression.rPos));
                                    }
                                case Variable.Types.STRING:
                                    length = variables.variables[assign.variable].arrayValues.valuesS.Length;
                                    if (indexVar.valueI < length && indexVar.valueI >= 0) {
                                        variables.variables[assign.variable].arrayValues.valuesS[indexVar.valueI] = var.valueS; break;
                                    } else {
                                        return (null, new PseudocodeParser.Error("Array index must be positive and < array length", assign.arrayIndexExpression.lPos, assign.arrayIndexExpression.rPos));
                                    }
                            }
                        } else {
                            return (null, new PseudocodeParser.Error("Index value has type '" + indexVar.type + "', expected INT", assign.arrayIndexExpression.lPos, assign.arrayIndexExpression.rPos));
                        }
                    }
                } else {
                    //Update variable value
                    if (assign.arrayIndexExpression == null) {
                        variables.variables[assign.variable] = var;
                        variables.variables[assign.variable].hasValue = true;
                    } else {
                        return (null, new PseudocodeParser.Error("Non-array variable '" + assign.variable + "' cannot be indexed like an array", assign.arrayIndexExpression.lPos, assign.arrayIndexExpression.rPos));
                    }
                }
                return (variables, error);
            } else {
                return (null, new PseudocodeParser.Error("Variable '" + assign.variable + "' with '" + actualType + "' type can't have '" + var.type + "' type assigned", assign.expression.lPos, assign.expression.rPos));
            }
        } else {
            return (null, new PseudocodeParser.Error("Variable '" + assign.variable + "' needs a type before a value can be assigned", assign.variablePos, assign.variablePos));
        }
    }
    private (Variable, PseudocodeParser.Error) InterpretExpression(PseudocodeParser.Expression exp, Variables variables, PseudocodeParser.Error error) {
        return InterpretEquation(exp.equation, variables, error);
    }
    private (Variable, PseudocodeParser.Error) InterpretEquation(PseudocodeParser.ParseType exp, Variables variables, PseudocodeParser.Error error) {
        if (error == null) {
            switch (exp) {
                case PseudocodeParser.Operation:
                    PseudocodeParser.Operation opObj = (PseudocodeParser.Operation)exp;
                    switch (opObj.op) {
                        case PseudocodeParser.Operation.Operators.ADD: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(left.type);
                                if (left.type == Variable.Types.FLOAT && right.type != Variable.Types.BOOL) {
                                    switch (right.type) {
                                        case Variable.Types.INT:
                                            var.valueF = left.valueF + right.valueI; break;
                                        case Variable.Types.FLOAT:
                                            var.valueF = left.valueF + right.valueF; break;
                                        case Variable.Types.STRING:
                                            var.type = Variable.Types.STRING;
                                            var.valueS = left.valueF.ToString() + right.valueS; break;
                                    }
                                    return (var, null);
                                }
                                if (left.type == Variable.Types.INT && right.type != Variable.Types.BOOL) {
                                    switch (right.type) {
                                        case Variable.Types.INT:
                                            var.valueI = left.valueI + right.valueI; break;
                                        case Variable.Types.FLOAT:
                                            var.type = Variable.Types.FLOAT;
                                            var.valueF = left.valueI + right.valueF; break;
                                        case Variable.Types.STRING:
                                            var.type = Variable.Types.STRING;
                                            var.valueS = left.valueI.ToString() + right.valueS; break;
                                    }
                                    return (var, null);
                                }
                                if (left.type == Variable.Types.STRING) {
                                    switch (right.type) {
                                        case Variable.Types.INT:
                                            var.valueS = left.valueS + right.valueI.ToString(); break;
                                        case Variable.Types.FLOAT:
                                            var.valueS = left.valueS + right.valueF.ToString(); break;
                                        case Variable.Types.BOOL:
                                            var.valueS = left.valueS + right.valueB.ToString().ToUpper(); break;
                                        case Variable.Types.STRING:
                                            var.valueS = left.valueS + right.valueS; break;
                                    }
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.BOOL)) {
                                    return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use + operation with BOOL type", opObj.pos, opObj.pos));
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use + operation with different types: '" + left.type + "' and '" + right.type + "'", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.SUBTRACT: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(left.type);
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueI = left.valueI - right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueF = left.valueF - right.valueF;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.FLOAT) && (right.type == Variable.Types.INT)) {
                                    var.valueF = left.valueF - right.valueI;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.INT) && (right.type == Variable.Types.FLOAT)) {
                                    var.type = Variable.Types.FLOAT;
                                    var.valueF = left.valueI - right.valueF;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.STRING || left.type == Variable.Types.BOOL)) {
                                    return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use - operation with " + left.type + " type", opObj.pos, opObj.pos));
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use - operation with different types: '" + left.type + "' and '" + right.type + "'", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.MULTIPLY: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(left.type);
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueI = left.valueI * right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueF = left.valueF * right.valueF;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.FLOAT) && (right.type == Variable.Types.INT)) {
                                    var.valueF = left.valueF * right.valueI;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.INT) && (right.type == Variable.Types.FLOAT)) {
                                    var.type = Variable.Types.FLOAT;
                                    var.valueF = left.valueI * right.valueF;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.STRING || left.type == Variable.Types.BOOL)) {
                                    return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use * operation with " + left.type + " type", opObj.pos, opObj.pos));
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use * operation with different types: '" + left.type + "' and '" + right.type + "'", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.DIVIDE: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(left.type);
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueI = left.valueI / right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueF = left.valueF / right.valueF;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.FLOAT) && (right.type == Variable.Types.INT)) {
                                    var.valueF = left.valueF / right.valueI;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.INT) && (right.type == Variable.Types.FLOAT)) {
                                    var.type = Variable.Types.FLOAT;
                                    var.valueF = left.valueI / right.valueF;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.STRING || left.type == Variable.Types.BOOL)) {
                                    return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use / operation with " + left.type + " type", opObj.pos, opObj.pos));
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use / operation with different types: '" + left.type + "' and '" + right.type + "'", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.LESS: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueB = left.valueI < right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueF < right.valueF;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.FLOAT) && (right.type == Variable.Types.INT)) {
                                    var.valueB = left.valueF < right.valueI;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.INT) && (right.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueI < right.valueF;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use < operation with types: '" + left.type + "' and '" + right.type + "', expected INT or FLOAT", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.GREATER: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueB = left.valueI > right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueF > right.valueF;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.FLOAT) && (right.type == Variable.Types.INT)) {
                                    var.valueB = left.valueF > right.valueI;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.INT) && (right.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueI > right.valueF;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use > operation with types: '" + left.type + "' and '" + right.type + "', expected INT or FLOAT", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.LESSEQUALS: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueB = left.valueI <= right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueF <= right.valueF;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.FLOAT) && (right.type == Variable.Types.INT)) {
                                    var.valueB = left.valueF <= right.valueI;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.INT) && (right.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueI <= right.valueF;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use <= operation with types: '" + left.type + "' and '" + right.type + "', expected INT or FLOAT", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.GREATEREQUALS: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueB = left.valueI >= right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueF >= right.valueF;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.FLOAT) && (right.type == Variable.Types.INT)) {
                                    var.valueB = left.valueF >= right.valueI;
                                    return (var, null);
                                }
                                if ((left.type == Variable.Types.INT) && (right.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueI >= right.valueF;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use >= operation with types: '" + left.type + "' and '" + right.type + "', expected INT or FLOAT", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.EQUALS: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (left.type == right.type && (left.type == Variable.Types.BOOL)) {
                                    var.valueB = left.valueB == right.valueB;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueB = left.valueI == right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueF == right.valueF;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.STRING)) {
                                    var.valueB = left.valueS == right.valueS;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use == operation with types: '" + left.type + "' and '" + right.type + "', expected BOOL", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.DIFFERENT: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (left.type == right.type && (left.type == Variable.Types.BOOL)) {
                                    var.valueB = left.valueB != right.valueB;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.INT)) {
                                    var.valueB = left.valueI != right.valueI;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.FLOAT)) {
                                    var.valueB = left.valueF != right.valueF;
                                    return (var, null);
                                }
                                if (left.type == right.type && (left.type == Variable.Types.STRING)) {
                                    var.valueB = left.valueS != right.valueS;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use != operation with types: '" + left.type + "' and '" + right.type + "', expected BOOL", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.AND: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (left.type == right.type && (left.type == Variable.Types.BOOL)) {
                                    var.valueB = left.valueB && right.valueB;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use && operation with types: '" + left.type + "' and '" + right.type + "', expected BOOL", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.OR: {
                                (Variable left, PseudocodeParser.Error err1) = InterpretEquation(opObj.left, variables, error);
                                if (err1 != null) { return (new Variable(Variable.Types.ERROR), err1); }
                                (Variable right, PseudocodeParser.Error err2) = InterpretEquation(opObj.right, variables, error);
                                if (err2 != null) { return (new Variable(Variable.Types.ERROR), err2); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (left.type == right.type && (left.type == Variable.Types.BOOL)) {
                                    var.valueB = left.valueB || right.valueB;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use || operation with types: '" + left.type + "' and '" + right.type + "', expected BOOL", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.NOT: {
                                (Variable right, PseudocodeParser.Error err) = InterpretEquation(opObj.right, variables, error);
                                if (err != null) { return (new Variable(Variable.Types.ERROR), err); }
                                Variable var = new Variable(Variable.Types.BOOL);
                                if (right.type == Variable.Types.BOOL) {
                                    var.valueB = !right.valueB;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use NOT operation with type: '" + right.type + "', expected BOOL", opObj.pos, opObj.pos));
                            }
                        case PseudocodeParser.Operation.Operators.NEGATION: {
                                (Variable right, PseudocodeParser.Error err) = InterpretEquation(opObj.right, variables, error);
                                if (err != null) { return (new Variable(Variable.Types.ERROR), err); }
                                Variable var = new Variable(right.type);
                                if (right.type == Variable.Types.INT) {
                                    var.valueI = -right.valueI;
                                    return (var, null);
                                }
                                if (right.type == Variable.Types.FLOAT) {
                                    var.valueF = -right.valueF;
                                    return (var, null);
                                }
                                return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Cannot use negation operation with type: '" + right.type + "', expected INT or FLOAT", opObj.pos, opObj.pos));
                            }
                        default:
                            return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("An unknown type error has occurred", null, null));
                    }
                case PseudocodeParser.Variable: {
                        PseudocodeParser.Variable varObj = (PseudocodeParser.Variable)exp;
                        if (variables.variables.ContainsKey(varObj.name) && variables.variables[varObj.name].hasValue) {
                            Variable existingVar = variables.variables[varObj.name];
                            if (existingVar.isArray && varObj.arrayIndexExpression == null) {
                                //Array with no indexer
                                Variable var = new Variable(existingVar.type);
                                var.arrayValues = existingVar.arrayValues;
                                var.isArray = true;
                                return (var, null);
                            } else if (existingVar.isArray && varObj.arrayIndexExpression != null) {
                                //Array with indexer
                                (Variable indexVar, PseudocodeParser.Error err) = InterpretExpression(varObj.arrayIndexExpression, variables, error);
                                if (err != null) { return (null, err); }
                                if (indexVar.type != Variable.Types.INT) {
                                    return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Index value has type '" + indexVar.type + "', expected INT", varObj.arrayIndexExpression.lPos, varObj.arrayIndexExpression.rPos));
                                }
                                Variable var = new Variable(ConvertToNonArrayType(existingVar.type));
                                switch (existingVar.type) {
                                    case Variable.Types.INTARRAY:
                                        var.valueI = existingVar.arrayValues.valuesI[indexVar.valueI]; break;
                                    case Variable.Types.FLOATARRAY:
                                        var.valueF = existingVar.arrayValues.valuesF[indexVar.valueI]; break;
                                    case Variable.Types.BOOLARRAY:
                                        var.valueB = existingVar.arrayValues.valuesB[indexVar.valueI]; break;
                                    case Variable.Types.STRINGARRAY:
                                        var.valueS = existingVar.arrayValues.valuesS[indexVar.valueI]; break;
                                }
                                return (var, null);
                            } else {
                                //Non-array
                                Variable var = new Variable(existingVar.type);
                                (var.valueI, var.valueF, var.valueS, var.valueB) = (existingVar.valueI, existingVar.valueF, existingVar.valueS, existingVar.valueB);
                                return (var, null);
                            }
                        } else {
                            return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Variable '" + varObj.name + "' has not been initialised, aka no value has been set", varObj.pos, varObj.pos));
                        }
                    }
                case PseudocodeParser.Value: {
                        PseudocodeParser.Value valObj = (PseudocodeParser.Value)exp;
                        Variable var = new Variable(ConvertType(valObj.type));
                        switch (valObj.type) {
                            case PseudocodeParser.Value.Types.INT:
                                var.valueI = valObj.valueI; break;
                            case PseudocodeParser.Value.Types.FLOAT:
                                var.valueF = valObj.valueF; break;
                            case PseudocodeParser.Value.Types.STRING:
                                var.valueS = valObj.valueS; break;
                            case PseudocodeParser.Value.Types.BOOL:
                                var.valueB = valObj.valueB; break;
                        }
                        return (var, null);
                    }
                case PseudocodeParser.ArrayValue: {
                        PseudocodeParser.ArrayValue valObj = (PseudocodeParser.ArrayValue)exp;
                        (Variable sizeVar, PseudocodeParser.Error err) = InterpretExpression(valObj.sizeExpression, variables, error);
                        if (err != null) { return (null, err); }
                        if (sizeVar.type != Variable.Types.INT) {
                            return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("Array declaration size must be INT type, '" + sizeVar.type + "' provided", valObj.sizeExpression.lPos, valObj.sizeExpression.rPos));
                        }
                        Variable var = new Variable(ConvertType(valObj.type));
                        switch (valObj.type) {
                            case PseudocodeParser.Value.Types.INTARRAY:
                                var.arrayValues.valuesI = new int[sizeVar.valueI]; break;
                            case PseudocodeParser.Value.Types.FLOATARRAY:
                                var.arrayValues.valuesF = new float[sizeVar.valueI]; break;
                            case PseudocodeParser.Value.Types.STRINGARRAY:
                                var.arrayValues.valuesS = new string[sizeVar.valueI]; break;
                            case PseudocodeParser.Value.Types.BOOLARRAY:
                                var.arrayValues.valuesB = new bool[sizeVar.valueI]; break;
                        }
                        var.isArray = true;
                        return (var, null);
                    }
                case PseudocodeParser.Random: {
                        PseudocodeParser.Random randObj = (PseudocodeParser.Random)exp;
                        Variable var = new Variable(ConvertType(randObj.type));
                        switch (randObj.type) {
                            case PseudocodeParser.Value.Types.INT:
                                var.valueI = Random.Range(0, 1000); break;
                            case PseudocodeParser.Value.Types.FLOAT:
                                var.valueF = Random.Range(0.0f, 1000.0f); break;
                            case PseudocodeParser.Value.Types.BOOL:
                                var.valueB = Random.value > 0.5f; break;
                            case PseudocodeParser.Value.Types.STRING:
                                string s = "";
                                string alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                                for (int i = 0; i < 4; i++) {
                                    s += alpha[Random.Range(0, 52)];
                                }
                                var.valueS = s; break;
                        }
                        return (var, null);
                    }
                default:
                    print(exp);
                    return (new Variable(Variable.Types.ERROR), new PseudocodeParser.Error("An unknown type error has occurred", null, null));
            }
        } else {
            return (new Variable(Variable.Types.ERROR), error);
        }
    }

    private Variables RemoveAssignments(Variables vars) {
        Variables newVars = new Variables();
        newVars.variables = new Dictionary<string, Variable>(vars.variables);
        List<string> removeList = new List<string>();
        foreach (KeyValuePair<string, Variable> v in vars.variables) {
            if (v.Value.isInitialise) {
                removeList.Add(v.Key);
            }
        }
        foreach (string key in removeList) {
            newVars.variables.Remove(key);
        }
        return newVars;
    }
    private Variables ClearAssignmentTags(Variables vars, string exception = "") {
        Variables newVars = new Variables();
        newVars.variables = new Dictionary<string, Variable>(vars.variables);
        foreach (KeyValuePair<string, Variable> v in newVars.variables) {
            if (v.Value.isInitialise && v.Key != exception) {
                v.Value.isInitialise = false;
            }
        }
        return newVars;
    }
    private Variable.Types ConvertType(PseudocodeParser.Value.Types type) {
        switch (type) {
            case PseudocodeParser.Value.Types.INT: return Variable.Types.INT;
            case PseudocodeParser.Value.Types.FLOAT: return Variable.Types.FLOAT;
            case PseudocodeParser.Value.Types.STRING: return Variable.Types.STRING;
            case PseudocodeParser.Value.Types.BOOL: return Variable.Types.BOOL;
            case PseudocodeParser.Value.Types.INTARRAY: return Variable.Types.INTARRAY;
            case PseudocodeParser.Value.Types.FLOATARRAY: return Variable.Types.FLOATARRAY;
            case PseudocodeParser.Value.Types.STRINGARRAY: return Variable.Types.STRINGARRAY;
            case PseudocodeParser.Value.Types.BOOLARRAY: return Variable.Types.BOOLARRAY;
        }
        return Variable.Types.ERROR;
    }
    private Variable.Types ConvertToArrayType(Variable.Types type) {
        switch (type) {
            case Variable.Types.INT: return Variable.Types.INTARRAY;
            case Variable.Types.FLOAT: return Variable.Types.FLOATARRAY;
            case Variable.Types.STRING: return Variable.Types.STRINGARRAY;
            case Variable.Types.BOOL: return Variable.Types.BOOLARRAY;
        }
        return Variable.Types.ERROR;
    }
    private Variable.Types ConvertToNonArrayType(Variable.Types type) {
        switch (type) {
            case Variable.Types.INTARRAY: return Variable.Types.INT;
            case Variable.Types.FLOATARRAY: return Variable.Types.FLOAT;
            case Variable.Types.STRINGARRAY: return Variable.Types.STRING;
            case Variable.Types.BOOLARRAY: return Variable.Types.BOOL;
        }
        return Variable.Types.ERROR;
    }
}