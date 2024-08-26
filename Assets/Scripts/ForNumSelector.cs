using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForNumSelector : MonoBehaviour
{
    public Track track;
    public Text text;
    private int n = 1;

    private void Start() {
        track.forRepeats = 0;
    }

    public void Plus() {
        n = Mathf.Min(n + 1, 10);
        track.forRepeats = n - 1;
        text.text = n.ToString();
    }

    public void Minus() {
        n = Mathf.Max(n - 1, 1);
        track.forRepeats = n - 1;
        text.text = n.ToString();
    }
}