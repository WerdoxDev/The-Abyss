using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectOnDisable : MonoBehaviour {
    [SerializeField] private Selectable objectToSelect;

    private CustomButtonSelectable _customButton;
    private Tab _tab;
    private AdvancedCustomButton _advancedCustomButton;

    private void Awake() {
        _tab = GetComponent<Tab>();
        _customButton = GetComponent<CustomButtonSelectable>();
        _advancedCustomButton = GetComponent<AdvancedCustomButton>();
    }

    private void OnEnable() {
        if (EventSystem.current?.currentSelectedGameObject == gameObject) {
            if (_customButton != null) _customButton.OnSelect(new BaseEventData(EventSystem.current));
            else if (_advancedCustomButton != null) _advancedCustomButton.OnSelect(new BaseEventData(EventSystem.current));
            else if (_tab != null) _tab.OnSelect(new BaseEventData(EventSystem.current));
        }
    }

    private void OnDisable() {
        if (objectToSelect != null && ReselectLastSelected.Instance?.LastSelectedObject == gameObject)
            EventSystem.current?.SetSelectedGameObject(objectToSelect.gameObject);
    }
}
