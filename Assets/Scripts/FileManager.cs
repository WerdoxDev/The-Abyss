using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileManager {
    public static bool WriteToFile(string fileName, string fileContent) {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);

        try {
            File.WriteAllText(fullPath, fileContent);
            return true;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to write to {fullPath} with exception {e}");
            return false;
        }
    }

    public static bool LoadFromFile(string fileName, out string result) {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);

        try {
            result = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to read from {fullPath} with exception {e}");
            result = "";
            return false;
        }
    }
}

public static class SaveManager {
    public static bool ProfileLoaded;

    public static void SaveSettings(Settings settings) {
        if (FileManager.WriteToFile("Settings.dat", JsonUtility.ToJson(settings))) {
            Debug.Log("Settings save successful");
        }
    }

    public static bool LoadSettings() {
        if (FileManager.LoadFromFile("Settings.dat", out string json)) {
            Settings settings = JsonUtility.FromJson<Settings>(json);
            SettingsManager.Instance.SetSettings(settings);
            return true;
        }
        return false;
    }

    public static void SaveProfile(ProfileData profile) {
        if (FileManager.WriteToFile("Profile.dat", JsonUtility.ToJson(profile))) {
            Debug.Log("Profile save successful");
        }
    }

    public static bool LoadProfile() {
        if (FileManager.LoadFromFile("Profile.dat", out string json)) {
            Debug.Log("Load profile");
            ProfileData profile = JsonUtility.FromJson<ProfileData>(json);
            UIManager.Instance.CustomizePanel.SetFromFile(profile);
            UIManager.Instance.ChangeNamePanel.SetFromData(profile);
            ProfileLoaded = true;
            return true;
        }
        ProfileLoaded = false;
        return false;
    }
}

[Serializable]
public struct ProfileData {
    public string PlayerName;
    public Color BodyColor;
    public Color HeadColor;
    public Color LeftEyeColor;
    public Color RightEyeColor;
    public bool IsBodyRgb;
    public bool IsHeadRgb;
    public bool IsLeftEyeRgb;
    public bool IsRightEyeRgb;
}