using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsPanel : MonoBehaviour {
    [SerializeField] private GameObject fpsPanel;
    [SerializeField] private GameObject pingPanel;
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private TMP_Text pingText;
    private bool _fpsVisible = true;
    private bool _pingVisible = true;

    private void Awake() {
        fpsText.text = "FPS:  0";
        pingText.text = "LATENCY:  0 MS";
        SetPingVisibility(false);

        GameManager.Instance.OnFpsChanged += (fps) => fpsText.text = "FPS:  " + fps;
        GameManager.Instance.OnPingChanged += (ping) => pingText.text = "LATENCY:  " + ping + " MS";
        GameManager.Instance.OnPlayerSpawned += (Player player, bool isOwner) => {
            if (isOwner) SetPingVisibility(_pingVisible);
        };
    }

    public void SetPingVisibility(bool visible) {
        pingPanel.SetActive(visible);
    }

    public void SetFpsVisibility(bool visible) {
        fpsPanel.SetActive(visible);
    }
}
