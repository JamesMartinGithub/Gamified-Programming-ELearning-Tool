using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour
{
    public GameObject[] panels;
    public Text errorText;
    public GameObject[] explainers;
    public GameObject hintHider;
    public GameObject otherErrorsPanel;
    public Text otherErrorText;
    public TextMeshProUGUI outputText;

    void Start() {
        hintHider.SetActive(true);
    }

    private void ShowPanel(bool b, int i = 0) {
        if (b == false) {
            foreach (GameObject panel in panels) {
                panel.SetActive(false);
            }
        } else {
            for (int e = 0; e < 3; e++) {
                if (e == i) {
                    panels[e].gameObject.SetActive(true);
                } else {
                    panels[e].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowError(string E) {
        errorText.text = E;
        ShowPanel(true, i: 2);
    }

    public void ShowError(List<string> E) {
        errorText.text = E[0];
        ShowPanel(true, i: 2);
        if (E.Count > 1) {
            string otherText = "";
            foreach (string e in E.GetRange(1, E.Count - 1)) {
                otherText += e + '\n';
                otherText += '\n';
            }
            otherErrorText.text = otherText;
            int newlineNum = otherErrorText.text.Split('\n').Length - 1;
            otherErrorsPanel.GetComponent<RectTransform>().offsetMin = new Vector2(-244.8f, 50 - (75 * newlineNum));
        } else {
            otherErrorText.text = "";
            otherErrorsPanel.GetComponent<RectTransform>().offsetMin = new Vector2(-244.8f, 50);
        }
        ShowOutput(new List<string>());
    }

    public void ShowOutput(List<string> output) {
        outputText.text = "";
        foreach(string s in output) { outputText.text += "<color=#909090>></color>" + s + "\n"; }
    }

    public void ResetError() {
        errorText.text = "";
        if (otherErrorText != null) {
            otherErrorText.text = "";
            otherErrorsPanel.GetComponent<RectTransform>().offsetMin = new Vector2(-244.8f, 50);
        }
        ShowPanel(false, 2);
    }

    public void ShowPanel1() {
        if (panels[0].activeSelf) {
            ShowPanel(false, 0);
        } else {
            ShowPanel(true, 0);
        }
    }
    public void ShowPanel2() {
        if (panels[1].activeSelf) {
            ShowPanel(false, 1);
        } else {
            ShowPanel(true, 1);
        }
    }
    public void ShowPanel3() {
        if (panels[2].activeSelf) {
            ShowPanel(false, 2);
        } else {
            ShowPanel(true, 2);
        }
    }

    public void ConstructHover(int index, bool b) {
        explainers[index].SetActive(b);
    }

    public void ShowHint() {
        hintHider.SetActive(false);
    }
}