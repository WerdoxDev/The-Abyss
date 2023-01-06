using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerNamePanel : MonoBehaviour {
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject wrapper;
    private Transform _playerToFollow;
    private Vector3 _offset;
    private Camera _customCamera;
    private RectTransform _customRect;

    private void FixedUpdate() {
        if (_playerToFollow == null) return;

        bool isInCameraBounds = UIManager.Instance.IsInCameraBounds(_playerToFollow.position, _customCamera);
        if (!isInCameraBounds && wrapper.activeSelf) wrapper.SetActive(false);
        else if (isInCameraBounds && !wrapper.activeSelf) wrapper.SetActive(true);

        transform.position = UIManager.Instance.WorldToScreen(_playerToFollow.position + _offset, _customCamera, _customRect);
    }

    public void SetPlayer(Transform player, string playerName, Vector3 offset, Camera customCamera = null, RectTransform customRect = null) {
        _playerToFollow = player;
        _offset = offset;
        _customCamera = customCamera;
        _customRect = customRect;

        nameText.text = playerName;
        nameText.ForceMeshUpdate();

        TMP_TextInfo info = nameText.textInfo;

        string matchString = "[EmamZaman]";
        if (!nameText.text.StartsWith(matchString)) return;

        for (int i = 0; i < matchString.Length; ++i) {
            int meshIndex = info.characterInfo[i].materialReferenceIndex;
            int vertexIndex = info.characterInfo[i].vertexIndex;

            Color32[] vertexColors = nameText.textInfo.meshInfo[meshIndex].colors32;
            vertexColors[vertexIndex + 0] = Color.red;
            vertexColors[vertexIndex + 1] = Color.red;
            vertexColors[vertexIndex + 2] = Color.red;
            vertexColors[vertexIndex + 3] = Color.red;
        }

        nameText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}
