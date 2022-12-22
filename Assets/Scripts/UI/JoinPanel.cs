using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoinPanel : MonoBehaviour
{
    [SerializeField] private CustomButton joinButton;
    [SerializeField] private TMP_InputField serverNameInputField;
    [SerializeField] private TMP_InputField serverUrlInputField;

    private void Awake()
    {
        joinButton.OnClick += (eventData) =>
        {
            string[] split = serverUrlInputField.text.Split(":");
            if (split.Length != 2) return;
            GameManager.Instance.SetConnectionData(split[0], ushort.Parse(split[1]));
            TheAbyssNetworkManager.Instance.Client(new PlayerConnData(UIManager.Instance.Username));
        };
    }
}
