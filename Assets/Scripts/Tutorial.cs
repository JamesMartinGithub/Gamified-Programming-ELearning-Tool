using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public GameObject[] steps;
    private int i = 0;
    private void Start() {
        AllFalse();
        steps[0].SetActive(true);
    }
    public void Next() {
        i++;
        if (i >= steps.Length) {
            gameObject.SetActive(false);
        } else {
            AllFalse();
            steps[i].SetActive(true);
        }
    }
    private void AllFalse() {
        foreach (GameObject step in steps) {
            step.SetActive(false);
        }
    }
}