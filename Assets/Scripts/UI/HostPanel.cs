using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class HostPanel : MonoBehaviour
{
    [SerializeField] private CustomButton createButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField hostIpInputField;

    private void Awake()
    {
        createButton.OnClick += (eventData) =>
        {
            string[] split = hostIpInputField.text.Split(":");
            if (split.Length != 2) return;
            GameManager.Instance.SetConnectionData(split[0], ushort.Parse(split[1]));
            TheAbyssNetworkManager.Instance.Host(new PlayerConnData(UIManager.Instance.Username));
        };
    }
}
