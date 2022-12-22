using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlinePanel : MonoBehaviour
{
    [SerializeField] private CustomButton joinButton;
    [SerializeField] private CustomButton hostButton;

    private void Awake()
    {
        joinButton.OnClick += (eventData) => UIManager.Instance.JoinPanel.Open();
        hostButton.OnClick += (eventData) => UIManager.Instance.HostPanel.Open();
    }
}
