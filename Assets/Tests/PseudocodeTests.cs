using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PseudocodeTests
{
    private PseudocodeInterpreter interpreter;

    [Test]
    public void BooleanComparison()
    {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT (4 / 4 < 2 AND NOT FALSE OR FALSE);";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "TRUE" }, result);
    }
    [Test]
    public void Equation1() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT (1 + (4 - -9 - 4.0) / 6 * 3);";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "5.5" }, result);
    }
    [Test]
    public void Equation2() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT (4-28+3/20/9*1*5/7-2);";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "-26" }, result);
    }
    [Test]
    public void Array() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "float[] fs;\r\nfs = float[5];\r\nfor int i = 0 to 5 do\r\n\tfs[i] = i * 2.0;\r\nendfor\r\nprint fs;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "0", "2", "4", "6", "8" }, result);
    }
    [Test]
    public void ArrayFloat() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "float[] fs;\r\nfs = float[5];\r\nfor int i = 0 to 5 do\r\n\tfs[i] = i * 2.1;\r\nendfor\r\nprint fs;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "0", "2.1", "4.2", "6.3", "8.4" }, result);
    }
    [Test]
    public void Comment() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT 5;\r\n//PRINT 4;\r\nPRINT 3;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "5", "3" }, result);
    }
    [Test]
    public void NestedWhileFor() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "FLOAT f = 0.0;\r\nWHILE f < 5.0 DO\r\n\tFOR INT i = 0 TO 2 DO\r\n\t\tf += 0.5;\r\n\tENDFOR\r\nENDWHILE\r\nPRINT \"Done: \" + f;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "Done: 5" }, result);
    }
    [Test]
    public void NestedFor() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "FOR INT i = 0 TO 6 DO\r\n\tFOR INT e = 0 TO 2 DO\r\n\t\tPRINT i;\r\n\tENDFOR\r\nENDFOR\r\nPRINT \"DONE\";";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "0", "0", "1", "1", "2", "2", "3", "3", "4", "4", "5", "5", "DONE" }, result);
    }
    [Test]
    public void IfInWhile() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "bool b = false;\r\nint i = 0;\r\nwhile b == false do\r\n\ti += 1;\r\n\tif i > 5 then\r\n\t\tb = true;\r\n\tendif\r\nendwhile\r\nprint \"done: \" + i;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "done: 6" }, result);
    }
    [Test]
    public void DanglingElse() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "IF TRUE THEN\r\n\tIF TRUE THEN\r\n\t\tPRINT 0;\r\n\tELSE\r\n\t\tPRINT 1;\r\n\tENDIF\r\nENDIF";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "0" }, result);
    }
    [Test]
    public void BoundryRoundOn() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT 10/5;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "2" }, result);
    }
    [Test]
    public void BoundryRoundLow() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT 10/6;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "1" }, result);
    }
    [Test]
    public void BoundryRoundHigh() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT 10/4;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "2" }, result);
    }
    [Test]
    public void AssignmentMethods() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "INT i = 0;\r\ni += 5;\r\ni = i + 5;\r\nPRINT i;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "10" }, result);
    }
    [Test]
    public void Negation1() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT -6.5--7.0;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "0.5" }, result);
    }
    [Test]
    public void Negation2() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT -6.4-2.0;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "-8.4" }, result);
    }
    [Test]
    public void Brackets1() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT 5.0 / 4.0 + 2.0;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "3.25" }, result);
    }
    [Test]
    public void Brackets2() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT 5.0 / (4.0 + 2.0);";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "0.8333333" }, result);
    }
    [Test]
    public void SyntaxError1() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "PRINT \"yo\"";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(null, result);
    }
    [Test]
    public void SyntaxError2() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "FOR 0 TO 5 DO\r\nPRINT 0;\r\nENDFOR";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(null, result);
    }
    [Test]
    public void SyntaxError3() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "FOR INT i = 0 TO 5 DO\r\nPRINT 0;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(null, result);
    }
    [Test]
    public void TypeError() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        string code = "INT i = 0;\r\nBOOL b = TRUE;\r\nPRINT i/b;";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(null, result);
    }
    [Test]
    public void PartitionAbove() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        int i = Random.Range(51, 100);
        string code = "IF " + i.ToString() +" > 50 THEN\r\nPRINT TRUE;\r\nELSE\r\nPRINT FALSE;\r\nENDIF";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "TRUE" }, result);
    }
    [Test]
    public void PartitionBelow() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        int i = Random.Range(0, 51);
        string code = "IF " + i.ToString() + " > 50 THEN\r\nPRINT TRUE;\r\nELSE\r\nPRINT FALSE;\r\nENDIF";
        List<string> result = interpreter.Interpret(code);
        CollectionAssert.AreEqual(new List<string>() { "FALSE" }, result);
    }
    [Test]
    public void PartitionAll() {
        GameObject obj = new GameObject();
        interpreter = obj.AddComponent<PseudocodeInterpreter>();
        PseudocodeParser parser = obj.AddComponent<PseudocodeParser>();
        interpreter.parser = parser;
        int i = Random.Range(0, 100);
        string code = "IF " + i.ToString() + " > 50 THEN\r\nPRINT TRUE;\r\nELSE\r\nPRINT FALSE;\r\nENDIF";
        List<string> result = interpreter.Interpret(code);
        bool boolRes = true;
        if (i > 50) {
            boolRes = true;
        } else {
            boolRes = false;
        }
        CollectionAssert.AreEqual(new List<string>() { boolRes.ToString().ToUpper() }, result);
    }
}