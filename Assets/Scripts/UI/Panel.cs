using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

public class Panel : MonoBehaviour {
    [SerializeField] private bool closeOnDisable;
    [SerializeField] private bool closeOnEscape;
    [SerializeField] private UITweener[] openTweeners;
    [SerializeField] private UITweener[] closeTweeners;
    [SerializeField] private CustomButton[] closeButtons;
    [SerializeField] private CustomButton submitButton;

    public Selectable SelectableOnOpen;
    public Selectable SelectableOnClose;
    public bool FullyOpened;
    public bool IsOpen;

    public event Action OnOpened;
    public event Action OnClosed;
    public event Action OnFullyOpened;
    public event Action OnFullyClosed;

    private UIOrder _order;

    private void OnEnable() => SetInputState(true);

    private void OnDisable() {
        if (closeOnDisable) Close();
        SetInputState(false);
    }

    private void OnDestroy() => SetInputState(false);

    private void Awake() {
        _order = GetComponent<UIOrder>();

        if (closeButtons == null) return;
        for (int i = 0; i < closeButtons.Length; i++) closeButtons[i].OnClick += (eventData) => Close();

        UITweener longestOpenTween = null;
        UITweener longestCloseTween = null;

        foreach (UITweener tweener in openTweeners)
            if (longestOpenTween == null || tweener.Duration > longestOpenTween.Duration) longestOpenTween = tweener;

        foreach (UITweener tweener in closeTweeners)
            if (longestCloseTween == null || tweener.Duration > longestCloseTween.Duration) longestCloseTween = tweener;


        longestOpenTween.OnTweenFinished += () => {
            OnFullyOpened?.Invoke();
            FullyOpened = true;
        };

        longestCloseTween.OnTweenFinished += () => OnFullyClosed?.Invoke();

        IsOpen = gameObject.activeInHierarchy;
    }

    private void FixedUpdate() {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && closeOnEscape && FullyOpened) Close();
    }

    public void Toggle() {
        if (IsOpen) Close();
        else Open();
    }

    public void Open(bool instant = false) {
        if (IsOpen) return;
        IsOpen = true;

        for (int i = 0; i < openTweeners.Length; i++)
            openTweeners[i].HandleTween(instant);

        if (_order != null) _order.Order();

        if (SelectableOnOpen != null) EventSystem.current.SetSelectedGameObject(SelectableOnOpen.gameObject);

        OnOpened?.Invoke();
    }

    public void Close(bool instant = false, bool parentCalling = false) {
        if (!IsOpen) return;
        IsOpen = false;

        for (int i = 0; i < closeTweeners.Length; i++)
            closeTweeners[i].HandleTween(instant);

        if (_order != null) _order.RemoveOrder();

        FullyOpened = false;
        OnClosed?.Invoke();

        if (SelectableOnClose != null) EventSystem.current.SetSelectedGameObject(SelectableOnClose.gameObject);

        if (parentCalling) return;
        foreach (Panel panel in GetComponentsInChildren<Panel>()) {
            panel.Close(parentCalling: true);
        }
    }

    private void SetInputState(bool enabled) {
        void OnUIButtonEvent(UIButtonType type, bool performed) {
            if (!performed || submitButton == null) return;
            if (type == UIButtonType.Submit) submitButton.Clicked(new BaseEventData(EventSystem.current));
        }

        if (enabled) UIManager.Instance.InputReader.UIButtonEvent += OnUIButtonEvent;
        else UIManager.Instance.InputReader.UIButtonEvent -= OnUIButtonEvent;
    }
}
