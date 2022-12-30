using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SettingsPanel : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private GameObject videoBasicContent;
    [SerializeField] private GameObject videoAdvancedContent;
    [SerializeField] private CustomButton videoBasicButton;
    [SerializeField] private CustomButton videoAdvancedButton;
    [SerializeField] private Selectable selectableOnPromptClose;

    [Header("Settings Options")]
    [SerializeField] private SettingsOption presetOption;
    [SerializeField] private SettingsOption resolutionOption;
    [SerializeField] private SettingsOption renderResolutionOption;
    [SerializeField] private SettingsOption windowedOption;
    [SerializeField] private SettingsOption showFpsOption;
    [SerializeField] private SettingsOption showPingOption;
    private bool _isPromptOpen;

    public event Action<int> OnMove;

    private void Awake() {
        videoBasicButton.OnClick += (eventData) => {
            videoAdvancedContent.SetActive(false);
            videoBasicContent.SetActive(true);
        };

        videoAdvancedButton.OnClick += (eventData) => {
            videoBasicContent.SetActive(false);
            videoAdvancedContent.SetActive(true);
        };

        KeybindLegend.Instance.OnApplyButtonClicked += () => {
            SettingsManager.Instance.ApplyChanges();
        };

        SettingsManager.Instance.OnSettingsChanged += (Settings newSettings, bool isDifferent) => {
            if (isDifferent) {
                KeybindLegend.Instance.ShowApplyButton();

                Action<Action> onBeforeTabChange = (confirm) => {
                    _isPromptOpen = true;
                    PromptPanel promptPanel =
                        PromptManager.Instance.CreatePrompt(null, "You have unsaved changes. Apply them?", "Cancel", "Apply", "Revert",
                        null,
                        () => {
                            SettingsManager.Instance.ApplyChanges();
                            confirm();
                            SetBeforeTabChangeEvent(null);
                            _isPromptOpen = false;
                        },
                        () => {
                            SettingsManager.Instance.RevertChanges();
                            confirm();
                            SetBeforeTabChangeEvent(null);
                        }, selectableOnPromptClose);
                };

                SetBeforeTabChangeEvent(onBeforeTabChange);

            } else {
                KeybindLegend.Instance.HideApplyButton();
                SetBeforeTabChangeEvent(null);
                _isPromptOpen = false;
            }

            SetOptionsFromSettings(newSettings);
        };

        SubscribeToChangeEvents();

        List<Resolution> resolutions = SettingsManager.Instance.SupportedResolutions;
        List<Option> options = new List<Option>();
        for (int i = 0; i < resolutions.Count; i++) options.Add(new Option($"{resolutions[i].width}x{resolutions[i].height}", i));

        resolutionOption.SetOptions(options.ToArray());
        renderResolutionOption.SetOptions(options.ToArray());
    }

    private void SubscribeToChangeEvents() {
        presetOption.OnChanged += (option) => SettingsManager.Instance.SetPreset(option.Value);

        resolutionOption.OnChanged += (option) => {
            Resolution resolution = SettingsManager.Instance.SupportedResolutions[option.Value];
            SettingsManager.Instance.SetResolution(resolution.width, resolution.height);
        };

        renderResolutionOption.OnChanged += (option) => {
            Resolution resolution = SettingsManager.Instance.SupportedResolutions[option.Value];
            SettingsManager.Instance.SetRenderResolution(resolution.width, resolution.height);
        };

        windowedOption.OnChanged += (option) => {
            SettingsManager.Instance.SetWindowedMode(GetFullScreenModeFromValue(option.Value));
        };

        showFpsOption.OnChanged += (option) => {
            SettingsManager.Instance.SetShowFps(option.Value == 1);
        };

        showPingOption.OnChanged += (option) => {
            SettingsManager.Instance.SetShowPing(option.Value == 1);
        };
    }

    private void SetBeforeTabChangeEvent(Action<Action> onBeforeTabChange) {
        UIManager.Instance.SetBeforeTabChange(TabGroupIndex.SettingsPanel, onBeforeTabChange);
        UIManager.Instance.SetBeforeTabChange(TabGroupIndex.InGameSettingsPanel, onBeforeTabChange);
        UIManager.Instance.SetBeforeTabChange(TabGroupIndex.Menu, onBeforeTabChange);
    }

    private void SetOptionsFromSettings(Settings settings) {
        resolutionOption.SelectOptionByValue(GetResolutionIndex(settings.ScreenResolution));
        renderResolutionOption.SelectOptionByValue(GetResolutionIndex(settings.RenderResolution));
        windowedOption.SelectOptionByValue(GetValueFromFullscreenMode(settings.FullScreenMode));
        showFpsOption.SelectOptionByValue(settings.Stats.ShowFps == true ? 1 : 0);
        showPingOption.SelectOptionByValue(settings.Stats.ShowPing == true ? 1 : 0);
    }

    private int GetValueFromFullscreenMode(FullScreenMode mode) {
        return mode == FullScreenMode.ExclusiveFullScreen ? 0 :
            mode == FullScreenMode.FullScreenWindow ? 1 :
            mode == FullScreenMode.MaximizedWindow ? 2 : 2;
    }

    private FullScreenMode GetFullScreenModeFromValue(int value) {
        return value == 0 ? FullScreenMode.ExclusiveFullScreen :
            value == 1 ? FullScreenMode.FullScreenWindow :
            value == 2 ? FullScreenMode.MaximizedWindow : FullScreenMode.MaximizedWindow;
    }

    private int GetResolutionIndex(Vector2 resolution) {
        return SettingsManager.Instance.SupportedResolutions.FindIndex(x =>
            x.width == resolution.x &&
            x.height == resolution.y);
    }

    private void OnEnable() {
        SetOptionsFromSettings(SettingsManager.Instance.LastSettings);
        SetInputState(true);
    }

    private void OnDisable() {
        videoAdvancedContent.SetActive(false);
        videoBasicContent.SetActive(true);
        SettingsManager.Instance.RevertChanges();
        KeybindLegend.Instance.HideApplyButton();
        SetInputState(false);
    }

    private void SetInputState(bool enabled) {
        void OnUIMove(Vector2 direction) {
            OnMove?.Invoke((int)direction.x);
        }

        void OnUIButtonEvent(UIButtonType type, bool performed) {
            if (!performed || _isPromptOpen) return;
            if (type == UIButtonType.Apply) SettingsManager.Instance.ApplyChanges();
        }

        if (enabled) {
            UIManager.Instance.InputReader.UIMoveEvent += OnUIMove;
            UIManager.Instance.InputReader.UIButtonEvent += OnUIButtonEvent;
        } else {
            UIManager.Instance.InputReader.UIMoveEvent -= OnUIMove;
            UIManager.Instance.InputReader.UIButtonEvent -= OnUIButtonEvent;
        }
    }
}

[System.Serializable]
public struct Option {
    public string Text;
    public int Value;

    public Option(string text, int value) {
        Text = text;
        Value = value;
    }
}