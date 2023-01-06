using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

public class Panel : MonoBehaviour {
    public PanelType Type = PanelType.Untyped;

    [SerializeField] private UITweener[] openTweeners;
    [SerializeField] private UITweener[] closeTweeners;
    [SerializeField] private CustomButton[] closeButtons;
    [SerializeField] private CustomButton submitButton;

    public Selectable SelectableOnOpen;
    public Selectable SelectableOnClose;

    [SerializeField] private bool addPanelToManager = true;

    public bool CloseOnDisable;
    public bool CloseOnCancel = true;
    public bool FullyOpened { get; private set; }
    public bool FullyClosed { get; private set; } = true;
    public bool IsOpen { get; private set; }

    public event Action OnOpened;
    public event Action OnClosed;
    public event Action OnClosedWithButton;
    public event Action OnFullyOpened;
    public event Action OnFullyClosed;

    private UIOrder _order;
    private bool _isCancelled;

    private void OnDisable() {
        if (CloseOnDisable) Close();
    }

    private void Awake() {
        _order = GetComponent<UIOrder>();

        if (closeButtons == null) return;
        for (int i = 0; i < closeButtons.Length; i++) closeButtons[i].OnClick += () => {
            Close();
            OnClosedWithButton?.Invoke();
        };

        UITweener longestOpenTween = null;
        UITweener longestCloseTween = null;

        foreach (UITweener tweener in openTweeners)
            if (longestOpenTween == null || tweener.Duration > longestOpenTween.Duration) longestOpenTween = tweener;

        foreach (UITweener tweener in closeTweeners)
            if (longestCloseTween == null || tweener.Duration > longestCloseTween.Duration) longestCloseTween = tweener;

        if (longestOpenTween != null)
            longestOpenTween.OnTweenFinished += () => {
                OnFullyOpened?.Invoke();
                FullyOpened = true;
            };

        if (longestCloseTween != null)
            longestCloseTween.OnTweenFinished += () => {
                OnFullyClosed?.Invoke();
                FullyClosed = true;
            };
    }

    public void Submit() => submitButton?.Submit();

    public void Toggle() {
        if (IsOpen) Close();
        else Open();
    }

    public void Open(bool instant = false) {
        if (IsOpen || !FullyClosed) return;

        UIManager.Instance.PanelChangeStateAttempt(this, true, () => _isCancelled = true);
        if (_isCancelled) {
            _isCancelled = false;
            return;
        }

        FullyOpened = false;
        IsOpen = true;

        if (addPanelToManager) UIManager.Instance.AddOpenedPanel(this);

        if (openTweeners.Length == 0) {
            gameObject.SetActive(true);
            FullyOpened = true;
        }

        for (int i = 0; i < openTweeners.Length; i++)
            openTweeners[i].HandleTween(instant);

        if (_order != null) _order.Order();

        if (SelectableOnOpen != null) EventSystem.current.SetSelectedGameObject(SelectableOnOpen.gameObject);

        OnOpened?.Invoke();
    }

    public void Close(bool instant = false, bool parentCalling = false) {
        if (!IsOpen || !FullyOpened) return;

        UIManager.Instance.PanelChangeStateAttempt(this, false, () => _isCancelled = true);
        if (_isCancelled) {
            _isCancelled = false;
            return;
        }

        FullyClosed = false;
        IsOpen = false;

        if (closeTweeners.Length == 0) {
            gameObject.SetActive(false);
            FullyClosed = true;
        }

        if (addPanelToManager) UIManager.Instance.RemoveOpenedPanel(this);

        for (int i = 0; i < closeTweeners.Length; i++)
            closeTweeners[i].HandleTween(instant);

        if (_order != null) _order.RemoveOrder();

        if (SelectableOnClose != null) EventSystem.current.SetSelectedGameObject(SelectableOnClose.gameObject);

        OnClosed?.Invoke();

        if (parentCalling) return;
        foreach (Panel panel in GetComponentsInChildren<Panel>()) {
            panel.Close(parentCalling: true);
        }
    }
}

public enum PanelType {
    Untyped,
    Join,
    Host,
    ChangeName,
    InGameSettings,
    InGameMenu
}