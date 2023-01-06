using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class PromptPanel : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private CustomButton cancelButton;
    [SerializeField] private CustomButton confirmButton;
    [SerializeField] private CustomButton alternateButton;

    public Panel Panel;

    public event Action OnCancel;
    public event Action OnConfirm;
    public event Action OnAlternative;

    private void Awake() {
        Panel = GetComponent<Panel>();

        Panel.OnClosed += () => {
            OnCancel?.Invoke();
        };

        cancelButton.OnClick += () => {
            Panel.Close();
        };

        confirmButton.OnClick += () => {
            OnConfirm?.Invoke();
            Panel.Close();
        };

        alternateButton.OnClick += () => {
            OnAlternative?.Invoke();
            Panel.Close();
        };

        Panel.OnFullyClosed += () => Destroy(gameObject);
    }

    // private void OnDestroy() {
    //     Panel.OnClose -= OnCancel;
    // }

    public void SetTexts(string title, string body) {
        if (title == null) titleText.gameObject.SetActive(false);
        else {
            titleText.text = title;
            titleText.gameObject.SetActive(true);
        }

        if (body == null) bodyText.gameObject.SetActive(false);
        else {
            bodyText.text = body;
            bodyText.gameObject.SetActive(true);
        }
    }

    public void SetButtons(string cancelText, string confirmText, string alternateText) {
        cancelButton.gameObject.SetActive(cancelText != null);
        confirmButton.gameObject.SetActive(confirmText != null);
        alternateButton.gameObject.SetActive(alternateText != null);

        cancelButton.GetComponentInChildren<TMP_Text>().text = cancelText;
        confirmButton.GetComponentInChildren<TMP_Text>().text = confirmText;
        alternateButton.GetComponentInChildren<TMP_Text>().text = alternateText;
    }

    public void SetButtonsAction(Action onCancel, Action onConfirm, Action onAlternate) {
        OnCancel = onCancel;
        OnConfirm = onConfirm;
        OnAlternative = onAlternate;
    }

    // public void SetAlternate(Action onAlternate) => OnAlternative = onAlternate;
}
