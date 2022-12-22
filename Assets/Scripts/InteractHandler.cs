using System;
using UnityEngine;
using Unity.Netcode;

public class InteractHandler : NetworkBehaviour
{
    public NetworkVariable<bool> Occupied = new NetworkVariable<bool>(false);
    [HideInInspector] public NetworkBehaviour interactable;

    public InteractType Type;

    public Transform StandTransform;
    public Vector3 StandPos { get => StandTransform.position; }
    public Vector3 LocalStandPos { get => StandTransform.localPosition; }

    public string UiText;
    // Usucally used to know which side of an interactable was used
    public byte Data;

    public bool IsLongPress;

    public bool Active = true;

    public Action InteractEvent;

    public void Interact() => InteractEvent?.Invoke();
}

public enum InteractType
{
    Capstan,
    Steering,
    Ladder,
    SailControl
}