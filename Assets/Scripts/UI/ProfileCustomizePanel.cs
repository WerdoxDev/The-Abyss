using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class ProfileCustomizePanel : MonoBehaviour
{
    [SerializeField] private CustomizationColor faceColor;
    [SerializeField] private CustomizationColor bodyColor;

    [SerializeField] private MeshRenderer playerFace;
    [SerializeField] private MeshRenderer playerBody;

    public PlayerCustomization PlayerCustomization;

    private void Awake()
    {
        faceColor.OnSetRgb += (isOn) => SetRgb(CustomizationType.Face, isOn);
        faceColor.OnChooseColor += (color) => ChooseColor(CustomizationType.Face, color);

        bodyColor.OnSetRgb += (isOn) => SetRgb(CustomizationType.Body, isOn);
        bodyColor.OnChooseColor += (color) => ChooseColor(CustomizationType.Body, color);

        ChooseColor(CustomizationType.Face, Color.white);
        ChooseColor(CustomizationType.Body, Color.white);
    }

    private void ChooseColor(CustomizationType type, Color color)
    {
        if (type == CustomizationType.Face)
        {
            playerFace.material.color = color;
            PlayerCustomization.FaceColor = color;
        }
        if (type == CustomizationType.Body)
        {
            playerBody.material.color = color;
            PlayerCustomization.BodyColor = color;
        }
    }

    private void SetRgb(CustomizationType type, bool isOn)
    {
        if (type == CustomizationType.Face)
        {
            PlayerCustomization.IsFaceRgb = isOn;
            if (!isOn) playerFace.material.color = PlayerCustomization.FaceColor;
        }
        else if (type == CustomizationType.Body)
        {
            PlayerCustomization.IsBodyRgb = isOn;
            if (!isOn) playerBody.material.color = PlayerCustomization.BodyColor;
        }
    }

    private void Update()
    {
        if (PlayerCustomization.IsBodyRgb)
            playerBody.material.color = Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1), 1, 1);
        if (PlayerCustomization.IsFaceRgb)
            playerFace.material.color = Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1), 1, 1);

    }
}

public struct PlayerCustomization
{
    public Color FaceColor;
    public Color BodyColor;
    public bool IsFaceRgb;
    public bool IsBodyRgb;
}

public enum CustomizationType
{
    Face, Body
}
