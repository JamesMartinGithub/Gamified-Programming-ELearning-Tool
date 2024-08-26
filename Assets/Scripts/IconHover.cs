using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IconHover : MonoBehaviour
{
    public int iconIndex;
    public InfoUI infoUI;

    public void HoverEnter() {
        infoUI.ConstructHover(iconIndex, true);
    }

    public void HoverExit() {
        infoUI.ConstructHover(iconIndex, false);
    }
}