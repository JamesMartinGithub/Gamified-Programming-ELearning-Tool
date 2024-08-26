using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMove : MonoBehaviour
{
    void Update() {
        GameObject target = GameObject.Find("TrainTarget");
        if (target != null) {
            transform.position = target.transform.position;
            transform.rotation = target.transform.rotation;
        }
    }

    public void ResetPos() {
        transform.position = new Vector3(0, 0, 1);
        transform.eulerAngles = new Vector3(0, 180, 0);
    }
}