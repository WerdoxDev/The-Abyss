using System;
using Unity.Netcode;
using UnityEngine;

public class ProfileCustomizePanel : MonoBehaviour {

    [Header("Color Settings")]
    [SerializeField] private CustomizationColor bodyColor;
    [SerializeField] private CustomizationColor headColor;
    [SerializeField] private CustomizationColor leftEyeColor;
    [SerializeField] private CustomizationColor rightEyeColor;

    [Header("Settings")]
    public RectTransform RawImageTransform;

    public PlayerCustomizationInfo PlayerCustomization;

    private void Awake() {
        if (!SaveManager.ProfileLoaded) PlayerCustomization = PlayerCustomizationInfo.Default;

        bodyColor.OnSetRgb += (isOn) => SetRgb(CustomizationType.Body, isOn);
        bodyColor.OnChooseColor += (color) => ChooseColor(CustomizationType.Body, color);

        headColor.OnSetRgb += (isOn) => SetRgb(CustomizationType.Head, isOn);
        headColor.OnChooseColor += (color) => ChooseColor(CustomizationType.Head, color);

        leftEyeColor.OnSetRgb += (isOn) => SetRgb(CustomizationType.LeftEye, isOn);
        leftEyeColor.OnChooseColor += (color) => ChooseColor(CustomizationType.LeftEye, color);

        rightEyeColor.OnSetRgb += (isOn) => SetRgb(CustomizationType.RightEye, isOn);
        rightEyeColor.OnChooseColor += (color) => ChooseColor(CustomizationType.RightEye, color);

        if (SaveManager.ProfileLoaded) return;
        ChooseColor(CustomizationType.Head, Color.white);
        ChooseColor(CustomizationType.Body, Color.white);
        ChooseColor(CustomizationType.LeftEye, Color.white);
        ChooseColor(CustomizationType.RightEye, Color.white);
    }

    public void SetFromFile(ProfileData profile) {
        PlayerCustomization.BodyColor = profile.BodyColor;
        PlayerCustomization.HeadColor = profile.HeadColor;
        PlayerCustomization.LeftEyeColor = profile.LeftEyeColor;
        PlayerCustomization.RightEyeColor = profile.RightEyeColor;

        PlayerCustomization.IsBodyRgb = profile.IsBodyRgb;
        PlayerCustomization.IsHeadRgb = profile.IsHeadRgb;
        PlayerCustomization.IsLeftEyeRgb = profile.IsLeftEyeRgb;
        PlayerCustomization.IsRightEyeRgb = profile.IsRightEyeRgb;
        bodyColor.SetRgb(profile.IsBodyRgb);
        headColor.SetRgb(profile.IsHeadRgb);
        leftEyeColor.SetRgb(profile.IsLeftEyeRgb);
        rightEyeColor.SetRgb(profile.IsRightEyeRgb);
    }

    public void SaveToFile() {
        SaveManager.SaveProfile(new() {
            PlayerName = UIManager.Instance.PlayerName,
            BodyColor = PlayerCustomization.BodyColor,
            HeadColor = PlayerCustomization.HeadColor,
            LeftEyeColor = PlayerCustomization.LeftEyeColor,
            RightEyeColor = PlayerCustomization.RightEyeColor,
            IsBodyRgb = PlayerCustomization.IsBodyRgb,
            IsHeadRgb = PlayerCustomization.IsHeadRgb,
            IsLeftEyeRgb = PlayerCustomization.IsLeftEyeRgb,
            IsRightEyeRgb = PlayerCustomization.IsRightEyeRgb,
        });
    }

    private void ChooseColor(CustomizationType type, Color color) {
        if (type == CustomizationType.Body) PlayerCustomization.BodyColor = color;
        else if (type == CustomizationType.Head) PlayerCustomization.HeadColor = color;
        else if (type == CustomizationType.LeftEye) PlayerCustomization.LeftEyeColor = color;
        else if (type == CustomizationType.RightEye) PlayerCustomization.RightEyeColor = color;

        UIManager.Instance.PlayerCustomization.UpdatePlayer(UIManager.Instance.PlayerName, PlayerCustomization);

        SaveToFile();
    }

    private void SetRgb(CustomizationType type, bool isOn) {
        if (type == CustomizationType.Body) PlayerCustomization.IsBodyRgb = isOn;
        else if (type == CustomizationType.Head) PlayerCustomization.IsHeadRgb = isOn;
        else if (type == CustomizationType.LeftEye) PlayerCustomization.IsLeftEyeRgb = isOn;
        else if (type == CustomizationType.RightEye) PlayerCustomization.IsRightEyeRgb = isOn;

        UIManager.Instance.PlayerCustomization.UpdatePlayer(UIManager.Instance.PlayerName, PlayerCustomization);

        SaveToFile();
    }
}

[System.Serializable]
public struct PlayerCustomizationInfo : IEquatable<PlayerCustomizationInfo>, INetworkSerializable {
    public static PlayerCustomizationInfo Default = new() {
        BodyColor = Color.white,
        HeadColor = Color.white,
        LeftEyeColor = Color.white,
        RightEyeColor = Color.white
    };

    public Color BodyColor;
    public Color HeadColor;
    public Color LeftEyeColor;
    public Color RightEyeColor;
    public bool IsBodyRgb;
    public bool IsHeadRgb;
    public bool IsLeftEyeRgb;
    public bool IsRightEyeRgb;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref BodyColor);
        serializer.SerializeValue(ref HeadColor);
        serializer.SerializeValue(ref LeftEyeColor);
        serializer.SerializeValue(ref RightEyeColor);
        serializer.SerializeValue(ref IsBodyRgb);
        serializer.SerializeValue(ref IsHeadRgb);
        serializer.SerializeValue(ref IsLeftEyeRgb);
        serializer.SerializeValue(ref IsRightEyeRgb);
    }

    public bool Equals(PlayerCustomizationInfo other) =>
        other.BodyColor == BodyColor && other.HeadColor == HeadColor &&
        other.LeftEyeColor == LeftEyeColor && other.RightEyeColor == RightEyeColor &&
        other.IsBodyRgb == IsBodyRgb && other.IsHeadRgb == IsHeadRgb &&
        other.IsLeftEyeRgb == IsLeftEyeRgb && other.IsRightEyeRgb == IsRightEyeRgb;
}

public enum CustomizationType {
    Body, Head, LeftEye, RightEye
}
