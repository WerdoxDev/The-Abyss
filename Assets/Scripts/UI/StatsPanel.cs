using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class StatsPanel : MonoBehaviour {
    [SerializeField] private GameObject fpsPanel;
    [SerializeField] private GameObject pingPanel;
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private TMP_Text pingText;
    private bool _fpsVisible = true;
    private bool _pingVisible = true;

    private void Awake() {
        NetworkStats.Instance.OnFpsChanged += (fps) => fpsText.text = "FPS:  " + fps;
        NetworkStats.Instance.OnPingChanged += (ping) => pingText.text = "LATENCY:  " + ping + " MS";
        GameManager.Instance.OnGameStateChanged += (state) => {
            SetPingVisibility(_pingVisible && state == GameState.InGame);
        };
        //

        fpsText.text = "FPS:  0";
        pingText.text = "LATENCY:  0 MS";
        SetPingVisibility(false);
    }

    public void SetPingVisibility(bool visible) {
        pingPanel.SetActive(visible && GameManager.Instance.GameState == GameState.InGame);
        _pingVisible = visible;
    }

    public void SetFpsVisibility(bool visible) {
        fpsPanel.SetActive(visible);
        _fpsVisible = visible;
    }
}
