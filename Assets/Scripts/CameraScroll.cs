using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScroll : MonoBehaviour
{
    private float top = -2f;
    private float bottom = -10.37f;

    void Update() {
        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            transform.position = new Vector3(4.3f, 10f, Mathf.Clamp(transform.position.z + (Input.GetAxis("Mouse ScrollWheel") * 4), bottom, top));
        }
    }
}