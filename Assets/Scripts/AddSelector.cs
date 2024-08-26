using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddSelector : MonoBehaviour
{
    public Color[] rgb;
    private string[] colours = { "red", "green", "blue" };
    private int i = 0;
    public Image colImage;
    public Stop stop;
    public Dropdown drop;

    private void Start() {
        stop = GetComponentInParent<Stop>();
        colImage.color = rgb[0];
        stop.operation.colour = colours[0];
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

    public void TypeChange() {
        switch (drop.value) {
            case 0:
                stop.operation.type = "int";
                break;
            case 1:
                stop.operation.type = "bool";
                break;
            case 2:
                stop.operation.type = "string";
                break;
        }
        UpdateForBranchOp();
    }

    private void UpdateForBranchOp() {
        stop.connectedTrack.forBranchOp = stop.operation;
    }
}