using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance;

    [Header("Settings")]
    [SerializeField] private RenderTexture mainRenderTexture;

    public Camera CurrentCamera;

    public List<Resolution> SupportedResolutions;

    public Settings CurrentSettings;
    public Settings LastSettings;

    public event Action<Settings, bool> OnSettingsChanged;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }

        SupportedResolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToList();

        ApplyDefaultSettings();
    }

    public bool IsDirty() => !CurrentSettings.Equals(LastSettings);

    public void SetPreset(int value) {
        Debug.Log(value);
    }

    public void SetResolution(int width, int height) {
        CurrentSettings.ScreenResolution = new Vector2Int(width, height);
        SettingsChanged();
    }

    public void SetRenderResolution(int width, int height) {
        CurrentSettings.RenderResolution = new Vector2Int(width, height);
        SettingsChanged();
    }

    public void SetWindowedMode(FullScreenMode mode) {
        CurrentSettings.FullScreenMode = mode;
        SettingsChanged();
    }

    public void SetMaxFps(int maxFps) {
        CurrentSettings.MaxFps = maxFps;
        SettingsChanged();
    }

    public void SetVsync(bool vsync) {
        CurrentSettings.Vsync = vsync;
        SettingsChanged();
    }

    public void SetShowFps(bool show) {
        CurrentSettings.Stats.ShowFps = show;
        SettingsChanged();
    }

    public void SetShowPing(bool show) {
        CurrentSettings.Stats.ShowPing = show;
        SettingsChanged();
    }

    public void SetStatSettings(StatSettings stats) {
        CurrentSettings.Stats = stats;
        SettingsChanged();
    }

    public void ApplyChanges() {
        Screen.SetResolution(CurrentSettings.ScreenResolution.x, CurrentSettings.ScreenResolution.y, CurrentSettings.FullScreenMode);

        if (CurrentCamera?.targetTexture != null) CurrentCamera.targetTexture.Release();

        if (CurrentCamera != null) {
            RenderTexture renderTexture = new RenderTexture(CurrentSettings.RenderResolution.x, CurrentSettings.RenderResolution.y, 24); ;

            CurrentCamera.targetTexture = renderTexture;
            UIManager.Instance?.SetRenderTexure(renderTexture);
        }


        if (CurrentSettings.Vsync) CurrentSettings.MaxFps = -1;
        QualitySettings.vSyncCount = CurrentSettings.Vsync ? 1 : 0;
        // Max fps option has a off that feeds -1 which is basically no fps limit
        Application.targetFrameRate = CurrentSettings.MaxFps;

        UIManager.Instance.StatsPanel.SetFpsVisibility(CurrentSettings.Stats.ShowFps);
        UIManager.Instance.StatsPanel.SetPingVisibility(CurrentSettings.Stats.ShowPing);

        LastSettings = CurrentSettings;
        SettingsChanged();
    }

    public void RevertChanges() {
        CurrentSettings = LastSettings;
        SettingsChanged();
    }

    private void SettingsChanged() {
        OnSettingsChanged?.Invoke(CurrentSettings, !CurrentSettings.Equals(LastSettings));
    }

    private void ApplyDefaultSettings() {
        Resolution highestResolution = SupportedResolutions.OrderBy(x => x.width).Last();
        CurrentSettings.ScreenResolution = new Vector2Int(highestResolution.width, highestResolution.height);
        CurrentSettings.RenderResolution = new Vector2Int(highestResolution.width, highestResolution.height);
        CurrentSettings.FullScreenMode = FullScreenMode.Windowed;
        CurrentSettings.Vsync = true;
        CurrentSettings.MaxFps = -1;
        CurrentSettings.Stats = new StatSettings(false, false);
        ApplyChanges();
    }
}

[Serializable]
public struct Settings : IEquatable<Settings> {
    public Vector2Int ScreenResolution;
    public Vector2Int RenderResolution;
    public FullScreenMode FullScreenMode;
    public int MaxFps;
    public bool Vsync;
    public StatSettings Stats;

    public bool Equals(Settings other) =>
        other.ScreenResolution == ScreenResolution &&
        other.RenderResolution == RenderResolution &&
        other.FullScreenMode == FullScreenMode &&
        other.MaxFps == MaxFps &&
        other.Vsync == Vsync &&
        other.Stats.Equals(Stats);
}

public struct StatSettings : IEquatable<StatSettings> {
    public bool ShowFps;
    public bool ShowPing;

    public StatSettings(bool showFps, bool showPing) {
        ShowFps = showFps;
        ShowPing = showPing;
    }

    public bool Equals(StatSettings other) =>
        other.ShowFps == ShowFps && other.ShowPing == ShowPing;
}