using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComparisonSelector : MonoBehaviour
{
    public Color[] rgb;
    private string[] colours = { "red", "green", "blue" };
    private int i = 0;
    public Sprite[] ops;
    private char[] operations = { '=', '>', '<' };
    private int e = 0;
    public Image colImage;
    public Image opImage;
    public Track track;
    public InputField text;

    private void Start() {
        colImage.color = rgb[0];
        track.forBranchOp.colour = colours[0];
        opImage.sprite = ops[0];
        track.forBranchOp.compOperation = operations[0];
        track.forBranchOp.valueI = 0;
        track.forBranchOp.type = "int";
    }

    public void ColourChange() {
        i += 1;
        if (i > 2) {
            i = 0;
        }
        colImage.color = rgb[i];
        track.forBranchOp.colour = colours[i];
    }

    public void OperatorChange() {
        e += 1;
        if (e > 2) {
            e = 0;
        }
        opImage.sprite = ops[e];
        track.forBranchOp.compOperation = operations[e];
    }

    public void ValueChange() {
        string t = text.text;
        int i;
        bool b;
        if (int.TryParse(t, out i)) {
            track.forBranchOp.valueI = i;
            track.forBranchOp.type = "int";
        } else if (bool.TryParse(t, out b)) {
            track.forBranchOp.valueB = b;
            track.forBranchOp.type = "bool";
        } else {
            track.forBranchOp.valueS = t;
            track.forBranchOp.type = "string";
        }
    }
}