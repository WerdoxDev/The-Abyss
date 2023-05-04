using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class ServerPlayerDrag : NetworkBehaviour {
    public NetworkVariable<bool> IsDragging = new(false);

    public InteractHandler Handler { get; private set; }

    private Player _player;
    private Draggable _draggable;
    private Vector2 _mouseInput;

    private ClientPlayerDrag _client;

    private void Awake() {
        _client = GetComponent<ClientPlayerDrag>();
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            enabled = false;
            return;
        }
    }

    private void FixedUpdate() {
        if (!IsDragging.Value) return;

        if (Handler.Type == InteractType.Handle) {
            _draggable.Handle.Interact(_mouseInput);
        }
    }

    public void StartDrag() {
        if (Handler == null) return;

        _draggable.Reset();

        if (Handler.Type == InteractType.Handle) {
            _draggable.Handle = (Handle)Handler.Interactable;
            _draggable.Handle.SetHeldBy(_player.OwnerClientId);
        }

        Handler.Occupied.Value = true;
        IsDragging.Value = true;
    }

    public void StopDrag() {
        if (Handler == null) return;

        _draggable.Reset();

        Handler.Occupied.Value = false;
        Handler = null;
        IsDragging.Value = false;
    }

    public void SetHandler(InteractHandler handler) => Handler = handler;

    [ServerRpc]
    public void SetMouseInputServerRpc(Vector2 input) => _mouseInput = input;
}

public struct Draggable {
    public Handle Handle;

    public void Reset() {
        Handle = null;
    }
}