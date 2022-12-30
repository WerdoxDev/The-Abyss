using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class SettingsOption : MonoBehaviour {
    [Header("Settings")]
    public Option[] options;
    [SerializeField] private CustomButton nextButton;
    [SerializeField] private CustomButton previousButton;
    [SerializeField] private TMP_Text optionText;
    [SerializeField] private Transform indicatorWrapper;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;
    private List<Image> indicatorImages = new List<Image>();
    private SettingsPanel _settingsPanel;
    private AdvancedCustomButton _advancedButton;
    private int _currentIndex;

    public event Action<Option> OnChanged;

    private void Awake() {
        _settingsPanel = GetComponentInParent<SettingsPanel>();
        _advancedButton = GetComponent<AdvancedCustomButton>();

        nextButton.OnClick += (eventData) => NextOption();
        previousButton.OnClick += (eventData) => PreviousOption();

        _settingsPanel.OnMove += (direction) => {
            if (!_advancedButton.IsSelected) return;
            if (direction == 1) NextOption();
            else if (direction == -1) PreviousOption();
        };

        CreateOptionIndicator();
    }

    public void SetOptions(Option[] options) {
        this.options = options;
        CreateOptionIndicator();
    }

    public void SelectOptionByValue(int value) {
        _currentIndex = options.TakeWhile(x => x.Value != value).Count();
        ShowSelectedOption();
    }

    private void CreateOptionIndicator() {
        if (options.Length == 0) return;

        for (int i = 0; i < indicatorWrapper.childCount; i++)
            Destroy(indicatorWrapper.GetChild(i).gameObject);

        indicatorImages.Clear();
        for (int i = 0; i < options.Length; i++) {
            GameObject go = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity, indicatorWrapper);
            indicatorImages.Add(go.GetComponent<Image>());
        }

        ShowSelectedOption();
    }

    private void NextOption() {
        _currentIndex++;
        if (_currentIndex == options.Length) _currentIndex = 0;
        // ShowSelectedOption();
        OnChanged?.Invoke(options[_currentIndex]);
    }

    private void PreviousOption() {
        _currentIndex--;
        if (_currentIndex < 0) _currentIndex = options.Length - 1;

        // ShowSelectedOption();
        OnChanged?.Invoke(options[_currentIndex]);
    }

    private void ShowSelectedOption() {
        optionText.text = options[_currentIndex].Text;

        for (int i = 0; i < indicatorImages.Count; i++) {
            if (_currentIndex != i) indicatorImages[i].color = normalColor;
            else indicatorImages[i].color = selectedColor;
        }
    }
}