using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class ColorButton : MonoBehaviour, IPointerClickHandler {
    public event Action<Color> OnClick;

    private Image _colorImage;

    private void Awake() {
        _colorImage = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(_colorImage.color);
}
