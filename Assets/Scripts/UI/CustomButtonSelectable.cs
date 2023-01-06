using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CustomButton), typeof(Selectable))]
public class CustomButtonSelectable : MonoBehaviour, ISelectHandler, IDeselectHandler {

    private CustomButton _customButton;

    private void Awake() {
        _customButton = GetComponent<CustomButton>();
    }

    public void OnSelect(BaseEventData eventData) {
        if (_customButton == null) _customButton = GetComponent<CustomButton>();

        _customButton.Enter(true);
        UIManager.Instance.SetEnteredCustomButton(_customButton);
    }

    public void OnDeselect(BaseEventData eventData) {
        _customButton.IsSelected = false;
        _customButton.Exit();
        UIManager.Instance.SetEnteredCustomButton(null);
    }
}
