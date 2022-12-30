using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientPlayerMovement : NetworkBehaviour {
    private InputReader _inputReader;
    private Player _player;
    private Vector2 _lastSentInput;

    private ServerPlayerMovement _server;

    private void Awake() {
        _player = GetComponent<Player>();
        _server = GetComponent<ServerPlayerMovement>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) {
            enabled = false;
            return;
        }

        _inputReader = _player.InputReader;
        SetInputState(true);
    }

    public override void OnDestroy() => SetInputState(false);

    private void SetInputState(bool enabled) {
        if (!IsOwner) return;

        void OnMove(Vector2 direction) {
            if (!_player.CanMove) {
                _server.SetMovementInputServerRpc(Vector2.zero);
                _lastSentInput = direction;
                return;
            }

            if (direction != _lastSentInput) {
                if (!_player.Attachable.IsAttached.Value)
                    _server.SetMovementInputServerRpc(direction);
                else
                    _server.SetMovementInputServerRpc(Vector2.zero);

                _lastSentInput = direction;
            }
        }
        void OnButtonEvent(ButtonType type, bool performed) {
            if (performed) {
                if (type == ButtonType.Jump && _player.CanJump) _server.SetJumpingStateServerRpc(true);
                else if (type == ButtonType.Sprint && _player.CanSprint) _server.SetSprintingStateServerRpc(!_server.IsSprinting.Value);
            } else {
                if (type == ButtonType.Jump && _player.CanJump) _server.SetJumpingStateServerRpc(false);
            }
        }

        if (enabled) {
            _inputReader.MoveEvent += OnMove;
            _inputReader.ButtonEvent += OnButtonEvent;
        } else {
            _inputReader.MoveEvent -= OnMove;
            _inputReader.ButtonEvent -= OnButtonEvent;
        }
    }
}
