using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class KeybindLegend : MonoBehaviour {
    public static KeybindLegend Instance;

    [SerializeField] private CustomButton backButton;
    [SerializeField] private CustomButton applyButton;

    public bool IsApplyVisible;
    public bool IsBackVisible;

    public event Action OnApplyButtonClicked;
    public event Action OnBackButtonClicked;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        applyButton.OnClick += () => OnApplyButtonClicked?.Invoke();
        backButton.OnClick += () => OnBackButtonClicked?.Invoke();

        HideApplyButton();
    }

    public void ShowApplyButton() {
        applyButton.gameObject.SetActive(true);
        IsApplyVisible = true;
    }

    public void HideApplyButton() {
        applyButton.gameObject.SetActive(false);
        IsApplyVisible = false;
    }
}
