using System.Diagnostics;
using TMPro;
using UnityEngine;

public class CaretMover : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public TMP_InputField input;
    private int posOffset = 14;
    public string[] illegalChars;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Edit();
        }
    }

    private void Edit () {
        if (input.isFocused) {
            if (input.text.Length > 0 && input.text[input.text.Length - 1] != '\n') {
                input.text += '\n';
            }
            Vector3 p = Input.mousePosition;
            p.x += posOffset;
            input.text = input.text.Replace(' ', 'Ð');
            int nearestC = TMP_TextUtilities.FindNearestCharacter(textMesh, p, GameObject.Find("Main Camera").GetComponent<Camera>(), false);
            input.text = input.text.Replace('Ð', ' ');
            if (nearestC != -1) {
                input.stringPosition = nearestC;
                input.caretPosition = nearestC;
                input.selectionFocusPosition = nearestC;
                input.selectionStringFocusPosition = nearestC;
                input.selectionStringAnchorPosition = nearestC;
            }
        }
    }

    public void RemoveIllegalChars() {
        foreach (string s in illegalChars) {
            input.text = input.text.Replace(s, "");
        }
    }
}