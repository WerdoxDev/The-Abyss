using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class SettingsPanel : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private GameObject videoBasicContent;
    [SerializeField] private GameObject videoAdvancedContent;
    [SerializeField] private CustomButton videoBasicButton;
    [SerializeField] private CustomButton videoAdvancedButton;
    [SerializeField] private Selectable selectableOnPromptClose;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Basic Settings Options")]
    [SerializeField] private SettingsOption presetOption;
    [SerializeField] private SettingsOption resolutionOption;
    [SerializeField] private SettingsOption renderResolutionOption;
    [SerializeField] private SettingsOption windowedOption;
    [SerializeField] private SettingsOption maxFpsOption;
    [SerializeField] private SettingsOption vsyncOption;
    [SerializeField] private SettingsOption showFpsOption;
    [SerializeField] private SettingsOption showPingOption;

    [Header("Advanced Settings Options")]
    [SerializeField] private SettingsOption raytracing;
    [SerializeField] private GameObject raytracingSettings;

    private bool _isPromptOpen;

    public event Action<int> OnMove;

    private void OnEnable() {
        SetOptionsFromSettings(SettingsManager.Instance.LastSettings);
        SetInputState(true);
        SetExternalEventsState(true);
        SetPreventionState(true);
    }

    private void OnDisable() {
        videoAdvancedContent.SetActive(false);
        videoBasicContent.SetActive(true);
        SettingsManager.Instance.RevertChanges();
        KeybindLegend.Instance.HideApplyButton();

        SetInputState(false);
        SetExternalEventsState(false);
        SetPreventionState(false);
    }

    private void Awake() {
        videoBasicButton.OnClick += () => {
            videoAdvancedContent.SetActive(false);
            videoBasicContent.SetActive(true);
        };

        videoAdvancedButton.OnClick += () => {
            videoBasicContent.SetActive(false);
            videoAdvancedContent.SetActive(true);
        };

        KeybindLegend.Instance.OnApplyButtonClicked += () => {
            SettingsManager.Instance.ApplyChanges();
        };

        SubscribeToChangeEvents();

        List<Resolution> resolutions = SettingsManager.Instance.SupportedResolutions;
        List<Option> options = new List<Option>();
        for (int i = 0; i < resolutions.Count; i++) options.Add(new Option($"{resolutions[i].width}x{resolutions[i].height}", i));

        resolutionOption.SetOptions(options.ToArray());
        renderResolutionOption.SetOptions(options.ToArray());

        raytracingSettings.SetActive(raytracing.options[raytracing.CurrentIndex].Value == 0);
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

        maxFpsOption.OnChanged += (option) => {
            SettingsManager.Instance.SetMaxFps(option.Value);
        };

        vsyncOption.OnChanged += (option) => {
            SettingsManager.Instance.SetVsync(option.Value == 1);
        };

        showFpsOption.OnChanged += (option) => {
            SettingsManager.Instance.SetShowFps(option.Value == 1);
        };

        showPingOption.OnChanged += (option) => {
            SettingsManager.Instance.SetShowPing(option.Value == 1);
        };

        raytracing.OnChanged += (option) => {
            SettingsManager.Instance.SetRaytracing(option.Value == 1);
            raytracingSettings.SetActive(option.Value == 1);
        };
    }

    private void ShowWarningPrompt() {
        PromptManager.Instance.CreatePrompt(null, "You have unsaved changes. Apply them?", "Cancel", "Apply", "Revert",
            () => EventSystem.current.SetSelectedGameObject(selectableOnPromptClose.gameObject),
            () => {
                SettingsManager.Instance.ApplyChanges();
                _isPromptOpen = false;
                EventSystem.current.SetSelectedGameObject(selectableOnPromptClose.gameObject);
            },
            () => {
                SettingsManager.Instance.RevertChanges();
                EventSystem.current.SetSelectedGameObject(selectableOnPromptClose.gameObject);
            });
    }

    private void SetOptionsFromSettings(Settings settings) {
        resolutionOption.SelectOptionByValue(GetResolutionIndex(settings.ScreenResolution));
        renderResolutionOption.SelectOptionByValue(GetResolutionIndex(settings.RenderResolution));
        windowedOption.SelectOptionByValue(GetValueFromFullscreenMode(settings.FullScreenMode));
        maxFpsOption.SelectOptionByValue(settings.MaxFps);
        vsyncOption.SelectOptionByValue(settings.Vsync ? 1 : 0);
        showFpsOption.SelectOptionByValue(settings.Stats.ShowFps == true ? 1 : 0);
        showPingOption.SelectOptionByValue(settings.Stats.ShowPing == true ? 1 : 0);
        raytracing.SelectOptionByValue(settings.Raytracing == true ? 1 : 0);
        if (settings.Raytracing == true) scrollRect.verticalNormalizedPosition = 1;
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

    private void SetExternalEventsState(bool enabled) {
        void SettingsChanged(Settings newSettings, bool isDirty) {
            if (isDirty) KeybindLegend.Instance.ShowApplyButton();
            else {
                KeybindLegend.Instance.HideApplyButton();
                _isPromptOpen = false;
            }

            SetOptionsFromSettings(newSettings);
        }

        if (enabled) SettingsManager.Instance.OnSettingsChanged += SettingsChanged;
        else SettingsManager.Instance.OnSettingsChanged -= SettingsChanged;
    }

    private void SetPreventionState(bool enabled) {
        void TabAttempt(TabGroupIndex groupIndex, Tab tab, Action cancel) {
            if (groupIndex != TabGroupIndex.SettingsPanel &&
                groupIndex != TabGroupIndex.InGameSettingsPanel &&
                groupIndex != TabGroupIndex.Menu) return;

            if (!SettingsManager.Instance.IsDirty()) return;
            cancel?.Invoke();

            ShowWarningPrompt();
        }

        void PanelAttempt(Panel panel, bool isOpen, Action cancel) {
            if (panel.Type != PanelType.InGameSettings && panel.Type != PanelType.InGameMenu) return;

            if (!SettingsManager.Instance.IsDirty()) return;
            cancel?.Invoke();
            Debug.Log(panel.Type);

            if (panel.Type == PanelType.InGameSettings) ShowWarningPrompt();
        }


        if (enabled) {
            UIManager.Instance.OnChangeTabAttempt += TabAttempt;
            UIManager.Instance.OnPanelChangeStateAttempt += PanelAttempt;
        }
        else {
            UIManager.Instance.OnChangeTabAttempt -= TabAttempt;
            UIManager.Instance.OnPanelChangeStateAttempt -= PanelAttempt;
        }
    }

    private void SetInputState(bool enabled) {
        void OnUIMove(Vector2 direction) {
            OnMove?.Invoke((int)direction.x);
        }

        void OnUIButtonEvent(UIButtonType type, bool performed) {
            if (!performed || _isPromptOpen || !KeybindLegend.Instance.IsApplyVisible) return;
            if (type == UIButtonType.Apply) SettingsManager.Instance.ApplyChanges();
        }

        if (enabled) {
            UIManager.Instance.InputReader.UIMoveEvent += OnUIMove;
            UIManager.Instance.InputReader.UIButtonEvent += OnUIButtonEvent;
        }
        else {
            UIManager.Instance.InputReader.UIMoveEvent -= OnUIMove;
            UIManager.Instance.InputReader.UIButtonEvent -= OnUIButtonEvent;
        }
    }
}

[Serializable]
public struct Option {
    public string Text;
    public int Value;

    public Option(string text, int value) {
        Text = text;
        Value = value;
    }
}