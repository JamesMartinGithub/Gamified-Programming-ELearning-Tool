using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Stop : MonoBehaviour
{
    public GameObject UI;
    public GameObject cross;
    public TrackOperation operation = new TrackOperation();
    public Track connectedTrack;
    private int[,] trackGrid; // 0=empty 1=track 2= notTrack
    private List<Transform>[,] possibleStopGrid;
    private TrainLevelController controller;
    private bool placing = true;
    private bool validPos = false;
    private int shiftIndex = 0;

    void Start() {
        controller = GameObject.Find("Controller").GetComponent<TrainLevelController>();
        trackGrid = controller.trackGrid;
        possibleStopGrid = controller.possibleStopGrid;
        name = "Stop (Placing)";
    }

    void Update() {
        if (placing) {
            Vector3 mousePos = GameObject.Find("MouseFollow").transform.position;
            int x = Mathf.RoundToInt(mousePos.x);
            int z = -Mathf.RoundToInt(mousePos.z);
            if (InGrid(x, z) && trackGrid[x, z] == 0 && possibleStopGrid[x, z].Count >= 1) {
                //Rotate if shift pressed
                if (Input.GetKeyDown(KeyCode.LeftShift)) {
                    shiftIndex += 1;
                    if (shiftIndex > (possibleStopGrid[x, z].Count - 1)) {
                        shiftIndex = 0;
                    }
                }
                //Valid
                cross.SetActive(false);
                transform.position = mousePos;
                validPos = true;
                //Set rotation
                Transform lastTrackTransform = possibleStopGrid[x, z][0 + shiftIndex];
                transform.rotation = lastTrackTransform.rotation;
            }
            else {
                //Not valid
                NotValid();
            }
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                if (validPos) {
                    //Place stop
                    placing = false;
                    name = "Stop (" + x + "," + z + ")";
                    controller.trackGrid[x, z] = 1;
                    connectedTrack = possibleStopGrid[x, z][0 + shiftIndex].gameObject.GetComponentInParent<Track>();
                    connectedTrack.SetStop(this);
                    //Remove possibleStops from connected track
                    connectedTrack.ClearPossibleStops();
                    UI.transform.eulerAngles = new Vector3(90, 0, 0);
                    UI.SetActive(true);
                }
                else {
                    //Remove Stop
                    Destroy(gameObject);
                }
            }
        }
    }

    private void NotValid() {
        validPos = false;
        cross.SetActive(true);
        transform.position = GameObject.Find("MouseFollow").GetComponent<MouseFollow>().unroundedPos;
        shiftIndex = 0;
    }

    private bool InGrid(int x, int z) {
        if (z >= 0 && z <= 12 && x >= 0 && x <= 7) {
            return true;
        }
        else {
            return false;
        }
    }

    public void RemoveStop(bool fromTrack = false) {
        if (!fromTrack) {
            //Clear possible stop
            connectedTrack.stop = null;
            //Readd track possible stops
            foreach (Transform stop in connectedTrack.possibleStops) {
                int xs = Mathf.RoundToInt(stop.position.x);
                int zs = -Mathf.RoundToInt(stop.position.z);
                if (InGrid(xs, zs) && !controller.possibleStopGrid[xs, zs].Contains(stop)) {
                    controller.possibleStopGrid[xs, zs].Add(stop);
                }
            }
            if (connectedTrack.GetNextTrack()[0] == null) {
                //Readd track possible tracks
                foreach (Transform track in connectedTrack.possibleTracks) {
                    if (track != connectedTrack.possibleTracks[1]) {
                        int xs = Mathf.RoundToInt(track.position.x);
                        int zs = -Mathf.RoundToInt(track.position.z);
                        if (InGrid(xs, zs) && !controller.possibleTrackGrid[xs, zs].Contains(track)) {
                            controller.possibleTrackGrid[xs, zs].Add(track);
                        }
                    }
                }
            }
        }
        //Clear grid position
        int x = Mathf.RoundToInt(transform.position.x);
        int z = -Mathf.RoundToInt(transform.position.z);
        controller.trackGrid[x, z] = 0;
        Destroy(gameObject);
    }
}
