using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCustomization : MonoBehaviour {

    [Header("Material Settings")]
    [SerializeField] private MeshRenderer playerBody;
    [SerializeField] private MeshRenderer playerHead;
    [SerializeField] private MeshRenderer playerLeftEye;
    [SerializeField] private MeshRenderer playerRightEye;

    [Header("Settings")]
    public Vector3 NameOffset;

    private PlayerCustomizationInfo _customization;

    private void Awake() {
        UIManager.Instance.OnPlayerNameChanged += PlayerNameChanged;
    }

    private void OnDestroy() => UIManager.Instance.OnPlayerNameChanged -= PlayerNameChanged;

    public void UpdatePlayer(string name, PlayerCustomizationInfo customization) {
        _customization = customization;

        playerBody.material.color = customization.BodyColor;
        playerHead.material.color = customization.HeadColor;
        playerLeftEye.material.color = customization.LeftEyeColor;
        playerRightEye.material.color = customization.RightEyeColor;
    }

    private void Update() {
        if (_customization.IsHeadRgb)
            playerHead.material.color = Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1), 1, 1);
        if (_customization.IsBodyRgb)
            playerBody.material.color = Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1), 1, 1);
        if (_customization.IsLeftEyeRgb)
            playerLeftEye.material.color = Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1), 1, 1);
        if (_customization.IsRightEyeRgb)
            playerRightEye.material.color = Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1), 1, 1);
    }

    private void PlayerNameChanged(string playerName) => UpdatePlayer(playerName, _customization);
}
