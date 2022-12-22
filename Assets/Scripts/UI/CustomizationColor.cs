using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CustomizationColor : MonoBehaviour
{
    [SerializeField] private Transform colorsTransform;
    [SerializeField] private Toggle rgbToggle;

    public event Action<Color> OnChooseColor;
    public event Action<bool> OnSetRgb;

    private void Awake()
    {
        rgbToggle.isOn = false;
        rgbToggle.onValueChanged.AddListener((isOn) => OnSetRgb?.Invoke(isOn));
        foreach (RectTransform child in colorsTransform)
        {
            if (!child.TryGetComponent<ColorButton>(out ColorButton colorButton))
                colorButton = child.gameObject.AddComponent<ColorButton>();

            colorButton.OnClick += (color) => OnChooseColor?.Invoke(color);
        }
    }
}
