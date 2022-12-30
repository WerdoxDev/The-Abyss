using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CustomButton), typeof(Selectable))]
public class CustomButtonSelectable : MonoBehaviour, ISelectHandler, IDeselectHandler, ICancelHandler, ISubmitHandler {

    private CustomButton _customButton;

    private void Awake() {
        _customButton = GetComponent<CustomButton>();
    }

    public void OnSelect(BaseEventData eventData) => _customButton.Enter(true);

    public void OnDeselect(BaseEventData eventData) {
        _customButton.IsSelected = false;
        _customButton.Exit();
    }

    public void OnCancel(BaseEventData eventData) {
        _customButton.IsSelected = false;
        _customButton.Exit();
    }

    public void OnSubmit(BaseEventData eventData) => _customButton.Clicked(eventData);
}
