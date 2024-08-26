using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public bool isInitial = false;
    public string branchId;
    public GameObject[] endStops;
    //public bool hasStop = false;
    public Stop stop;
    public GameObject cross;
    public string type; // straight | curve | if | ifJoin | forStart | forEnd | forUntilStart | forUntilEnd
    public Transform[] possibleTracks;
    public Transform[] possibleStops;
    public GameObject[] visualAlternatives;
    public Transform secondPos;
    public string scope = "";
    //For || if
    public int forRepeats;
    public TrackOperation forBranchOp = new TrackOperation();
    public GameObject forBranchUI;
    public Track forStart;
    //
    private Track lastTrack;
    private Track lastTrack2;
    private Track[] nextTrack = new Track[2];
    private bool placing = true;
    private bool validPos = false;
    private int[,] trackGrid; // 0=empty 1=track 2= notTrack
    private List<Transform>[,] possibleTrackGrid;
    private TrainLevelController controller;
    private int shiftIndex = 0;

    void Start() {
        if (isInitial) {
            placing = false;
            controller = GameObject.Find("Controller").GetComponent<TrainLevelController>();
        } else {
            controller = GameObject.Find("Controller").GetComponent<TrainLevelController>();
            trackGrid = controller.trackGrid;
            possibleTrackGrid = controller.possibleTrackGrid;
            name = "Track (Placing)";
        }
    }

    void Update() {
        if (placing) {
            Vector3 mousePos = GameObject.Find("MouseFollow").transform.position;
            int x = Mathf.RoundToInt(mousePos.x);
            int z = -Mathf.RoundToInt(mousePos.z);
            if (InGrid(x,z) && trackGrid[x,z] == 0 && possibleTrackGrid[x, z].Count >= 1) {
                //Rotate if shift pressed
                if (Input.GetKeyDown(KeyCode.LeftShift)) {
                    shiftIndex += 1;
                    if (((possibleTrackGrid[x, z].Count - 1) - shiftIndex) < 0) {
                        shiftIndex = 0;
                    }
                }
                //Set rotation
                //Transform lastTrackTransform = possibleTrackGrid[x, z][(possibleTrackGrid[x, z].Count - 1) - shiftIndex];
                Transform lastTrackTransform = possibleTrackGrid[x, z][0 + shiftIndex];
                transform.rotation = lastTrackTransform.rotation;
                int x2 = 0;
                int z2 = 0;
                if (secondPos != null) {
                    x2 = Mathf.RoundToInt(secondPos.transform.position.x);
                    z2 = -Mathf.RoundToInt(secondPos.transform.position.z);
                }
                if (((type == "if" || type == "forStart" || type == "forEnd" || type == "forUntilStart" || type == "forUntilEnd" || (type == "ifJoin" && ValidIfJoin(x2, z2, lastTrackTransform).Item1)) && InGrid(x2, z2) && trackGrid[x2, z2] == 0) || (type != "if" && type != "forStart" && type != "forEnd" && type != "forUntilStart" && type != "forUntilEnd" && type != "ifJoin")) {
                    //Valid
                    cross.SetActive(false);
                    transform.position = mousePos;
                    validPos = true;
                    //Set other rotation
                    lastTrack = lastTrackTransform.gameObject.GetComponentInParent<Track>();
                    if (!lastTrack.isInitial) {
                        lastTrack.SetRotation(lastTrackTransform);
                    }
                } else {
                    //Not valid
                    NotValid();
                }
            }
            else {
                //Not valid
                NotValid();
            }
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                if (validPos) {
                    //Place track
                    placing = false;
                    name = "Track (" + x + "," + z + ")";
                    controller.trackGrid[x, z] = 1;
                    int x2 = 0;
                    int z2 = 0;
                    if (secondPos != null) {
                        x2 = Mathf.RoundToInt(secondPos.transform.position.x);
                        z2 = -Mathf.RoundToInt(secondPos.transform.position.z);
                        secondPos.name = "2ndPos (" + x2 + "," + z2 + ")";
                        controller.trackGrid[x2, z2] = 1;
                    }
                    AssignLastTrack();
                    SetEnd(true, transform);
                    //Set possibleStops
                    if (type == "straight") {
                        foreach (Transform stop in possibleStops) {
                            int xs = Mathf.RoundToInt(stop.position.x);
                            int zs = -Mathf.RoundToInt(stop.position.z);
                            if (InGrid(xs, zs)) {
                                controller.possibleStopGrid[xs, zs].Add(stop);
                            }
                        }
                    }
                }
                else {
                    //Remove Track
                    Destroy(gameObject);
                }
            }
        }
    }

    private (bool, Transform) ValidIfJoin(int x2, int z2, Transform leftTrack) {
        string GetFirstPart(string s) {
            if (s.Length <= 1) {
                return "";
            } else {
                return s.Substring(0, s.Length - 1);
            }
        }
        if (InGrid(x2, z2) &&  possibleTrackGrid[x2, z2].Count >= 1) {
            foreach (Transform possible in possibleTrackGrid[x2, z2]) {
                string id1 = possible.GetComponentInParent<Track>().branchId;
                string id2 = leftTrack.gameObject.GetComponentInParent<Track>().branchId;
                string p1 = GetFirstPart(id1);
                string p2 = GetFirstPart(id2);
                if (id1.Length == id2.Length && p1 == p2) {
                    return (true, possible);
                }
            }
        }
        return (false, null);
    }

    private void SetEnd(bool b, Transform t) {
        if (type == "if") {
            //Special case for ifs, only 1 side should be changed, unless being placed
            if (b) {
                if (t == transform) {
                    foreach (GameObject endStop in endStops) {
                        endStop.SetActive(b);
                    }
                } else {
                    if (ArePosSame(t, possibleTracks[0])) {
                        endStops[0].SetActive(b);
                    } else {
                        endStops[1].SetActive(b);
                    }
                }
            } else {
                if (t == possibleTracks[0]) {
                    endStops[0].SetActive(b);
                }
                if (t == possibleTracks[1]) {
                    endStops[1].SetActive(b);
                }
            }
        } else {
            foreach (GameObject endStop in endStops) {
                endStop.SetActive(b);
            }
        }
    }

    private bool ArePosSame(Transform trans1, Transform trans2) {
        int x1 = Mathf.RoundToInt(trans1.position.x);
        int z1 = -Mathf.RoundToInt(trans1.position.z);
        int x2 = Mathf.RoundToInt(trans2.position.x);
        int z2 = -Mathf.RoundToInt(trans2.position.z);
        if (x1 == x2 && z1 == z2) {
            return true;
        } else {
            return false;
        }
    }

    private void NotValid() {
        validPos = false;
        cross.SetActive(true);
        transform.position = GameObject.Find("MouseFollow").GetComponent<MouseFollow>().unroundedPos;
        shiftIndex = 0;
    }

    private void AssignLastTrack(){
        int x = Mathf.RoundToInt(transform.position.x);
        int z = -Mathf.RoundToInt(transform.position.z);
        if (x == 0 && z == 0) {
            //Assign lastTrack to initial track
            lastTrack = controller.initialTrack;
            lastTrack.SetNextTrack(this, controller.initialTrackPossible);
        }
        else {
            //Transform lastTrackTransform = possibleTrackGrid[x, z][(possibleTrackGrid[x, z].Count - 1) - shiftIndex];
            Transform lastTrackTransform = possibleTrackGrid[x, z][0 + shiftIndex];
            lastTrack = lastTrackTransform.gameObject.GetComponentInParent<Track>();
            lastTrack.SetNextTrack(this, lastTrackTransform);
            //Straight -> curve check for previous track
            lastTrack.CheckForCurve();
            if (type == "ifJoin") {
                int x2 = Mathf.RoundToInt(secondPos.position.x);
                int z2 = -Mathf.RoundToInt(secondPos.position.z);
                Transform lastTrack2Transform = ValidIfJoin(x2, z2, lastTrackTransform).Item2;
                lastTrack2 = lastTrack2Transform.gameObject.GetComponentInParent<Track>();
                lastTrack2.SetNextTrack(this, lastTrack2Transform);
                //Straight -> curve check for previous track
                lastTrack2.CheckForCurve();
            }
        }
        //Add possibilities
        foreach (Transform pos in possibleTracks) {
            int xp = Mathf.RoundToInt(pos.position.x);
            int zp = -Mathf.RoundToInt(pos.position.z);
            if (InGrid(xp, zp) && !controller.possibleTrackGrid[xp, zp].Contains(pos)) {
                controller.possibleTrackGrid[xp, zp].Add(pos);
            }
        }
        //Assign this branchId based on previous track
        if (type == "ifJoin") {
            if (branchId.Length <= 1) {
                branchId = "";
            } else {
                branchId = branchId.Substring(0, branchId.Length - 1);
            }
        } else {
            if (lastTrack.type == "if") {
                if (ArePosSame(transform, lastTrack.possibleTracks[0])) {
                    branchId = lastTrack.branchId + "0";
                } else {
                    branchId = lastTrack.branchId + "1";
                }
            } else {
                branchId = lastTrack.branchId;
            }
        }
        //Assign this forScope based on previous track
        SetScope();
    }

    private bool InGrid(int x, int z) {
        if (z >= 0 && z <= 12 && x >= 0 && x <= 7) {
            return true;
        }
        else {
            return false;
        }
    }

    private void SetScope() {
        string last = lastTrack.scope;
        switch (type) {
            case "straight":
            case "curve":
                scope = last;
                break;
            case "if":
                scope = last + "i";
                //Show compOperator selector
                forBranchUI.transform.eulerAngles = new Vector3(90, 0, 0);
                forBranchUI.SetActive(true);
                break;
            case "ifJoin":
                scope = last.Remove(last.Length - 1);
                break;
            case "forStart":
            case "forEnd":
            case "forUntilStart":
            case "forUntilEnd":
                if (last.Length == 0 || last[last.Length - 1] == 'i' || ((type == "forUntilStart" || type == "forUntilEnd") && last[last.Length - 1] == 'f') || ((type == "forStart" || type == "forEnd") && last[last.Length - 1] == 'u')) {
                    if (type == "forStart" || type == "forEnd") {
                        scope = last + "f";
                        type = "forStart";
                    } else {
                        scope = last + "u";
                        type = "forUntilStart";
                    }
                    visualAlternatives[0].SetActive(true);
                    visualAlternatives[1].SetActive(false);
                    //Show repeat count selector
                    forBranchUI.transform.eulerAngles = new Vector3(90, 0, 0);
                    forBranchUI.SetActive(true);
                }
                else {
                    scope = last.Remove(last.Length - 1);
                    visualAlternatives[0].SetActive(false);
                    visualAlternatives[1].SetActive(true);
                    if (type == "forStart" || type == "forEnd") {
                        type = "forEnd";
                        SetForStart(lastTrack, "forStart");
                    } else {
                        type = "forUntilEnd";
                        SetForStart(lastTrack, "forUntilStart");
                    }
                    //Draw line to forStart
                    gameObject.GetComponent<LineRenderer>().SetPosition(0, new Vector3(secondPos.position.x, -1, secondPos.position.z));
                    gameObject.GetComponent<LineRenderer>().SetPosition(1, new Vector3(forStart.secondPos.position.x, -1, forStart.secondPos.position.z));
                }
                break;
        }
    }

    //Recurse until forStart found
    private void SetForStart(Track last, string fStart) {
        if (last.type == fStart && last.branchId == branchId) {
            forStart = last;
        }else if (last.isInitial) {
            print("ERROR: No for(Until)Start found, from for(Until)End");
        } else {
            SetForStart(last.lastTrack, fStart);
        }
    }

    public void SetNextTrack(Track track, Transform possibleTrack) {
        if (track == null) {
            if (type == "if") {
                if (ArePosSame(possibleTrack, possibleTracks[0])) {
                    nextTrack[0] = null;
                } else {
                    nextTrack[1] = null;
                }
            } else {
                nextTrack = new Track[2];
            }
        } else {
            SetEnd(false, possibleTrack);
            if (isInitial) {
                nextTrack[0] = track;
                //Clear possible
                controller.possibleTrackGrid[0, 0] = new List<Transform>();
            } else {
                switch (type) {
                    case "straight":
                    case "curve": {
                            nextTrack[0] = track;
                            //Change curve
                            SetRotation(possibleTrack);
                            //Clear possibles
                            foreach (Transform pos in possibleTracks) {
                                int xp = Mathf.RoundToInt(pos.position.x);
                                int zp = -Mathf.RoundToInt(pos.position.z);
                                if (InGrid(xp, zp)) {
                                    controller.possibleTrackGrid[xp, zp].Remove(pos);
                                }
                            }
                            break;
                        }
                    case "if": {
                            if (possibleTrack == possibleTracks[0]) {
                                nextTrack[0] = track;
                            }
                            if (possibleTrack == possibleTracks[1]) {
                                nextTrack[1] = track;
                            }
                            //Clear possible
                            int xp = Mathf.RoundToInt(possibleTrack.position.x);
                            int zp = -Mathf.RoundToInt(possibleTrack.position.z);
                            if (InGrid(xp, zp)) {
                                controller.possibleTrackGrid[xp, zp].Remove(possibleTrack);
                            }
                            break;
                        }
                    case "ifJoin": {
                            nextTrack[0] = track;
                            //Clear possible
                            int xp = Mathf.RoundToInt(possibleTrack.position.x);
                            int zp = -Mathf.RoundToInt(possibleTrack.position.z);
                            if (InGrid(xp, zp)) {
                                controller.possibleTrackGrid[xp, zp].Remove(possibleTrack);
                            }
                            break;
                        }
                    case "forStart":
                    case "forEnd":
                    case "forUntilStart":
                    case "forUntilEnd": {
                            nextTrack[0] = track;
                            //Clear possible
                            Transform pos = possibleTracks[0];
                            int xp = Mathf.RoundToInt(pos.position.x);
                            int zp = -Mathf.RoundToInt(pos.position.z);
                            if (InGrid(xp, zp)) {
                                controller.possibleTrackGrid[xp, zp].Remove(pos);
                            }
                            break;
                        }
                }
            }
        }
    }

    public Track[] GetNextTrack() {
        return nextTrack;
    
    }

    public void SetRotation(Transform possibleTrack) {
        if (type == "straight" || type == "curve") {
            if (possibleTrack == possibleTracks[0]) {
                visualAlternatives[0].SetActive(true);
                visualAlternatives[1].SetActive(false);
                visualAlternatives[2].SetActive(false);
            }
            if (possibleTrack == possibleTracks[1]) {
                visualAlternatives[0].SetActive(false);
                visualAlternatives[1].SetActive(true);
                visualAlternatives[2].SetActive(false);
            }
            if (possibleTrack == possibleTracks[2]) {
                visualAlternatives[0].SetActive(false);
                visualAlternatives[1].SetActive(false);
                visualAlternatives[2].SetActive(true);
            }
        }
    }

    public void CheckForCurve() {
        if (type == "straight" && !visualAlternatives[1].activeSelf) {
            type = "curve";
            ClearPossibleStops();
        }
    }

    public void ClearPossibleStops() {
        //Remove possibleStops
        foreach (Transform stop in possibleStops) {
            int xs = Mathf.RoundToInt(stop.position.x);
            int zs = -Mathf.RoundToInt(stop.position.z);
            if (InGrid(xs, zs)) {
                controller.possibleStopGrid[Mathf.RoundToInt(stop.position.x), -Mathf.RoundToInt(stop.position.z)].Remove(stop);
            }
        }
    }

    public void SetStop(Stop s) {
        stop = s;
        //Remove possibleTracks on sides
        foreach (Transform pos in possibleTracks) {
            if (pos != possibleTracks[1]) {
                int xp = Mathf.RoundToInt(pos.position.x);
                int zp = -Mathf.RoundToInt(pos.position.z);
                if (InGrid(xp, zp)) {
                    controller.possibleTrackGrid[xp, zp].Remove(pos);
                }
            }
        }
        //Ensure track is straight
        visualAlternatives[0].SetActive(false);
        visualAlternatives[1].SetActive(true);
        visualAlternatives[2].SetActive(false);
    }

    public float PlayAnimation(bool branch = false, string forState = "straight") {
        float speedMult = controller.speedMult;
        Animation anim = GetComponent<Animation>();
        if (isInitial) {
            anim["TrackStraightInitial"].speed = speedMult;
            anim.Play("TrackStraightInitial");
            return 2f / speedMult;
        } else {
            switch (type) {
                case "straight":
                    if (stop != null) {
                        anim["TrackStraightStop"].speed = speedMult;
                        anim.Play("TrackStraightStop");
                        return 4f / speedMult;
                    } else {
                        anim["TrackStraight"].speed = speedMult;
                        anim.Play("TrackStraight");
                        return 2f / speedMult;
                    }
                case "curve":
                    if (visualAlternatives[0].activeSelf) {
                        anim["TrackCurveL"].speed = speedMult;
                        anim.Play("TrackCurveL");
                        return 2f / speedMult;
                    } else {
                        anim["TrackCurveR"].speed = speedMult;
                        anim.Play("TrackCurveR");
                        return 2f / speedMult;
                    }
                case "if":
                    if (branch) {
                        anim["TrackIfBranch"].speed = speedMult;
                        anim.Play("TrackIfBranch");
                        return 4f / speedMult;
                    } else {
                        anim["TrackIfStraight"].speed = speedMult;
                        anim.Play("TrackIfStraight");
                        return 2f / speedMult;
                    }
                case "ifJoin":
                    if (branch) {
                        anim["TrackIfJoin"].speed = speedMult;
                        anim.Play("TrackIfJoin");
                        return 4f / speedMult;
                    } else {
                        anim["TrackIfStraight"].speed = speedMult;
                        anim.Play("TrackIfStraight");
                        return 2f / speedMult;
                    }
                case "forStart":
                case "forUntilStart":
                    if (forState == "straight") {
                        anim["TrackForStraight"].speed = speedMult;
                        anim.Play("TrackForStraight");
                        return 2f / speedMult;
                    } else {
                        anim["TrackForStart"].speed = speedMult;
                        anim.Play("TrackForStart");
                        return 3f / speedMult;
                    }
                case "forEnd":
                case "forUntilEnd":
                    if (forState == "straight") {
                        anim["TrackForStraight"].speed = speedMult;
                        anim.Play("TrackForStraight");
                        return 2f / speedMult;
                    } else {
                        anim["TrackForEnd"].speed = speedMult;
                        anim.Play("TrackForEnd");
                        return 3f / speedMult;
                    }
            }
        }
        return 0f;
    }

    public void ActivateChildCrosses() {
        cross.SetActive(true);
        if (stop != null) {
            stop.cross.SetActive(true);
        }
        foreach (Track track in nextTrack) {
            if (track != null) {
                track.ActivateChildCrosses();
            }
        }
    }

    public void RemoveTrack(bool fromTrack = false) {
        foreach (Track track in nextTrack) {
            if (track != null) {
                track.RemoveTrack(fromTrack: true);
            }
        }
        //Clear grid positions
        int x = Mathf.RoundToInt(transform.position.x);
        int z = -Mathf.RoundToInt(transform.position.z);
        controller.trackGrid[x, z] = 0;
        if (secondPos != null) {
            int x2 = Mathf.RoundToInt(secondPos.position.x);
            int z2 = -Mathf.RoundToInt(secondPos.position.z);
            controller.trackGrid[x2, z2] = 0;
        }
        //Clear possible tracks
        foreach (Transform pos in possibleTracks) {
            int xp = Mathf.RoundToInt(pos.position.x);
            int zp = -Mathf.RoundToInt(pos.position.z);
            if (InGrid(xp, zp)) {
                controller.possibleTrackGrid[xp, zp].Remove(pos);
            }
        }
        //Clear possible stops
        foreach (Transform stop in possibleStops) {
            int xs = Mathf.RoundToInt(stop.position.x);
            int zs = -Mathf.RoundToInt(stop.position.z);
            if (InGrid(xs, zs)) {
                controller.possibleStopGrid[xs, zs].Remove(stop);
            }
        }
        //Clear Stops
        if (stop != null) {
            stop.RemoveStop(fromTrack: true);
        }
        //Remove last node
        if (fromTrack == false) {
            lastTrack.SetNextTrack(null, transform);
            if (lastTrack2 != null) {
                lastTrack2.SetNextTrack(null, transform);
            }
            //Re-add last track possible stops (unless its a straight track with a stop)
            if (lastTrack.stop == null) {
                foreach (Transform pos in lastTrack.possibleTracks) {
                    int xp = Mathf.RoundToInt(pos.position.x);
                    int zp = -Mathf.RoundToInt(pos.position.z);
                    if (InGrid(xp, zp) && !controller.possibleTrackGrid[xp, zp].Contains(pos)) {
                        controller.possibleTrackGrid[xp, zp].Add(pos);
                    }
                }
            } else {
                //If track with stop, only readd forward possibleTrack
                Transform pos = lastTrack.possibleTracks[1];
                int xp = Mathf.RoundToInt(pos.position.x);
                int zp = -Mathf.RoundToInt(pos.position.z);
                if (InGrid(xp, zp) && !controller.possibleTrackGrid[xp, zp].Contains(pos)) {
                    controller.possibleTrackGrid[xp, zp].Add(pos);
                }
            }
            if (lastTrack2 != null) {
                if (lastTrack2.stop == null) {
                    foreach (Transform pos in lastTrack2.possibleTracks) {
                        int xp = Mathf.RoundToInt(pos.position.x);
                        int zp = -Mathf.RoundToInt(pos.position.z);
                        if (InGrid(xp, zp) && !controller.possibleTrackGrid[xp, zp].Contains(pos)) {
                            controller.possibleTrackGrid[xp, zp].Add(pos);
                        }
                    }
                } else {
                    //If track with stop, only readd forward possibleTrack
                    Transform pos = lastTrack2.possibleTracks[1];
                    int xp = Mathf.RoundToInt(pos.position.x);
                    int zp = -Mathf.RoundToInt(pos.position.z);
                    if (InGrid(xp, zp) && !controller.possibleTrackGrid[xp, zp].Contains(pos)) {
                        controller.possibleTrackGrid[xp, zp].Add(pos);
                    }
                }
            }
            //Re-enable last track stopper
            lastTrack.SetEnd(true, transform);
        }
        Destroy(gameObject);
    }
}