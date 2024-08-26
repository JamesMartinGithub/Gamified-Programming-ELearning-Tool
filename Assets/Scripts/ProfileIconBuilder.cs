using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIconBuilder : MonoBehaviour
{
    public Sprite[] hair;
    public Sprite[] glasses;
    public Sprite[] moustache;
    public float[] head;
    public Image hairImg; 
    public Image glassesImg;
    public Image moustacheImg;
    public Image headImg;
    private int hairI;
    private int glassesI;
    private int moustacheI;
    private int headI = 0;

    private void Start() {
        hairI = hair.Length - 1;
        glassesI = glasses.Length - 1;
        moustacheI = moustache.Length - 1;
        Redraw();
    }

    public void SetIcon(int[] code) {
        hairI = code[0];
        glassesI = code[1];
        moustacheI = code[2];
        headI = code[3];
        Redraw();
    }

    private void Redraw() {
        if (hair[hairI] == null) { hairImg.enabled = false; } else { hairImg.enabled = true; }
        if (glasses[glassesI] == null) { glassesImg.enabled = false; } else { glassesImg.enabled = true; }
        if (moustache[moustacheI] == null) { moustacheImg.enabled = false; } else { moustacheImg.enabled = true; }
        hairImg.sprite = hair[hairI];
        glassesImg.sprite = glasses[glassesI];
        moustacheImg.sprite = moustache[moustacheI];
        headImg.color = Color.HSVToRGB(0, 0, head[headI]);
        if (head[headI] <= 0.61f) {
            hairImg.color = Color.HSVToRGB(0, 0, 0.4f);
            glassesImg.color = Color.HSVToRGB(0, 0, 0.4f);
            moustacheImg.color = Color.HSVToRGB(0, 0, 0.4f);
        } else {
            hairImg.color = Color.HSVToRGB(0, 0, 1);
            glassesImg.color = Color.HSVToRGB(0, 0, 1);
            moustacheImg.color = Color.HSVToRGB(0, 0, 1);
        }
    }

    private void Save() {
        GameObject.Find("Level Information").GetComponent<DataSaver>().SetIconCode(new int[4] { hairI, glassesI, moustacheI, headI });
    }

    public void BL1() {
        hairI += 1;
        if (hairI >= hair.Length) {
            hairI = 0;
        }
        Redraw();
        Save();
    }
    public void BR1() {
        hairI -= 1;
        if (hairI < 0) {
            hairI = hair.Length - 1;
        }
        Redraw();
        Save();
    }

    public void BL2() {
        glassesI += 1;
        if (glassesI >= glasses.Length) {
            glassesI = 0;
        }
        Redraw();
        Save();
    }
    public void BR2() {
        glassesI -= 1;
        if (glassesI < 0) {
            glassesI = glasses.Length - 1;
        }
        Redraw();
        Save();
    }

    public void BL3() {
        moustacheI += 1;
        if (moustacheI >= moustache.Length) {
            moustacheI = 0;
        }
        Redraw();
        Save();
    }
    public void BR3() {
        moustacheI -= 1;
        if (moustacheI < 0) {
            moustacheI = moustache.Length - 1;
        }
        Redraw();
        Save();
    }

    public void BL4() {
        headI += 1;
        if (headI >= head.Length) {
            headI = 0;
        }
        Redraw();
        Save();
    }
    public void BR4() {
        headI -= 1;
        if (headI < 0) {
            headI = head.Length - 1;
        }
        Redraw();
        Save();
    }
}