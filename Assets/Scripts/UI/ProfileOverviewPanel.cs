using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileOverviewPanel : MonoBehaviour {
    [SerializeField] private CustomButton changeUsernameButton;
    [SerializeField] private TMP_Text usernameText;

    private void Awake() {
        usernameText.text = UIManager.Instance.PlayerName;
        changeUsernameButton.OnClick += () => UIManager.Instance.ChangeNamePanel.Open();
        UIManager.Instance.OnPlayerNameChanged += (username) => usernameText.text = username;
    }
}
