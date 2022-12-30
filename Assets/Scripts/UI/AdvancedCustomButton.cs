using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AdvancedCustomButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ICancelHandler, ISubmitHandler {
    [SerializeField] private Graphic[] graphics;
    [SerializeField] private Color[] normalColors;
    [SerializeField] private Color[] enterColors;
    [SerializeField] private Vector3 enterScale;
    [SerializeField] private LeanTweenType easeType;
    [SerializeField] private float duration;
    [SerializeField] private bool scaleTween = true;
    [SerializeField] private bool resetOnDisable = true;
    private List<int> _lockedIndexes = new List<int>();

    public bool IsSelected;
    public Action<BaseEventData> OnClick;

    private void OnDisable() {
        if (resetOnDisable) Exit();
    }

    public void LockGraphic(GameObject gameObject) {
        for (int i = 0; i < graphics.Length; i++)
            if (graphics[i].gameObject == gameObject) _lockedIndexes.Add(i);
    }

    public void UnlockGraphic(GameObject gameObject) {
        for (int i = 0; i < graphics.Length; i++)
            if (graphics[i].gameObject == gameObject && _lockedIndexes.Contains(i)) _lockedIndexes.Remove(i);
    }

    public void Enter(bool setSelected = false) {
        if (setSelected) IsSelected = true;

        for (int i = 0; i < graphics.Length; i++) {
            if (_lockedIndexes.Contains(i)) continue;
            LeanTween.cancel(graphics[i].gameObject);
            LeanTween.graphicColor(graphics[i].rectTransform, enterColors[i], duration).setEase(easeType).setRecursive(false);
        }

        if (!scaleTween) return;
        RectTransform rectTransform = GetComponent<RectTransform>();
        LeanTween.scale(rectTransform, enterScale, duration).setEase(easeType);
    }

    public void Exit() {
        if (IsSelected) return;

        for (int i = 0; i < graphics.Length; i++)
            LeanTween.graphicColor(graphics[i].rectTransform, normalColors[i], duration).setEase(easeType).setRecursive(false);

        if (!scaleTween) return;
        RectTransform rectTransform = GetComponent<RectTransform>();
        LeanTween.scale(rectTransform, Vector3.one, duration).setEase(easeType);
    }

    public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(eventData);

    public void OnPointerEnter(PointerEventData eventData) => Enter();

    public void OnPointerExit(PointerEventData eventData) => Exit();

    public void OnSelect(BaseEventData eventData) => Enter(true);

    public void OnDeselect(BaseEventData eventData) {
        IsSelected = false;
        Exit();
        Debug.Log(eventData.selectedObject.name);
    }

    public void OnCancel(BaseEventData eventData) {
        IsSelected = false;
        Exit();
        Debug.Log("adv cancel");
    }

    public void OnSubmit(BaseEventData eventData) => OnClick?.Invoke(eventData);
}
