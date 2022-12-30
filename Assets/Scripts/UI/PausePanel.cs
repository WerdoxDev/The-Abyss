using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PausePanel : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private CustomButton settingsButton;
    [SerializeField] private CustomButton backButton;
    [SerializeField] private CustomButton levaeButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    private Panel _panel;
    private bool menuPanelVisible = false;
    private bool settingsPanelVisible = false;

    private void Awake() {
        _panel = GetComponent<Panel>();

        _panel.OnClosed += () => GameManager.Instance.ResumeGame();

        settingsButton.OnClick += (eventData) => ShowSettingsPanel();
        backButton.OnClick += (eventData) => ShowMenuPanel();
        levaeButton.OnClick += (eventData) => TheAbyssNetworkManager.Instance.Disconnect();

        ShowMenuPanel();
    }

    private void OnDisable() => ShowMenuPanel();

    private void FixedUpdate() {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && _panel.FullyOpened) {
            if (menuPanelVisible) _panel.Close();
            else if (settingsPanelVisible) ShowMenuPanel();
        }
    }

    public void ShowMenuPanel() {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
        menuPanelVisible = true;
        settingsPanelVisible = false;
    }

    public void ShowSettingsPanel() {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        settingsPanelVisible = true;
        menuPanelVisible = false;
    }
}
