using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;

public class ClientPlayerAttachable : NetworkBehaviour {
    [Header("Settings")]
    [SerializeField] private AttachableSetting[] attachableSettings;

    private InputReader _inputReader;
    private Player _player;
    private Vector2 _lastSentInput;

    private Action OnTargetArrived;

    private ServerPlayerAttachable _server;

    private void Awake() {
        _server = GetComponent<ServerPlayerAttachable>();
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) {
            enabled = false;
            return;
        }

        _inputReader = _player.InputReader;
        SetInputState(true);

        _player.OnRotationTargetChanged += () => {
            OnTargetArrived?.Invoke();
            OnTargetArrived = null;
        };
    }

    public override void OnDestroy() => SetInputState(false);

    [ClientRpc]
    public void SetAttachableSettingsClientRpc(ulong interactObjId, bool isAttaching, InteractType type, byte handlerData, ClientRpcParams rpcParams = default) {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(interactObjId, out var interactObj);
        if (interactObj == null) return;

        InteractHandler handler;
        handler = interactObj.GetComponent<Interactable>().GetHandler(handlerData);

        AttachableSetting settings = GetAttachableSetting(type, handler.Data);

        OnTargetArrived = () => {
            if (settings == null) return;
            if (isAttaching) {
                // _player.SetRotationTarget(handler.StandTransform);
                if (settings.setDefaultRotation) _player.CLCamera.SetRotation(settings.defaultRotation);
                _player.CLCamera.SetClamp(settings.xClamp, settings.yClamp);
                return;
            }

            if (settings.resetRotationTarget) _player.CLCamera.SetClamp(0, 0, 0, 0);
        };
    }

    public AttachableSetting GetAttachableSetting(InteractType type, byte handlerData) {
        AttachableSettingType attachableType = AttachableSettingType.None;
        if (type == InteractType.Capstan) attachableType = AttachableSettingType.Capstan;
        if (type == InteractType.Steering) attachableType = AttachableSettingType.Steering;
        if (type == InteractType.Ladder) attachableType = AttachableSettingType.Ladder;
        if (type == InteractType.SailControl) attachableType = AttachableSettingType.SailControl;
        return attachableSettings.FirstOrDefault(x => x.type == attachableType);
    }

    private void SetInputState(bool enabled) {
        if (!IsOwner) return;

        void OnMove(Vector2 direction) {
            if (!_player.CanUseAttachable) {
                _server.SetMovementInputServerRpc(Vector2.zero);

                _lastSentInput = direction;
                return;
            }

            if (direction != _lastSentInput) {
                if (_server.IsAttached.Value)
                    _server.SetMovementInputServerRpc(direction);
                else
                    _server.SetMovementInputServerRpc(Vector2.zero);

                _lastSentInput = direction;
            }
        }

        if (enabled) _inputReader.MoveEvent += OnMove;
        else _inputReader.MoveEvent -= OnMove;
    }
}

[System.Serializable]
public class AttachableSetting {
    public AttachableSettingType type;

    public Vector2 xClamp;
    public Vector2 yClamp;
    public Vector2 defaultRotation;

    public bool setDefaultRotation;
    public bool resetRotationTarget;
    public bool resetRotation;
}

public enum AttachableSettingType {
    None,
    Capstan,
    Steering,
    Ladder,
    SailControl
}