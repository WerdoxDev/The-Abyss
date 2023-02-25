using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class SettingsOption : MonoBehaviour {
    [Header("Settings")]
    public Option[] Options;
    [TextArea(3, 0)] public string Description;

    [SerializeField] private CustomButton nextButton;
    [SerializeField] private CustomButton previousButton;
    [SerializeField] private TMP_Text optionText;
    [SerializeField] private Transform indicatorWrapper;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;
    private readonly List<Image> _indicatorImages = new();
    private SettingsPanel _settingsPanel;
    private AdvancedCustomButton _advancedButton;

    public int CurrentIndex { get; private set; }

    public event Action<Option> OnChanged;

    private void Awake() {
        _settingsPanel = GetComponentInParent<SettingsPanel>();
        _advancedButton = GetComponent<AdvancedCustomButton>();

        _settingsPanel.AddOption(this);

        nextButton.OnClick += () => NextOption();
        previousButton.OnClick += () => PreviousOption();

        _settingsPanel.OnMove += (direction) => {
            if (!_advancedButton.IsSelected) return;
            if (direction == 1) NextOption();
            else if (direction == -1) PreviousOption();
        };

        _advancedButton.OnEnter += () => {
            _settingsPanel.ShowSettingDescription(this);
        };

        CreateOptionIndicator();
    }

    public void SetOptions(Option[] options) {
        this.Options = options;
        CreateOptionIndicator();
    }

    public void SelectOptionByValue(int value) {
        CurrentIndex = Options.TakeWhile(x => x.Value != value).Count();
        ShowSelectedOption();
    }

    private void CreateOptionIndicator() {
        if (Options.Length == 0) return;

        for (int i = 0; i < indicatorWrapper.childCount; i++)
            Destroy(indicatorWrapper.GetChild(i).gameObject);

        _indicatorImages.Clear();
        for (int i = 0; i < Options.Length; i++) {
            GameObject go = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity, indicatorWrapper);
            _indicatorImages.Add(go.GetComponent<Image>());
        }

        ShowSelectedOption();
    }

    private void NextOption() {
        CurrentIndex++;
        if (CurrentIndex == Options.Length) CurrentIndex = 0;

        OnChanged?.Invoke(Options[CurrentIndex]);
    }

    private void PreviousOption() {
        CurrentIndex--;
        if (CurrentIndex < 0) CurrentIndex = Options.Length - 1;

        OnChanged?.Invoke(Options[CurrentIndex]);
    }

    private void ShowSelectedOption() {
        optionText.text = Options[CurrentIndex].Text;

        for (int i = 0; i < _indicatorImages.Count; i++) {
            if (CurrentIndex != i) _indicatorImages[i].color = normalColor;
            else _indicatorImages[i].color = selectedColor;
        }
    }
}