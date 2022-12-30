using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomSelectable : Selectable {
    [SerializeField] private Selectable[] selectablesOnUp;
    [SerializeField] private Selectable[] selectablesOnDown;
    [SerializeField] private Selectable[] selectablesOnLeft;
    [SerializeField] private Selectable[] selectablesOnRight;

    private new void OnValidate() {
        navigation = new Navigation() { mode = Navigation.Mode.Automatic };
        transition = Transition.None;
    }

    public override Selectable FindSelectableOnDown() => GetActiveSelectable(selectablesOnDown);
    public override Selectable FindSelectableOnLeft() => GetActiveSelectable(selectablesOnLeft);
    public override Selectable FindSelectableOnRight() => GetActiveSelectable(selectablesOnRight);
    public override Selectable FindSelectableOnUp() => GetActiveSelectable(selectablesOnUp);

    private Selectable GetActiveSelectable(Selectable[] selectables) {
        Selectable activeSelectable = null;
        foreach (Selectable selectable in selectables) {
            if (selectable.gameObject.activeInHierarchy) {
                activeSelectable = selectable;
                break;
            }
        }
        return activeSelectable;
    }
}
