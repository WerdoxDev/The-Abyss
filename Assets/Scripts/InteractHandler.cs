using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class InteractHandler : NetworkBehaviour {
    public NetworkVariable<bool> Occupied = new(false);
    [HideInInspector] public NetworkBehaviour Interactable;

    public InteractType Type;

    public string UIText;
    // Usucally used to know which side of an interactable was used
    public byte Data;

    public bool Active = true;

    public UnityEvent<byte> InteractEvent;

    public void Interact() => InteractEvent?.Invoke(Data);
}

public enum InteractType {
    Wheel
}