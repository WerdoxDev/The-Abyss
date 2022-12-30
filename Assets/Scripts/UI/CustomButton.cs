using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
    [SerializeField] private Graphic graphic;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color enterColor;
    [SerializeField] private Vector3 enterScale;
    [SerializeField] private LeanTweenType easeType;
    [SerializeField] private float duration;
    [SerializeField] private bool changeColor = true;
    [SerializeField] private bool changeScale = true;
    [SerializeField] private bool tween = true;
    [SerializeField] private bool resetOnDisable = true;
    [SerializeField] private bool lockIfParentExists = true;
    private UIOrder _order;
    private AdvancedCustomButton _parentButton;

    public bool IsSelected;
    public event Action<BaseEventData> OnClick;

    private void Awake() {
        _order = GetComponent<UIOrder>();
        _parentButton = GetComponentInParent<AdvancedCustomButton>();
    }

    private void OnDisable() {
        if (resetOnDisable) {
            IsSelected = false;
            Exit();
        };
    }

    public void Enter(bool setSelected = false) {
        if (setSelected) IsSelected = true;

        if (graphic != null) LeanTween.cancel(graphic.rectTransform);

        if (_parentButton != null && lockIfParentExists) _parentButton.LockGraphic(graphic.gameObject);

        if (!tween) {
            if (changeColor) graphic.color = enterColor;
            if (changeScale) graphic.rectTransform.localScale = enterScale;
            return;
        }

        if (changeColor) LeanTween.color(graphic.rectTransform, enterColor, duration).setEase(easeType).setRecursive(false);

        if (changeScale) {
            if (_order != null) _order.Order();
            LeanTween.scale(graphic.rectTransform, enterScale, duration).setEase(easeType);
        }
    }

    public void Exit() {
        if (IsSelected) return;

        if (_parentButton != null && lockIfParentExists) _parentButton.UnlockGraphic(graphic.gameObject);

        if (!tween) {
            if (changeColor) graphic.color = normalColor;
            if (changeScale) graphic.rectTransform.localScale = Vector3.one;
            return;
        };

        if (changeColor) LeanTween.color(graphic.rectTransform, normalColor, duration).setEase(easeType).setRecursive(false);

        if (changeScale) LeanTween.scale(graphic.rectTransform, Vector3.one, duration).setEase(easeType).setOnComplete(() => {
            if (_order != null) _order.RemoveOrder();
        });
    }

    public void Clicked(BaseEventData eventData) => OnClick?.Invoke(eventData);

    public void OnPointerClick(PointerEventData eventData) => Clicked(eventData);

    public void OnPointerEnter(PointerEventData eventData) => Enter();

    public void OnPointerExit(PointerEventData eventData) => Exit();

    // Selectable parent will prevent OnPointerClick if this doesn't exists
    public void OnPointerDown(PointerEventData eventData) { }
}
