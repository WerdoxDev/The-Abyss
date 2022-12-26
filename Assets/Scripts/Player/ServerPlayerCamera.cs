using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class ServerPlayerCamera : NetworkBehaviour {

    [Header("Settings")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform head;
    private Transform _lastTarget;
    private bool _newInfoConfirmed;

    [HideInInspector] public Transform Target;
    public NetworkVariable<CameraTargetInfo> TargetInfo = new NetworkVariable<CameraTargetInfo>();

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            enabled = false;
            return;
        }
    }

    private void FixedUpdate() {
        Vector3 targetEuler = Target != null ? Target.eulerAngles : Vector3.zero;
        targetEuler.z = 0;

        if (Target != _lastTarget) {
            _newInfoConfirmed = false;
            TargetInfo.Value = new CameraTargetInfo(targetEuler, Target != null, true);
            _lastTarget = Target;
        } else if (Target != null && _newInfoConfirmed)
            TargetInfo.Value = new CameraTargetInfo(targetEuler, false, false);
    }

    public void SetTarget(Transform target) => Target = target;

    [ServerRpc]
    public void SetOrientationServerRpc(Vector2 rotation) {
        orientation.localRotation = Quaternion.Euler(orientation.localEulerAngles.x, rotation.y, orientation.localEulerAngles.z);
        head.localRotation = Quaternion.Euler(rotation.x, head.localEulerAngles.y, head.localEulerAngles.z);
    }

    [ServerRpc]
    public void ConfirmNewInfoServerRpc() => _newInfoConfirmed = true;
}

public struct CameraTargetInfo : INetworkSerializable, IEquatable<CameraTargetInfo> {
    public Vector3 TargetEulerAngles;
    public bool SetOffset;
    public bool ClearOffset;

    public CameraTargetInfo(Vector3 targetEulerAngles, bool setOffset, bool clearOffset) {
        TargetEulerAngles = targetEulerAngles;
        SetOffset = setOffset;
        ClearOffset = clearOffset;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref TargetEulerAngles);
        serializer.SerializeValue(ref SetOffset);
        serializer.SerializeValue(ref ClearOffset);
    }

    public bool Equals(CameraTargetInfo other) {
        return other.TargetEulerAngles == TargetEulerAngles && other.SetOffset == SetOffset && other.ClearOffset == ClearOffset;
    }
}