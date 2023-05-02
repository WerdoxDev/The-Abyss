using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ClientPlayerDrag : NetworkBehaviour {
    public InteractHandler Handler { get; private set; }

    private Player _player;
    private Draggable _draggable;
    private InputReader _inputReader;
    private Vector2 _lastSentInput;

    private float _timer;

    private ServerPlayerDrag _server;

    private void Awake() {
        _server = GetComponent<ServerPlayerDrag>();
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) {
            enabled = false;
            return;
        }

        _inputReader = _player.InputReader;
        SetInputState(true);
    }

    public void StartDrag() {
        if (Handler == null) return;

        _draggable.Reset();

        if (Handler.Type == InteractType.Handle) _draggable.Handle = (Handle)Handler.Interactable;

        _player.CanLook = false;
    }

    public void StopDrag() {
        if (Handler == null) return;

        _draggable.Reset();

        _player.CanLook = true;
        Handler = null;
    }

    public void SetHandler(InteractHandler handler) => Handler = handler;

    private void SetInputState(bool enabled) {
        if (!IsOwner) return;

        void OnLook(Vector2 rotation) {
            if (rotation == _lastSentInput) return;

            if (Handler != null && Handler.Occupied.Value) _server.SetMouseInputServerRpc(rotation);
            else _server.SetMouseInputServerRpc(Vector2.zero);

            _lastSentInput = rotation;
        }

        if (enabled) _inputReader.LookEvent += OnLook;
        else _inputReader.LookEvent -= OnLook;
    }
}
