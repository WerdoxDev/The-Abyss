using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class ServerTransform : NetworkBehaviour {

    public NetworkVariable<float> PositionX;
    public NetworkVariable<float> PositionY;
    public NetworkVariable<float> PositionZ;

    public NetworkVariable<float> RotationX;
    public NetworkVariable<float> RotationY;
    public NetworkVariable<float> RotationZ;

    public NetworkVariable<BetterSyncInfo> BetterSyncInfo;

    public UpdateCycle cycle;

    public bool SyncPositionX;
    public bool SyncPositionY;
    public bool SyncPositionZ;

    public bool SyncRotationX;
    public bool SyncRotationY;
    public bool SyncRotationZ;

    public float PositionThreshold = 0.001f;
    public float RotationThreshold = 0.01f;

    [Tooltip("Syncs position and rotation at the same time but increases payload size")]
    public bool IsBetterSync;

    public bool IsLocalSpace;

    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    private void Awake() {
        if (SyncPositionX) PositionX = new NetworkVariable<float>();
        if (SyncPositionY) PositionY = new NetworkVariable<float>();
        if (SyncPositionZ) PositionZ = new NetworkVariable<float>();
        if (SyncRotationX) RotationX = new NetworkVariable<float>();
        if (SyncRotationY) RotationY = new NetworkVariable<float>();
        if (SyncRotationZ) RotationZ = new NetworkVariable<float>();
    }

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            enabled = false;

            PositionX.OnValueChanged += (float oldX, float newX) => SetPosition(newX, null, null);
            PositionY.OnValueChanged += (float oldY, float newY) => SetPosition(null, newY, null);
            PositionZ.OnValueChanged += (float oldZ, float newZ) => SetPosition(null, null, newZ);

            RotationX.OnValueChanged += (float oldX, float newX) => SetRotation(newX, null, null);
            RotationY.OnValueChanged += (float oldY, float newY) => SetRotation(null, newY, null);
            RotationZ.OnValueChanged += (float oldZ, float newZ) => SetRotation(null, null, newZ);

            BetterSyncInfo.OnValueChanged += (BetterSyncInfo oldInfo, BetterSyncInfo newInfo) => {
                if (IsLocalSpace) {
                    transform.localPosition = newInfo.Position;
                    transform.localRotation = Quaternion.Euler(newInfo.Rotation);
                    return;
                }
                transform.position = newInfo.Position;
                transform.rotation = Quaternion.Euler(newInfo.Rotation);
            };
        }
    }

    private void Update() {
        if (cycle == UpdateCycle.Update) Sync();
    }

    private void FixedUpdate() {
        if (cycle == UpdateCycle.FixedUpdate) Sync();
    }

    public void Sync() {
        Vector3 position = IsLocalSpace ? transform.localPosition : transform.position;
        Vector3 rotation = IsLocalSpace ? transform.localEulerAngles : transform.eulerAngles;

        if (IsBetterSync) {
            if (!ShouldUpdatePosition(position, true, true, true) && !ShouldUpdateRotation(rotation, true, true, true)) return;
            // Debug.Log($"Pos {position}, {_lastPosition}, {position == _lastPosition}");
            // Debug.Log($"Rot {rotation}, {_lastRotation}, {rotation == _lastRotation}");
            BetterSyncInfo.Value = new BetterSyncInfo(position, rotation);
            return;
        }

        if (ShouldUpdatePosition(position, SyncPositionX, SyncPositionY, SyncPositionZ)) {
            if (SyncPositionX) PositionX.Value = position.x;
            if (SyncPositionY) PositionY.Value = position.y;
            if (SyncPositionZ) PositionZ.Value = position.z;
        }

        if (ShouldUpdateRotation(rotation, SyncRotationX, SyncRotationY, SyncRotationZ)) {
            if (SyncRotationX) RotationX.Value = rotation.x;
            if (SyncRotationY) RotationY.Value = rotation.y;
            if (SyncRotationZ) RotationZ.Value = rotation.z;
        }
    }

    private void SetPosition(float? x, float? y, float? z) {
        Vector3 position = IsLocalSpace ? transform.localPosition : transform.position;
        if (x != null) position.x = x ?? 0;
        if (y != null) position.y = y ?? 0;
        if (z != null) position.z = z ?? 0;
        if (IsLocalSpace) transform.localPosition = position;
        else transform.position = position;
    }

    private void SetRotation(float? x, float? y, float? z) {
        Vector3 rotation = IsLocalSpace ? transform.localEulerAngles : transform.eulerAngles;
        if (x != null) rotation.x = x ?? 0;
        if (y != null) rotation.y = y ?? 0;
        if (z != null) rotation.z = z ?? 0;
        if (IsLocalSpace) transform.localRotation = Quaternion.Euler(rotation);
        else transform.rotation = Quaternion.Euler(rotation);
    }

    private bool ShouldUpdatePosition(Vector3 position, bool syncX, bool syncY, bool syncZ) {
        bool shouldUpdate = false;
        if (syncX && Mathf.Abs(position.x - _lastPosition.x) >= PositionThreshold) shouldUpdate = true;
        if (syncY && Mathf.Abs(position.y - _lastPosition.y) >= PositionThreshold) shouldUpdate = true;
        if (syncZ && Mathf.Abs(position.z - _lastPosition.z) >= PositionThreshold) shouldUpdate = true;
        if (shouldUpdate) _lastPosition = position;
        return shouldUpdate;
    }

    private bool ShouldUpdateRotation(Vector3 rotation, bool syncX, bool syncY, bool syncZ) {
        bool shouldUpdate = false;
        Vector3 lastEuler = _lastRotation.eulerAngles;
        if (syncX && Mathf.Abs(Mathf.DeltaAngle(rotation.x, lastEuler.x)) >= RotationThreshold) shouldUpdate = true;
        if (syncY && Mathf.Abs(Mathf.DeltaAngle(rotation.y, lastEuler.y)) >= RotationThreshold) shouldUpdate = true;
        if (syncZ && Mathf.Abs(Mathf.DeltaAngle(rotation.z, lastEuler.z)) >= RotationThreshold) shouldUpdate = true;
        if (shouldUpdate) _lastRotation = Quaternion.Euler(rotation);
        return shouldUpdate;
    }
}

public struct BetterSyncInfo : INetworkSerializable {
    public Vector3 Position;
    public Vector3 Rotation;

    public BetterSyncInfo(Vector3 position, Vector3 rotation) {
        Position = position;
        Rotation = rotation;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Rotation);
    }
}

public enum UpdateCycle {
    FixedUpdate,
    Update
}