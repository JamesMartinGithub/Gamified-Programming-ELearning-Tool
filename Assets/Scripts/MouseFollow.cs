using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow : MonoBehaviour
{
    public Camera mainCam;
    public Vector3 unroundedPos;
    public bool outOfBounds;
    private Plane floorPlane;
    private Vector3 outOfBoundsPos = new Vector3(-10,0,0);

    private void Start() {
        floorPlane = new Plane(transform.up, Vector3.zero);
    }


    void Update() {
        Vector3 pos = Input.mousePosition;
        if (pos.y < 149 || pos.y > 1001 || pos.x < 71 || pos.x > 1551) {
            //Out of placement grid bounds
            outOfBounds = true;
            transform.position = outOfBoundsPos;
        }
        else {
            outOfBounds = false;
            Ray ray = mainCam.ScreenPointToRay(pos);
            float enter = 0.0f;
            if (floorPlane.Raycast(ray, out enter)) {
                Vector3 hit = ray.GetPoint(enter);
                transform.position = new Vector3(Mathf.Round(hit.x), 0, Mathf.Round(hit.z));
                unroundedPos = new Vector3(hit.x, 0, hit.z);
            }
        }
    }
}