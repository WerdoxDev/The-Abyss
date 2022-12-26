using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : MonoBehaviour {
    private Panel _panel;

    private void Awake() {
        _panel = GetComponent<Panel>();

        _panel.OnClose += () => GameManager.Instance.ResumeGame();
    }
}
