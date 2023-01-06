using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeNamePanel : MonoBehaviour {
    [SerializeField] private CustomButton saveButton;
    [SerializeField] private TMP_InputField usernameInputField;

    private void OnEnable() {
        usernameInputField.text = UIManager.Instance.PlayerName;
    }

    private void Awake() {
        saveButton.OnClick += () => {
            UIManager.Instance.PlayerNameChanged(usernameInputField.text);
            GetComponent<Panel>().Close();
        };
    }
}
