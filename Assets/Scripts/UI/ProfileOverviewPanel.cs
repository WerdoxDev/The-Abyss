using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileOverviewPanel : MonoBehaviour
{
    [SerializeField] private CustomButton changeUsernameButton;
    [SerializeField] private TMP_Text usernameText;

    private void Awake()
    {
        usernameText.text = UIManager.Instance.Username;
        changeUsernameButton.OnClick += (eventData) => UIManager.Instance.ChangeNamePanel.Open();
        UIManager.Instance.OnUsernameChanged += (username) => usernameText.text = username;
    }
}
