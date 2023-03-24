using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private GameObject videoBasicContent;
    [SerializeField] private GameObject videoAdvancedContent;
    [SerializeField] private CustomButton videoBasicButton;
    [SerializeField] private CustomButton videoAdvancedButton;
    [SerializeField] private Selectable selectableOnPromptClose;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private int extraNavigationOffset = 0;

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
    [SerializeField] private SettingsOption bloomOption;
    [SerializeField] private SettingsOption antiAliasingOption;
    [SerializeField] private SettingsOption dlssOption;
    [SerializeField] private SettingsOption shadowsOption;
    [SerializeField] private SettingsOption volumetricFogOption;
    [SerializeField] private SettingsOption volumetricCloudOption;
    [SerializeField] private SettingsOption globalIlluminationOption;
    [SerializeField] private SettingsOption ambientOcclusionOption;
    [SerializeField] private SettingsOption reflectionOption;
    [SerializeField] private SettingsOption raytracingGlobalIlluminationOption;
    [SerializeField] private SettingsOption raytracingAmbientOcclusionOption;
    [SerializeField] private SettingsOption raytracingReflectionOption;
    [SerializeField] private SettingsOption raytracingOption;
    [SerializeField] private SettingsOption raytracingShadowsOption;
    [SerializeField] private GameObject raytracingSettings;

    private readonly List<RectTransform> _options = new();

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

        _options.Add(videoBasicButton.GetComponent<RectTransform>());
        _options.Add(videoAdvancedButton.GetComponent<RectTransform>());

        SubscribeToChangeEvents();

        List<Resolution> resolutions = SettingsManager.Instance.SupportedResolutions;
        List<Option> options = new();
        for (int i = 0; i < resolutions.Count; i++) options.Add(new Option($"{resolutions[i].width}x{resolutions[i].height}", i));

        resolutionOption.SetOptions(options.ToArray());
        renderResolutionOption.SetOptions(options.ToArray());

        raytracingSettings.SetActive(raytracingOption.Options[raytracingOption.CurrentIndex].Value == 1);
    }

    private void Update() {
        foreach (RectTransform rect in _options) {
            if (EventSystem.current.currentSelectedGameObject != rect.gameObject) continue;

            float offset = ((rect.rect.height * rect.localScale.y)) + extraNavigationOffset;

            float topPoint = scrollRect.viewport.position.y + (scrollRect.viewport.rect.height);
            float bottomPoint = scrollRect.viewport.position.y - (scrollRect.viewport.rect.height);
            if (rect.position.y > topPoint)
                scrollRect.content.anchoredPosition += new Vector2(0, topPoint - (rect.position.y + offset)) / 2;
            else if (rect.position.y < bottomPoint)
                scrollRect.content.anchoredPosition -= new Vector2(0, (rect.position.y - offset) - bottomPoint) / 2;
        }
    }

    public void AddOption(SettingsOption option) {
        _options.Add(option.GetComponent<RectTransform>());
    }

    public void ShowSettingDescription(SettingsOption setting) {
        descriptionText.text = setting.Description;
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

        windowedOption.OnChanged += (option) => SettingsManager.Instance.SetWindowedMode(GetFullScreenModeFromValue(option.Value));
        maxFpsOption.OnChanged += (option) => SettingsManager.Instance.SetMaxFps(option.Value);
        vsyncOption.OnChanged += (option) => SettingsManager.Instance.SetVsync(option.Value == 1);
        showFpsOption.OnChanged += (option) => SettingsManager.Instance.SetShowFps(option.Value == 1);
        showPingOption.OnChanged += (option) => SettingsManager.Instance.SetShowPing(option.Value == 1);
        bloomOption.OnChanged += (option) => SettingsManager.Instance.SetBloomQuality(option.Value);
        antiAliasingOption.OnChanged += (option) => SettingsManager.Instance.SetAntiAliasingQuality(option.Value);
        dlssOption.OnChanged += (option) => SettingsManager.Instance.SetDLSSQuality(option.Value);
        volumetricFogOption.OnChanged += (option) => SettingsManager.Instance.SetVolumetricFogQuality(option.Value);
        volumetricCloudOption.OnChanged += (option) => SettingsManager.Instance.SetVolumetricCloudQuality(option.Value);
        raytracingOption.OnChanged += (option) => SettingsManager.Instance.SetRaytracingState(option.Value);

        shadowsOption.OnChanged += (option) => SettingsManager.Instance.
           SetShadowsQuality(option.Value, SettingsManager.Instance.CurrentSettings.RaytracingSettings.Shadows);
        globalIlluminationOption.OnChanged += (option) => SettingsManager.Instance.
            SetGlobalIlluminationQuality(option.Value, SettingsManager.Instance.CurrentSettings.RaytracingSettings.GlobalIllumination);
        ambientOcclusionOption.OnChanged += (option) => SettingsManager.Instance.
            SetAmbientOcclusionQuality(option.Value, SettingsManager.Instance.CurrentSettings.RaytracingSettings.AmbientOcclusion);
        reflectionOption.OnChanged += (option) => SettingsManager.Instance.
            SetReflectionQuality(option.Value, SettingsManager.Instance.CurrentSettings.RaytracingSettings.Reflection);

        raytracingShadowsOption.OnChanged += (option) => SettingsManager.Instance.
           SetShadowsQuality(SettingsManager.Instance.CurrentSettings.ShadowsQuality, option.Value == 1);
        raytracingGlobalIlluminationOption.OnChanged += (option) => SettingsManager.Instance.
            SetGlobalIlluminationQuality(SettingsManager.Instance.CurrentSettings.GlobalIlluminationQuality, option.Value == 1);
        raytracingAmbientOcclusionOption.OnChanged += (option) => SettingsManager.Instance.
            SetAmbientOcclusionQuality(SettingsManager.Instance.CurrentSettings.AmbientOcclusionQuality, option.Value == 1);
        raytracingReflectionOption.OnChanged += (option) => SettingsManager.Instance.
            SetReflectionQuality(SettingsManager.Instance.CurrentSettings.ReflectionQuality, option.Value == 1);
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
        presetOption.SelectOptionByValue(settings.Preset);
        resolutionOption.SelectOptionByValue(GetResolutionIndex(settings.ScreenResolution));
        renderResolutionOption.SelectOptionByValue(GetResolutionIndex(settings.RenderResolution));
        windowedOption.SelectOptionByValue(GetValueFromFullscreenMode(settings.FullScreenMode));
        maxFpsOption.SelectOptionByValue(settings.MaxFps);
        vsyncOption.SelectOptionByValue(settings.Vsync ? 1 : 0);
        showFpsOption.SelectOptionByValue(settings.Stats.ShowFps == true ? 1 : 0);
        showPingOption.SelectOptionByValue(settings.Stats.ShowPing == true ? 1 : 0);
        bloomOption.SelectOptionByValue(settings.BloomQuality);
        antiAliasingOption.SelectOptionByValue(settings.AntiAliasingQuality);
        dlssOption.SelectOptionByValue(settings.DLSSQuality);
        shadowsOption.SelectOptionByValue(settings.ShadowsQuality);
        volumetricFogOption.SelectOptionByValue(settings.VolumetricFogQuality);
        volumetricCloudOption.SelectOptionByValue(settings.VolumetricCloudQuality);
        globalIlluminationOption.SelectOptionByValue(settings.GlobalIlluminationQuality);
        ambientOcclusionOption.SelectOptionByValue(settings.AmbientOcclusionQuality);
        raytracingOption.SelectOptionByValue(settings.RaytracingSettings.State);
        reflectionOption.SelectOptionByValue(settings.ReflectionQuality);
        raytracingGlobalIlluminationOption.SelectOptionByValue(settings.RaytracingSettings.GlobalIllumination == true ? 1 : 0);
        raytracingAmbientOcclusionOption.SelectOptionByValue(settings.RaytracingSettings.AmbientOcclusion == true ? 1 : 0);
        raytracingReflectionOption.SelectOptionByValue(settings.RaytracingSettings.Reflection == true ? 1 : 0);
        raytracingShadowsOption.SelectOptionByValue(settings.RaytracingSettings.Shadows == true ? 1 : 0);
        raytracingSettings.SetActive(settings.RaytracingSettings.State == 1);
    }

    private int GetValueFromFullscreenMode(FullScreenMode mode) {
        return mode == FullScreenMode.ExclusiveFullScreen ? 0 :
            mode == FullScreenMode.FullScreenWindow ? 1 :
            mode == FullScreenMode.Windowed ? 2 : 2;
    }

    private FullScreenMode GetFullScreenModeFromValue(int value) {
        return value == 0 ? FullScreenMode.ExclusiveFullScreen :
            value == 1 ? FullScreenMode.FullScreenWindow :
            value == 2 ? FullScreenMode.Windowed : FullScreenMode.Windowed;
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