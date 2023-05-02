using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, InputControls.IPlayerActions {
    public event Action<Vector2> LookEvent;
    public event Action<Vector2> MoveEvent;
    public event Action<ButtonType, bool> ButtonEvent;

    public event Action<Vector2> UIMoveEvent;
    public event Action<int> UIChangeTabEvent;
    public event Action<UIButtonType, bool> UIButtonEvent;

    private InputControls _controls;

    public void Init() {
        _controls = new InputControls();
        _controls.Player.SetCallbacks(this);

        EnablePlayerControls();
        EnableUIControls();

        _controls.UI.Navigate.performed += (context) => UIMoveEvent?.Invoke(context.ReadValue<Vector2>());
        _controls.UI.ChangeTab.performed += (context) => UIChangeTabEvent?.Invoke((int)context.ReadValue<float>());
        _controls.UI.Apply.performed += (context) => SendUIButtonEvent(context, UIButtonType.Apply);
        _controls.UI.Cancel.performed += (context) => SendUIButtonEvent(context, UIButtonType.Cancel);
        _controls.UI.Submit.performed += (context) => SendUIButtonEvent(context, UIButtonType.Submit);
    }

    public void OnLook(InputAction.CallbackContext context) => LookEvent?.Invoke(context.ReadValue<Vector2>());
    public void OnMove(InputAction.CallbackContext context) => MoveEvent?.Invoke(context.ReadValue<Vector2>());

    public void OnJump(InputAction.CallbackContext context) => SendButtonEvent(context, ButtonType.Jump);
    public void OnInteract(InputAction.CallbackContext context) => SendButtonEvent(context, ButtonType.Interact);
    public void OnSprint(InputAction.CallbackContext context) => SendButtonEvent(context, ButtonType.Sprint);
    public void OnPause(InputAction.CallbackContext context) => SendButtonEvent(context, ButtonType.Pause);
    public void OnChat(InputAction.CallbackContext context) => SendButtonEvent(context, ButtonType.Chat);

    public void SendButtonEvent(InputAction.CallbackContext context, ButtonType type) {
        if (context.performed) ButtonEvent?.Invoke(type, true);
        else if (context.canceled) ButtonEvent?.Invoke(type, false);
    }

    public void SendUIButtonEvent(InputAction.CallbackContext context, UIButtonType type) {
        if (context.performed) UIButtonEvent?.Invoke(type, true);
        else if (context.canceled) UIButtonEvent?.Invoke(type, false);
    }

    public void SendUIButtonEvent(UIButtonType type) => UIButtonEvent?.Invoke(type, true);

    public void EnablePlayerControls() {
        _controls.Player.Enable();
    }

    public void EnableUIControls() {
        _controls.UI.Enable();
    }
}

public enum ButtonType {
    Jump,
    Interact,
    Sprint,
    Pause,
    Chat,
}

public enum UIButtonType {
    Submit,
    Cancel,
    Apply,
}
