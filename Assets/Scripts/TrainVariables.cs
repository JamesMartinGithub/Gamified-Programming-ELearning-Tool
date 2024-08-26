using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrainVariables
{
    private string[] types = { default, default, default }; // int || bool || string
    private int[] valuesI = { default, default, default };
    private bool[] valuesB = { default, default, default };
    private string[] valuesS = { default, default, default };
    private bool[] initialised = { false, false, false };

    public void AddVariable(string colour, string type) {
        types[ColToIndex(colour)] = type;
    }

    public bool GetInitialised(string colour) {
        return initialised[ColToIndex(colour)];
    }

    public string GetType(string colour) {
        int index = ColToIndex(colour);
        return types[index];
    }

    public void SetType(string colour, string type) {
        int index = ColToIndex(colour);
        types[index] = type;
    }

    public void SetValue(string colour, int valueI = 0, bool valueB = false, string valueS = null) {
        int index = ColToIndex(colour);
        switch (types[index]) {
            case "int":
                valuesI[index] = valueI;
                break;
            case "bool":
                valuesB[index] = valueB;
                break;
            case "string":
                valuesS[index] = valueS;
                break;
        }
        initialised[ColToIndex(colour)] = true;
    }

    public string GetValue(string colour) {
        int index = ColToIndex(colour);
        switch (types[index]) {
            case "int":
                return valuesI[index].ToString();
            case "bool":
                return valuesB[index].ToString();
            case "string":
                return valuesS[index];
        }
        return "null";
    }

    private int ColToIndex(string colour) {
        switch (colour) {
            case "red":
                return 0;
            case "green":
                return 1;
            case "blue":
                return 2;
        }
        return -1;
    }
}