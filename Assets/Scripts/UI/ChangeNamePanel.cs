using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeNamePanel : MonoBehaviour
{
    [SerializeField] private CustomButton saveButton;
    [SerializeField] private TMP_InputField usernameInputField;

    private void OnEnable()
    {
        usernameInputField.text = UIManager.Instance.Username;
    }

    private void Awake()
    {
        saveButton.OnClick += (eventData) =>
        {
            UIManager.Instance.UsernameChanged(usernameInputField.text);
            GetComponent<Panel>().Close();
        };
    }
}
