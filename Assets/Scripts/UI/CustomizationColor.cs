using System;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationColor : MonoBehaviour {
    [SerializeField] private Transform colorsTransform;
    [SerializeField] private Toggle rgbToggle;

    public event Action<Color> OnChooseColor;
    public event Action<bool> OnSetRgb;

    private void Awake() {
        rgbToggle.onValueChanged.AddListener((isOn) => OnSetRgb?.Invoke(isOn));
        foreach (RectTransform child in colorsTransform) {
            if (!child.TryGetComponent(out ColorButton colorButton))
                colorButton = child.gameObject.AddComponent<ColorButton>();

            colorButton.OnClick += (color) => OnChooseColor?.Invoke(color);
        }
    }

    public void SetRgb(bool isRgb, bool invokeEvent = false) {
        rgbToggle.isOn = isRgb;
    }
}
