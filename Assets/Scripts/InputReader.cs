using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, InputControls.IMovementActions
{
    public event Action<Vector2> LookEvent;
    public event Action<Vector2> MoveEvent;
    public event Action<ButtonType, bool> ButtonEvent;

    private InputControls _controls;

    public void Init()
    {
        _controls = new InputControls();
        _controls.Movement.SetCallbacks(this);

        EnableMovementControls();
    }

    public void OnLook(InputAction.CallbackContext context) => LookEvent(context.ReadValue<Vector2>());
    public void OnMove(InputAction.CallbackContext context) => MoveEvent(context.ReadValue<Vector2>());

    public void OnJump(InputAction.CallbackContext context) => SendButtonEvent(context, ButtonType.Jump);
    public void OnInteract(InputAction.CallbackContext context) => SendButtonEvent(context, ButtonType.Interact);
    public void OnSprint(InputAction.CallbackContext context) => SendButtonEvent(context, ButtonType.Sprint);

    public void SendButtonEvent(InputAction.CallbackContext context, ButtonType type)
    {
        if (context.performed)
            ButtonEvent?.Invoke(type, true);
        else if (context.canceled)
            ButtonEvent?.Invoke(type, false);
    }

    public void EnableMovementControls()
    {
        _controls.Movement.Enable();
    }
}

public enum ButtonType
{
    Jump,
    Interact,
    Sprint
}
