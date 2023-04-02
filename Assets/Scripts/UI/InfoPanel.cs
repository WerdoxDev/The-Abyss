using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour {
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text statusText;

    private void Awake() {
        usernameText.text = UIManager.Instance.PlayerName;
        UIManager.Instance.OnPlayerNameChanged += (username) => usernameText.text = username;

        if (Services.Instance.OfflineMode) statusText.text = "Offline";
        else statusText.text = "Online";
    }
}
