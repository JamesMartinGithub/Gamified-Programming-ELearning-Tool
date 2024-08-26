using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackRemover : MonoBehaviour
{
    public GameObject icon;
    private TrainLevelController controller;
    private int[,] trackGrid; // 0=empty 1=track 2= notTrack
    private (int, int) selectedCoords;
    private MouseFollow mouseFollow;
    private Track selectedTrack;
    private Stop selectedStop;

    void Start() {
        controller = GameObject.Find("Controller").GetComponent<TrainLevelController>();
        trackGrid = controller.trackGrid;
        mouseFollow = GameObject.Find("MouseFollow").GetComponent<MouseFollow>();

    }

    void Update() {
        Vector3 mousePos = mouseFollow.unroundedPos;
        transform.position = mousePos;
        int x = Mathf.RoundToInt(mousePos.x);
        int z = -Mathf.RoundToInt(mousePos.z);
        if (InGrid(x, z) && trackGrid[x, z] == 1) {
            //Track here
            icon.SetActive(false);
            if ((x, z) != selectedCoords) {
                selectedCoords = (x, z);
                DisableCrosses();
                //Find selected track and activate crosses
                GameObject selectedObj = GameObject.Find("Track (" + x + "," + z + ")");
                selectedTrack = null;
                selectedStop = null;
                if (selectedObj != null) {
                    selectedTrack = selectedObj.GetComponent<Track>();
                    selectedTrack.ActivateChildCrosses();
                } else {
                    selectedObj = GameObject.Find("2ndPos (" + x + "," + z + ")");
                    if (selectedObj != null) {
                        selectedTrack = selectedObj.GetComponentInParent<Track>();
                        selectedTrack.ActivateChildCrosses();
                    } else {
                        selectedObj = GameObject.Find("Stop (" + x + "," + z + ")");
                        if (selectedObj != null) {
                            selectedStop = selectedObj.GetComponent<Stop>();
                            selectedStop.cross.SetActive(true);
                        }
                    }
                }
            }
        } else {
            //No track here
            if (mouseFollow.outOfBounds) {
                icon.SetActive(false);
            } else {
                icon.SetActive(true);
            }
            if ((x, z) != selectedCoords) {
                selectedCoords = (x, z);
                DisableCrosses();
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            if (InGrid(x, z) && trackGrid[x, z] == 1 && (selectedTrack != null || selectedStop != null)) {
                if (selectedTrack != null) {
                    selectedTrack.RemoveTrack();
                } else {
                    selectedStop.RemoveStop();
                }
            } else {
                Destroy(gameObject);
            }
        }
    }

    private bool InGrid(int x, int z) {
        if (z >= 0 && z <= 12 && x >= 0 && x <= 7) {
            return true;
        } else {
            return false;
        }
    }

    private void DisableCrosses() {
        foreach (Track track in FindObjectsOfType<Track>()) {
            if (track.cross != null) {
                track.cross.SetActive(false);
            }
        }
        foreach (Stop stop in FindObjectsOfType<Stop>()) {
            if (stop.cross != null) {
                stop.cross.SetActive(false);
            }
        }
    }
}