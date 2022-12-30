using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class InputNavigator : MonoBehaviour {
    private TMP_InputField _inputField;

    private void Awake() {
        _inputField = GetComponent<TMP_InputField>();
    }

    private void Update() {
        if (Keyboard.current.tabKey.wasPressedThisFrame) {
            Selectable next = _inputField.FindSelectableOnDown();
            if (next != null) EventSystem.current.SetSelectedGameObject(next.gameObject);
        }
    }
}
