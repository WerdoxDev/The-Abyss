using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour {
    public static CrosshairManager Instance;

    [Header("Settings")]
    [SerializeField] private Image crosshairGraphic;
    [SerializeField] private CrosshairVariant[] crosshairVariants;

    public CrosshairState CurrentState;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }

        SetState(CrosshairState.Hidden);
    }

    public void SetState(CrosshairState newState) {
        CurrentState = newState;

        crosshairGraphic.gameObject.SetActive(newState != CrosshairState.Hidden);

        if (newState == CrosshairState.Hidden) return;

        CrosshairVariant variant = crosshairVariants.FirstOrDefault(x => x.State == CurrentState);
        crosshairGraphic.sprite = variant.Sprite;
        crosshairGraphic.color = variant.Color;
    }
}

[Serializable]
public struct CrosshairVariant {
    public CrosshairState State;
    public Sprite Sprite;
    public Color Color;
}

public enum CrosshairState {
    Hidden,
    Neutral,
    Interact,
}
