using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainVariableDisplay : MonoBehaviour
{
    public RectTransform bg;
    public GameObject[] colours; 
    public Text[] types;
    public Text[] values;
    public Image[] cars;
    public Color[] rgb;
    private float[] bgHeights = { 856.265f, 717.565f, 578.92f };
    private TrackInterpreter interpreter;

    private void Start() {
        interpreter = GameObject.Find("Controller").GetComponent<TrackInterpreter>();
        bg.offsetMin = new Vector2(bg.offsetMin.x, bgHeights[0]);
        foreach (var colour in colours) {
            colour.SetActive(false);
        }
    }

    public void RefreshAfter(float time) {
        Invoke("Refresh", time);
    }

    public void Refresh() {
        TrainVariables variables = interpreter.variables;
        List<string> activeVars = interpreter.activeVars;
        for (int i = 0; i < 3; i++) {
            if (activeVars.Count > i) {
                bg.offsetMin = new Vector2(bg.offsetMin.x, bgHeights[i]);
                colours[i].SetActive(true);
                types[i].text = variables.GetType(activeVars[i]);
                if (variables.GetInitialised(activeVars[i])) {
                    values[i].text = variables.GetValue(activeVars[i]);
                } else {
                    values[i].text = "";
                }
                switch (activeVars[i]) {
                    case "red":
                        cars[i].color = rgb[0];
                        break;
                    case "green":
                        cars[i].color = rgb[1];
                        break;
                    case "blue":
                        cars[i].color = rgb[2];
                        break;
                }
            } else {
                colours[i].SetActive(false);
            }
        }
    }
}