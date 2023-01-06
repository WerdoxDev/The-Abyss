using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPanel : MonoBehaviour {
    [SerializeField] private TMP_Text interactionKeyText;
    [SerializeField] private TMP_Text interactionText;

    public void SetTarget(Vector3 worldPosition, string key, string text) {
        transform.position = UIManager.Instance.WorldToScreen(worldPosition);

        interactionText.text = text;
        interactionKeyText.text = key;

        gameObject.SetActive(true);
    }

    public void ClearTarget() => gameObject.SetActive(false);
}
