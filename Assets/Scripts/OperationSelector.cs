using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OperationSelector : MonoBehaviour
{
    public Color[] rgb;
    private string[] colours = { "red", "green", "blue" };
    private int i = 0;
    public Sprite[] ops;
    private char[] operations = { '+', '-', '*', '=' };
    private int e = 3;
    public Image colImage;
    public Image opImage;
    public Stop stop;
    public InputField text;

    private void Start() {
        stop = GetComponentInParent<Stop>();
        colImage.color = rgb[0];
        stop.operation.colour = colours[0];
        opImage.sprite = ops[3];
        stop.operation.operation = operations[3];
        stop.operation.valueI = 0;
        stop.operation.type = "int";
        UpdateForBranchOp();
    }

    public void ColourChange() {
        i += 1;
        if (i > 2) {
            i = 0;
        }
        colImage.color = rgb[i];
        stop.operation.colour = colours[i];
        UpdateForBranchOp();
    }

    public void OperatorChange() {
        e += 1;
        if (e > 3) {
            e = 0;
        }
        opImage.sprite = ops[e];
        stop.operation.operation = operations[e];
        UpdateForBranchOp();
    }

    public void ValueChange() {
        string t = text.text;
        int i;
        bool b;
        if (int.TryParse(t, out i)) {
            stop.operation.valueI = i;
            stop.operation.type = "int";
        }
        else if (bool.TryParse(t, out b)) {
            stop.operation.valueB = b;
            stop.operation.type = "bool";
        }
        else {
            stop.operation.valueS = t;
            stop.operation.type = "string";
        }
        UpdateForBranchOp();
    }

    private void UpdateForBranchOp() {
        stop.connectedTrack.forBranchOp = stop.operation;
    }
}