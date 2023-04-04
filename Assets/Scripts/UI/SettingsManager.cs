using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance;

    [Header("Settings")]
    [SerializeField] private RenderTexture mainRenderTexture;
    [SerializeField] private VolumeProfile globalVolumeProfile;
    [SerializeField] private VolumeProfile menuVolumeProfile;
    [SerializeField] private VolumeProfile gameVolumeProfile;
    [SerializeField] private VolumeProfile menuSkyProfile;
    [SerializeField] private VolumeProfile gameSkyProfile;
    [SerializeField] private int defaultPreset = 1;

    public WaterGenerator CurrentWaterGenerator;
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
    }

    public bool IsDirty() => !CurrentSettings.Equals(LastSettings);

    public void SetPreset(int preset) {
        CurrentSettings.Preset = preset;

        if (preset == 0) {
            CurrentSettings.BloomQuality = 0;
            CurrentSettings.AntiAliasingQuality = 0;
            CurrentSettings.ShadowsQuality = 0;
            CurrentSettings.VolumetricFogQuality = 0;
            CurrentSettings.VolumetricCloudQuality = 0;
            CurrentSettings.GlobalIlluminationQuality = 0;
            CurrentSettings.AmbientOcclusionQuality = 0;
            CurrentSettings.ReflectionQuality = 0;
        }
        else if (preset == 1) {
            CurrentSettings.BloomQuality = 2;
            CurrentSettings.AntiAliasingQuality = 2;
            CurrentSettings.ShadowsQuality = 2;
            CurrentSettings.VolumetricFogQuality = 1;
            CurrentSettings.VolumetricCloudQuality = 2;
            CurrentSettings.GlobalIlluminationQuality = 1;
            CurrentSettings.AmbientOcclusionQuality = 1;
            CurrentSettings.ReflectionQuality = 2;
        }
        else if (preset == 2) {
            CurrentSettings.BloomQuality = 3;
            CurrentSettings.AntiAliasingQuality = 3;
            CurrentSettings.ShadowsQuality = 3;
            CurrentSettings.VolumetricFogQuality = 2;
            CurrentSettings.VolumetricCloudQuality = 3;
            CurrentSettings.GlobalIlluminationQuality = 2;
            CurrentSettings.AmbientOcclusionQuality = 2;
            CurrentSettings.ReflectionQuality = 3;
        }
        else if (preset == 3) {
            CurrentSettings.BloomQuality = 3;
            CurrentSettings.AntiAliasingQuality = 3;
            CurrentSettings.ShadowsQuality = 3;
            CurrentSettings.VolumetricFogQuality = 3;
            CurrentSettings.VolumetricCloudQuality = 3;
            CurrentSettings.GlobalIlluminationQuality = 3;
            CurrentSettings.AmbientOcclusionQuality = 3;
            CurrentSettings.ReflectionQuality = 3;
        }

        SettingsChanged();
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

    public void SetBloomQuality(int bloomQuality) {
        CurrentSettings.BloomQuality = bloomQuality;
        SettingsChanged();
    }

    public void SetAntiAliasingQuality(int antiAliasingQuality) {
        CurrentSettings.AntiAliasingQuality = antiAliasingQuality;
        SettingsChanged();
    }

    public void SetDLSSQuality(int dlssQuality) {
        CurrentSettings.DLSSQuality = dlssQuality;
        if (dlssQuality != 0) CurrentSettings.AntiAliasingQuality = -1;
        else CurrentSettings.AntiAliasingQuality = 0;
        SettingsChanged();
    }

    public void SetVolumetricFogQuality(int volumetricFogQuality) {
        CurrentSettings.VolumetricFogQuality = volumetricFogQuality;
        SettingsChanged();
    }

    public void SetVolumetricCloudQuality(int volumetricCloudQuality) {
        CurrentSettings.VolumetricCloudQuality = volumetricCloudQuality;
        SettingsChanged();
    }

    public void SetRaytracingState(int state) {
        CurrentSettings.RaytracingSettings.State = state;

        if (state == 0 && CurrentSettings.ShadowsQuality == -1) CurrentSettings.ShadowsQuality = 0;
        else if (state == 1 && CurrentSettings.RaytracingSettings.Shadows) CurrentSettings.ShadowsQuality = -1;
        SettingsChanged();
    }

    public void SetGlobalIlluminationQuality(int globalIlluminationQuality, bool raytracing) {
        CurrentSettings.RaytracingSettings.GlobalIllumination = raytracing;
        CurrentSettings.GlobalIlluminationQuality = globalIlluminationQuality;
        SettingsChanged();
    }

    public void SetAmbientOcclusionQuality(int ambientOcclusionQuality, bool raytracing) {
        CurrentSettings.RaytracingSettings.AmbientOcclusion = raytracing;
        CurrentSettings.AmbientOcclusionQuality = ambientOcclusionQuality;
        SettingsChanged();
    }

    public void SetReflectionQuality(int reflectionQuality, bool raytracing) {
        CurrentSettings.RaytracingSettings.Reflection = raytracing;
        CurrentSettings.ReflectionQuality = reflectionQuality;
        SettingsChanged();
    }

    public void SetShadowsQuality(int shadowsQuality, bool raytracing) {
        CurrentSettings.RaytracingSettings.Shadows = raytracing;

        if (raytracing) CurrentSettings.ShadowsQuality = -1;
        else CurrentSettings.ShadowsQuality = shadowsQuality == -1 ? 0 : shadowsQuality;
        SettingsChanged();
    }

    public void SetRaytracingSettings(RaytracingSettings raytracing) {
        CurrentSettings.RaytracingSettings = raytracing;
        SettingsChanged();
    }

    public void SetSettings(Settings settings) {
        Debug.Log("Loaded");
        CurrentSettings = settings;
        ApplyChanges();
    }

    public void ApplyChanges() {
        if (!SystemInfo.supportsRayTracing)
            CurrentSettings.RaytracingSettings.State = -1;
        else if (CurrentSettings.RaytracingSettings.State == -1) CurrentSettings.RaytracingSettings.State = 0;

        if (!HDDynamicResolutionPlatformCapabilities.DLSSDetected)
            CurrentSettings.DLSSQuality = -1;
        else if (CurrentSettings.DLSSQuality == -1) CurrentSettings.DLSSQuality = 0;

        Screen.SetResolution(CurrentSettings.ScreenResolution.x, CurrentSettings.ScreenResolution.y, CurrentSettings.FullScreenMode);

        if (CurrentCamera?.targetTexture != null) CurrentCamera.targetTexture.Release();

        if (CurrentCamera != null) {
            RenderTexture renderTexture = new(CurrentSettings.RenderResolution.x, CurrentSettings.RenderResolution.y, 24);

            CurrentCamera.targetTexture = renderTexture;
            UIManager.Instance?.SetRenderTexure(renderTexture);
        }

        if (CurrentSettings.Vsync) CurrentSettings.MaxFps = 0;
        QualitySettings.vSyncCount = CurrentSettings.Vsync ? 1 : 0;
        // Max fps option has a off that needs -1 which is basically no fps limit
        Application.targetFrameRate = CurrentSettings.MaxFps;

        UIManager.Instance.StatsPanel.SetFpsVisibility(CurrentSettings.Stats.ShowFps);
        UIManager.Instance.StatsPanel.SetPingVisibility(CurrentSettings.Stats.ShowPing);

        menuVolumeProfile.TryGet<Bloom>(out var menuBloom);
        gameVolumeProfile.TryGet<Bloom>(out var gameBloom);

        menuBloom.active = gameBloom.active = CurrentSettings.BloomQuality != 0;

        if (CurrentSettings.BloomQuality == 1) menuBloom.quality.value = gameBloom.quality.value = 0;
        else if (CurrentSettings.BloomQuality == 2) menuBloom.quality.value = gameBloom.quality.value = 1;
        else if (CurrentSettings.BloomQuality == 3) menuBloom.quality.value = gameBloom.quality.value = 2;

        HDAdditionalCameraData cameraData = CurrentCamera.GetComponent<HDAdditionalCameraData>();
        cameraData.antialiasing = CurrentSettings.AntiAliasingQuality == 0 ?
            HDAdditionalCameraData.AntialiasingMode.None :
            HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;

        if (CurrentSettings.AntiAliasingQuality == 1) cameraData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.Low;
        else if (CurrentSettings.AntiAliasingQuality == 2) cameraData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.Medium;
        else if (CurrentSettings.AntiAliasingQuality == 3) cameraData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.High;

        cameraData.allowDeepLearningSuperSampling = CurrentSettings.DLSSQuality != 0;
        if (CurrentSettings.DLSSQuality == 1) cameraData.deepLearningSuperSamplingQuality = 0;
        else if (CurrentSettings.DLSSQuality == 2) cameraData.deepLearningSuperSamplingQuality = 1;
        else if (CurrentSettings.DLSSQuality == 3) cameraData.deepLearningSuperSamplingQuality = 2;
        else if (CurrentSettings.DLSSQuality == 4) cameraData.deepLearningSuperSamplingQuality = 3;

        foreach (HDAdditionalLightData lightData in FindObjectsByType<HDAdditionalLightData>(FindObjectsSortMode.None)) {
            lightData.useRayTracedShadows = CurrentSettings.RaytracingSettings.State == 1 && CurrentSettings.RaytracingSettings.Shadows;
            lightData.rayTraceContactShadow = CurrentSettings.RaytracingSettings.State == 1 && CurrentSettings.RaytracingSettings.Shadows;
            lightData.numRayTracingSamples = 6;

            lightData.EnableShadows(CurrentSettings.ShadowsQuality != 0 ||
                (CurrentSettings.RaytracingSettings.State == 1 && CurrentSettings.RaytracingSettings.Shadows));

            if (CurrentSettings.RaytracingSettings.State == 1 || !CurrentSettings.RaytracingSettings.Shadows) {
                lightData.useContactShadow.level = CurrentSettings.ShadowsQuality == 0 ? 0 : 2;
                lightData.SetShadowResolutionOverride(true);
                if (CurrentSettings.ShadowsQuality == 1) lightData.SetShadowResolution(256);
                else if (CurrentSettings.ShadowsQuality == 2) lightData.SetShadowResolution(1024);
                else if (CurrentSettings.ShadowsQuality == 3) lightData.SetShadowResolution(2048);
            }
        }

        menuSkyProfile.TryGet<Fog>(out var menuFog);
        gameSkyProfile.TryGet<Fog>(out var gameFog);

        menuFog.active = gameFog.active = CurrentSettings.VolumetricFogQuality != 0;

        if (CurrentSettings.VolumetricFogQuality == 1) menuFog.quality.value = gameFog.quality.value = 0;
        else if (CurrentSettings.VolumetricFogQuality == 2) menuFog.quality.value = gameFog.quality.value = 1;
        else if (CurrentSettings.VolumetricFogQuality == 3) menuFog.quality.value = gameFog.quality.value = 2;

        menuSkyProfile.TryGet<VolumetricClouds>(out var menuClouds);
        gameSkyProfile.TryGet<VolumetricClouds>(out var gameClouds);

        menuClouds.active = gameClouds.active = CurrentSettings.VolumetricCloudQuality != 0;

        if (CurrentSettings.VolumetricCloudQuality == 1) menuClouds.shadowResolution.value = gameClouds.shadowResolution.value =
                VolumetricClouds.CloudShadowResolution.VeryLow64;
        else if (CurrentSettings.VolumetricCloudQuality == 2) menuClouds.shadowResolution.value = gameClouds.shadowResolution.value =
                VolumetricClouds.CloudShadowResolution.Medium256;
        else if (CurrentSettings.VolumetricCloudQuality == 3) menuClouds.shadowResolution.value = gameClouds.shadowResolution.value =
                VolumetricClouds.CloudShadowResolution.Ultra1024;

        menuSkyProfile.TryGet<GlobalIllumination>(out var menuGl);
        gameSkyProfile.TryGet<GlobalIllumination>(out var gameGl);

        menuGl.active = gameGl.active = CurrentSettings.GlobalIlluminationQuality != 0;

        menuGl.tracing.value = gameGl.tracing.value =
            CurrentSettings.RaytracingSettings.GlobalIllumination &&
            CurrentSettings.RaytracingSettings.State == 1 ? RayCastingMode.RayTracing : RayCastingMode.RayMarching;

        menuGl.mode.value = gameGl.mode.value = RayTracingMode.Performance;

        if (CurrentSettings.GlobalIlluminationQuality == 1) menuGl.quality.value = gameGl.quality.value = 0;
        if (CurrentSettings.GlobalIlluminationQuality == 2) menuGl.quality.value = gameGl.quality.value = 1;
        if (CurrentSettings.GlobalIlluminationQuality == 3) menuGl.quality.value = gameGl.quality.value = 2;

        menuSkyProfile.TryGet<ScreenSpaceAmbientOcclusion>(out var menuAmbient);
        gameSkyProfile.TryGet<ScreenSpaceAmbientOcclusion>(out var gameAmbient);

        menuAmbient.active = gameAmbient.active = CurrentSettings.AmbientOcclusionQuality != 0;

        menuAmbient.rayTracing.value = gameAmbient.rayTracing.value =
            CurrentSettings.RaytracingSettings.AmbientOcclusion && CurrentSettings.RaytracingSettings.State == 1;

        if (CurrentSettings.AmbientOcclusionQuality == 1) menuAmbient.quality.value = gameAmbient.quality.value = 0;
        else if (CurrentSettings.AmbientOcclusionQuality == 2) menuAmbient.quality.value = gameAmbient.quality.value = 1;
        else if (CurrentSettings.AmbientOcclusionQuality == 3) menuAmbient.quality.value = gameAmbient.quality.value = 2;

        menuSkyProfile.TryGet<ScreenSpaceReflection>(out var menuReflection);
        gameSkyProfile.TryGet<ScreenSpaceReflection>(out var gameReflection);

        menuReflection.enabled.value = gameReflection.enabled.value = CurrentSettings.ReflectionQuality != 0;
        menuReflection.enabledTransparent.value = gameReflection.enabledTransparent.value = CurrentSettings.ReflectionQuality != 0;

        menuReflection.tracing.value = gameReflection.tracing.value =
            CurrentSettings.RaytracingSettings.Reflection &&
            CurrentSettings.RaytracingSettings.State == 1 ? RayCastingMode.RayTracing : RayCastingMode.RayMarching;

        menuReflection.mode.value = gameReflection.mode.value = RayTracingMode.Performance;

        if (CurrentSettings.ReflectionQuality == 1) menuReflection.quality.value = 0;
        if (CurrentSettings.ReflectionQuality == 2) menuReflection.quality.value = 1;
        if (CurrentSettings.ReflectionQuality == 3) menuReflection.quality.value = 2;

        LastSettings = CurrentSettings;
        SettingsChanged();

        SaveManager.SaveSettings(CurrentSettings);
    }

    public void RevertChanges() {
        CurrentSettings = LastSettings;
        SettingsChanged();
    }

    private void SettingsChanged() {
        OnSettingsChanged?.Invoke(CurrentSettings, !CurrentSettings.Equals(LastSettings));
    }

    public void ApplyDefaultSettings() {
        Resolution highestResolution = SupportedResolutions.OrderBy(x => x.width).Last();
        CurrentSettings.ScreenResolution = new Vector2Int(highestResolution.width, highestResolution.height);
        CurrentSettings.RenderResolution = new Vector2Int(highestResolution.width, highestResolution.height);
        CurrentSettings.FullScreenMode = FullScreenMode.ExclusiveFullScreen;
        CurrentSettings.Vsync = false;
        CurrentSettings.MaxFps = 0;
        CurrentSettings.DLSSQuality = 0;
        SetPreset(defaultPreset);
        CurrentSettings.RaytracingSettings = new(0, false, false, false, false);
        CurrentSettings.Stats = new(false, false);

        ApplyChanges();
    }
}

[Serializable]
public struct Settings : IEquatable<Settings> {
    public int Preset;
    public Vector2Int ScreenResolution;
    public Vector2Int RenderResolution;
    public FullScreenMode FullScreenMode;
    public int MaxFps;
    public bool Vsync;
    public int BloomQuality;
    public int AntiAliasingQuality;
    public int DLSSQuality;
    public int ShadowsQuality;
    public int VolumetricFogQuality;
    public int VolumetricCloudQuality;
    public int AmbientOcclusionQuality;
    public int GlobalIlluminationQuality;
    public int ReflectionQuality;
    public RaytracingSettings RaytracingSettings;
    public StatSettings Stats;

    public bool Equals(Settings other) =>
        other.Preset == Preset &&
        other.ScreenResolution == ScreenResolution &&
        other.RenderResolution == RenderResolution &&
        other.FullScreenMode == FullScreenMode &&
        other.MaxFps == MaxFps &&
        other.Vsync == Vsync &&
        other.BloomQuality == BloomQuality &&
        other.AntiAliasingQuality == AntiAliasingQuality &&
        other.DLSSQuality == DLSSQuality &&
        other.ShadowsQuality == ShadowsQuality &&
        other.VolumetricFogQuality == VolumetricFogQuality &&
        other.VolumetricCloudQuality == VolumetricCloudQuality &&
        other.AmbientOcclusionQuality == AmbientOcclusionQuality &&
        other.GlobalIlluminationQuality == GlobalIlluminationQuality &&
        other.ReflectionQuality == ReflectionQuality &&
        other.RaytracingSettings.Equals(RaytracingSettings) &&
        other.Stats.Equals(Stats);
}

[Serializable]
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

[Serializable]
public struct RaytracingSettings : IEquatable<RaytracingSettings> {
    public int State;
    public bool AmbientOcclusion;
    public bool GlobalIllumination;
    public bool Reflection;
    public bool Shadows;

    public RaytracingSettings(int raytracingState, bool ambientOcclusion, bool globalIllumination, bool reflection, bool shadows) {
        State = raytracingState;
        AmbientOcclusion = ambientOcclusion;
        GlobalIllumination = globalIllumination;
        Reflection = reflection;
        Shadows = shadows;
    }

    public bool Equals(RaytracingSettings other) =>
        other.State == State &&
        other.AmbientOcclusion == AmbientOcclusion &&
        other.GlobalIllumination == GlobalIllumination &&
        other.Reflection == Reflection &&
        other.Shadows == Shadows;
}