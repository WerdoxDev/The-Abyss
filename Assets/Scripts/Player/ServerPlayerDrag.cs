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
    private Vector3 _initPosition;
    private Vector3 _initNormal;

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

        //if (Handler.Type == InteractType.Wheel) {
        //    Transform camTransform = _player.CLCamera.Camera.transform;
        //    //Quaternion camRotation = Quaternion.Euler(cam, _player.SRCamera.OrientationRotation.Value.y, 0);
        //    _draggable.Wheel.Drag(_initPosition, _initNormal, camTransform.position, camTransform.forward);
        //}
    }

    public void StartDrag(Vector3 initPosition, Vector3 initNormal) {
        if (Handler == null) return;

        _draggable.Reset();

        if (Handler.Type == InteractType.Wheel) {
            _draggable.Wheel = (Wheel)Handler.Interactable;
            _draggable.Wheel.SpawnInitial(initPosition, false);
            _draggable.Wheel.SetHeldBy(_player.OwnerClientId);
        }

        _initPosition = initPosition;
        _initNormal = initNormal;

        Handler.Occupied.Value = true;
        IsDragging.Value = true;
    }

    public void StopDrag() {
        if (Handler == null) return;

        //if (Handler.Type == InteractType.Wheel) {
        //    _draggable.Wheel.ResetWheel();
        //}

        _draggable.Reset();

        Handler.Occupied.Value = false;
        Handler = null;
        IsDragging.Value = false;
    }

    public void SetHandler(InteractHandler handler) => Handler = handler;
}

public struct Draggable {
    public Wheel Wheel;

    public void Reset() {
        Wheel = null;
    }
}
