using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public class Level {
        public int id = -1;
        public bool isTrain;
        public bool showTutorial = false;
        public bool isSandbox = false;
        public string task;
        public List<string> taskcode;
        public string constraints;
        public string musthavecode;
        public string limitscode;
        public string hint;
    }
    public Level level;

    private void Start() {
        DontDestroyOnLoad(this);
    }
}