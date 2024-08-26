using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    public bool spawnable;
    public GameObject trackPrefab;
    public GameObject trackIfPrefab;
    public GameObject trackIfJoinPrefab;
    public GameObject trackForPrefab;
    public GameObject trackForUntilPrefab;

    public GameObject stopAddPrefab;
    public GameObject stopOperationPrefab;

    public GameObject removerPrefab;

    public GameObject shiftSnapImg;
    private TrainLevelController controller;
    public bool placing = false;
    private bool isTrack = true;
    public GameObject border;

    void Start() {
        controller = GameObject.Find("Controller").GetComponent<TrainLevelController>();
        shiftSnapImg.SetActive(false);
        border.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            placing = false;
            shiftSnapImg.SetActive(false);
            border.SetActive(false);
        }
        if (placing) {
            Vector3 mousePos = GameObject.Find("MouseFollow").transform.position;
            int x = Mathf.RoundToInt(mousePos.x);
            int z = -Mathf.RoundToInt(mousePos.z);
            if (isTrack) {
                if (InGrid(x, z) && controller.trackGrid[x, z] == 0 && controller.possibleTrackGrid[x, z].Count > 1) {
                    shiftSnapImg.SetActive(true);
                } else {
                    shiftSnapImg.SetActive(false);
                }
            } else {
                if (InGrid(x, z) && controller.trackGrid[x, z] == 0 && controller.possibleStopGrid[x, z].Count > 1) {
                    shiftSnapImg.SetActive(true);
                } else {
                    shiftSnapImg.SetActive(false);
                }
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

    private void Spawned(bool istrack) {
        if (istrack) {
            isTrack = true;
        } else {
            isTrack = false;
        }
        placing = true;
        border.SetActive(true);
    }

    public void SpawnTrack() {
        if (spawnable) {
            Instantiate(trackPrefab, new Vector3(-10, 0, 0), GameObject.Find("Controller").transform.rotation);
            Spawned(true);
        }
    }
    public void SpawnTrackIf() {
        if (spawnable) {
            Instantiate(trackIfPrefab, new Vector3(-10, 0, 0), GameObject.Find("Controller").transform.rotation);
            Spawned(true);
        }
    }
    public void SpawnTrackIfJoin() {
        if (spawnable) {
            Instantiate(trackIfJoinPrefab, new Vector3(-10, 0, 0), GameObject.Find("Controller").transform.rotation);
            Spawned(true);
        }
    }
    public void SpawnTrackFor() {
        if (spawnable) {
            Instantiate(trackForPrefab, new Vector3(-10, 0, 0), GameObject.Find("Controller").transform.rotation);
            Spawned(true);
        }
    }
    public void SpawnTrackForUntil() {
        if (spawnable) {
            Instantiate(trackForUntilPrefab, new Vector3(-10, 0, 0), GameObject.Find("Controller").transform.rotation);
            Spawned(true);
        }
    }

    public void SpawnStopAdd() {
        if (spawnable) {
            Instantiate(stopAddPrefab, new Vector3(-10, 0, 0), GameObject.Find("Controller").transform.rotation);
            Spawned(false);
        }
    }
    public void SpawnStopOperation() {
        if (spawnable) {
            Instantiate(stopOperationPrefab, new Vector3(-10, 0, 0), GameObject.Find("Controller").transform.rotation);
            Spawned(false);
        }
    }

    public void SpawnRemover() {
        if (spawnable) {
            Instantiate(removerPrefab, new Vector3(-10, 0, 0), GameObject.Find("Controller").transform.rotation);
            Spawned(false);
        }
    }
}