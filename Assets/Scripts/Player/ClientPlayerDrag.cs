using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ClientPlayerDrag : NetworkBehaviour {
    public InteractHandler Handler { get; private set; }

    private Player _player;
    private Draggable _draggable;
    private Vector3 _initPosition;
    private Vector3 _initNormal;

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
    }

    private void FixedUpdate() {
        if (!_player.SRDragHandler.IsDragging.Value || Handler == null) return;

        if (Handler.Type == InteractType.Wheel) {
            Transform camTransform = _player.CLCamera.Camera.transform;
            if (_player.CLInteract.StopDragIfOutOfRange(_initPosition)) return;
            _draggable.Wheel.CalculateAndSendForce(_initPosition, _initNormal, camTransform.position, camTransform.forward);
        }
    }

    public void StartDrag(Vector3 initPosition, Vector3 initNormal) {
        if (Handler == null) return;

        _draggable.Reset();

        if (Handler.Type == InteractType.Wheel) {
            _draggable.Wheel = (Wheel)Handler.Interactable;
            _draggable.Wheel.SpawnInitial(initPosition, true);
        }

        _initPosition = initPosition;
        _initNormal = initNormal;
    }

    public void StopDrag() {
        if (Handler == null) return;

        if (Handler.Type == InteractType.Wheel) {
            _draggable.Wheel.ResetWheel();
        }

        _draggable.Reset();

        Handler = null;
    }

    public void SetHandler(InteractHandler handler) => Handler = handler;
}
