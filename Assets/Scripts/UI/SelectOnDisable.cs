using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectOnDisable : MonoBehaviour {
    [SerializeField] private Selectable objectToSelect;

    private CustomButton _customButton;
    private Tab _tab;
    private AdvancedCustomButton _advancedCustomButton;

    private void Awake() {
        _customButton = GetComponent<CustomButton>();
        _tab = GetComponent<Tab>();
        _advancedCustomButton = GetComponent<AdvancedCustomButton>();
    }

    private void OnEnable() {
        if (EventSystem.current?.currentSelectedGameObject == gameObject) {
            if (_customButton != null) _customButton.Enter();
            else if (_advancedCustomButton != null) _advancedCustomButton.Enter();
            else if (_tab != null) _tab.TabGroup.OnTabEnter(_tab);
        }
    }

    private void OnDisable() {
        if (objectToSelect != null && ReselectLastSelected.Instance?.LastSelectedObject == gameObject)
            EventSystem.current?.SetSelectedGameObject(objectToSelect.gameObject);
    }
}
