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
        CurrentSettings.Stats.ShowFps = false;
        CurrentSettings.Stats.ShowPing = false;
        ApplyChanges();
    }
}

[Serializable]
public struct Settings : IEquatable<Settings> {
    public Vector2Int ScreenResolution;
    public Vector2Int RenderResolution;
    public FullScreenMode FullScreenMode;
    public StatSettings Stats;

    public bool Equals(Settings other) {
        return other.ScreenResolution == ScreenResolution && other.RenderResolution == RenderResolution && other.FullScreenMode == FullScreenMode;
    }
}

public struct StatSettings {
    public bool ShowFps;
    public bool ShowPing;
}