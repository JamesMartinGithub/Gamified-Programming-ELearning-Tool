using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySpeedUI : MonoBehaviour
{
    public TrainLevelController controller;
    public GameObject playIcon;
    public GameObject stopIcon;
    public GameObject[] speedIcons;
    private int i = 0;

    public void Play() {
        stopIcon.SetActive(true);
        playIcon.SetActive(false);
        controller.Play();
    }

    public void Stop() {
        playIcon.SetActive(true);
        stopIcon.SetActive(false);
        controller.Stop();
    }

    public void SpeedChange() {
        i += 1;
        if (i > 2) {
            i = 0;
        }
        for (int e = 0; e < 3; e++) {
            if (e != i) {
                speedIcons[e].SetActive(false);
            } else {
                speedIcons[e].SetActive(true);
            }
        }
        controller.SetSpeed(i);
    }
}
