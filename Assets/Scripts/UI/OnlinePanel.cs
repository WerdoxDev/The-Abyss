using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlinePanel : MonoBehaviour {
    [SerializeField] private CustomButton joinButton;
    [SerializeField] private CustomButton hostButton;

    private void Awake() {
        joinButton.OnClick += () => UIManager.Instance.JoinPanel.Open();
        hostButton.OnClick += () => UIManager.Instance.HostPanel.Open();
    }
}
