using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PromptManager : MonoBehaviour {
    public static PromptManager Instance;

    [Header("Settings")]
    [SerializeField] private GameObject promptPrefab;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public PromptPanel CreatePrompt(string title, string body, string cancelText, string confirmText, string alternateText, Action onCancel, Action onConfirm, Action onAlternate) {
        GameObject promptGo = Instantiate(promptPrefab, transform);
        PromptPanel prompt = promptGo.GetComponent<PromptPanel>();
        prompt.SetTexts(title, body);
        prompt.SetButtons(cancelText, confirmText, alternateText);
        prompt.SetButtonsAction(onCancel, onConfirm, onAlternate);
        prompt.Panel.Open();
        return prompt;
    }
}
