using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public int id;
    public bool isTrain;
    public bool showTutorial;
    public string task;
    public List<string> taskcode;
    public string constraints;
    public string musthavecode;
    public string limitscode;
    public string hint;

    public void Load() {
        LevelInfo.Level level = new LevelInfo.Level();
        level.id = id;
        level.isTrain = this.isTrain;
        level.task = this.task;
        level.taskcode = this.taskcode;
        level.constraints = this.constraints;
        level.musthavecode = this.musthavecode;
        level.limitscode = this.limitscode;
        level.hint = this.hint;
        level.showTutorial = this.showTutorial;
        GameObject.Find("Controller").GetComponent<MenuController>().LoadLevel(level);
    }
}