using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Panel : MonoBehaviour
{
    [SerializeField] private UITweener[] openTweeners;
    [SerializeField] private UITweener[] closeTweeners;
    [SerializeField] private CustomButton[] closeButtons;
    [SerializeField] private bool closeOnDisable;
    private UIOrder _order;

    public bool IsOpen;

    private void OnDisable()
    {
        if (closeOnDisable) Close();
    }

    private void Awake()
    {
        _order = GetComponent<UIOrder>();
        if (closeButtons == null) return;
        for (int i = 0; i < closeButtons.Length; i++) closeButtons[i].OnClick += (eventData) => Close();
    }

    private void FixedUpdate()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) Close();
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open(bool instant = false)
    {
        if (IsOpen) return;
        IsOpen = true;

        for (int i = 0; i < openTweeners.Length; i++)
            openTweeners[i].HandleTween(instant);

        if (_order != null) _order.Order();
    }

    public void Close(bool instant = false, bool parentCalling = false)
    {
        if (!IsOpen) return;
        IsOpen = false;

        for (int i = 0; i < closeTweeners.Length; i++)
            closeTweeners[i].HandleTween(instant);

        if (_order != null) _order.RemoveOrder();

        if (parentCalling) return;
        foreach (Panel panel in GetComponentsInChildren<Panel>())
        {
            panel.Close(parentCalling: true);
        }
    }
}
