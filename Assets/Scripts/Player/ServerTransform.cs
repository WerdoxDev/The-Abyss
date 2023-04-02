using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.UIElements;

public class ServerTransform : NetworkBehaviour {

    [HideInInspector] public NetworkVariable<float> PositionX;
    [HideInInspector] public NetworkVariable<float> PositionY;
    [HideInInspector] public NetworkVariable<float> PositionZ;

    [HideInInspector] public NetworkVariable<float> RotationX;
    [HideInInspector] public NetworkVariable<float> RotationY;
    [HideInInspector] public NetworkVariable<float> RotationZ;

    [HideInInspector] public NetworkVariable<BetterSyncInfo> BetterSyncInfo;

    public NetworkVariable<LerpInfo> LerpInfo;

    [Header("Settings")]
    [SerializeField] private UpdateCycle cycle;

    [SerializeField] private bool syncPositionX;
    [SerializeField] private bool syncPositionY;
    [SerializeField] private bool syncPositionZ;

    [SerializeField] private bool syncRotationX;
    [SerializeField] private bool syncRotationY;
    [SerializeField] private bool syncRotationZ;

    [SerializeField] private float positionThreshold = 0.001f;
    [SerializeField] private float rotationThreshold = 0.01f;
    [SerializeField] private float lerpMultiplier;

    [Tooltip("Syncs position and rotation at the same time but increases payload size")]
    [SerializeField] private bool isBetterSync;

    [SerializeField] private bool isLocalSpace;

    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    private void Awake() {
        if (syncPositionX) PositionX = new NetworkVariable<float>();
        if (syncPositionY) PositionY = new NetworkVariable<float>();
        if (syncPositionZ) PositionZ = new NetworkVariable<float>();
        if (syncRotationX) RotationX = new NetworkVariable<float>();
        if (syncRotationY) RotationY = new NetworkVariable<float>();
        if (syncRotationZ) RotationZ = new NetworkVariable<float>();
    }

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            PositionX.OnValueChanged += (float oldX, float newX) => SetPosition(newX, null, null);
            PositionY.OnValueChanged += (float oldY, float newY) => SetPosition(null, newY, null);
            PositionZ.OnValueChanged += (float oldZ, float newZ) => SetPosition(null, null, newZ);

            RotationX.OnValueChanged += (float oldX, float newX) => SetRotation(newX, null, null);
            RotationY.OnValueChanged += (float oldY, float newY) => SetRotation(null, newY, null);
            RotationZ.OnValueChanged += (float oldZ, float newZ) => SetRotation(null, null, newZ);

            BetterSyncInfo.OnValueChanged += (BetterSyncInfo oldInfo, BetterSyncInfo newInfo) => {
                if (isLocalSpace) {
                    transform.SetLocalPositionAndRotation(newInfo.Position, Quaternion.Euler(newInfo.Rotation));
                    return;
                }
                transform.SetPositionAndRotation(newInfo.Position, Quaternion.Euler(newInfo.Rotation));
            };
        }
    }

    private void Update() {
        if (cycle != UpdateCycle.Update) return;
        if (IsServer) Sync();
        LerpPositionAndRotation(Time.deltaTime);
    }

    private void FixedUpdate() {
        if (cycle != UpdateCycle.FixedUpdate) return;
        if (IsServer) Sync();
        LerpPositionAndRotation(Time.fixedDeltaTime);
    }

    public void SetLerpInfo(bool lerpPosition, bool lerpRotation) {
        LerpInfo.Value = new LerpInfo(lerpPosition, lerpRotation);
    }

    private void LerpPositionAndRotation(float time) {
        if (!LerpInfo.Value.LerpPosition && !LerpInfo.Value.LerpRotation) return;

        Vector3 newPosition = isBetterSync
            ? BetterSyncInfo.Value.Position
            : new(PositionX.Value, PositionY.Value, PositionZ.Value);

        Quaternion newRotation = isBetterSync
            ? Quaternion.Euler(BetterSyncInfo.Value.Rotation)
            : Quaternion.Euler(RotationX.Value, RotationY.Value, RotationZ.Value);

        if (isLocalSpace) {
            if (LerpInfo.Value.LerpPosition) transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, time * lerpMultiplier);
            if (LerpInfo.Value.LerpRotation) transform.localRotation = Quaternion.Lerp(transform.localRotation, newRotation, time * lerpMultiplier);
        }
        else {
            if (LerpInfo.Value.LerpPosition) transform.position = Vector3.Lerp(transform.position, newPosition, time * lerpMultiplier);
            if (LerpInfo.Value.LerpRotation) transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, time * lerpMultiplier);
        }
    }

    private void Sync() {
        Vector3 position = isLocalSpace ? transform.localPosition : transform.position;
        Vector3 rotation = isLocalSpace ? transform.localEulerAngles : transform.eulerAngles;

        if (isBetterSync) {
            if (!ShouldUpdatePosition(position, true, true, true) && !ShouldUpdateRotation(rotation, true, true, true)) return;
            // Debug.Log($"Pos {position}, {_lastPosition}, {position == _lastPosition}");
            // Debug.Log($"Rot {rotation}, {_lastRotation}, {rotation == _lastRotation}");
            BetterSyncInfo.Value = new BetterSyncInfo(position, rotation);
            return;
        }

        if (ShouldUpdatePosition(position, syncPositionX, syncPositionY, syncPositionZ)) {
            if (syncPositionX) PositionX.Value = position.x;
            if (syncPositionY) PositionY.Value = position.y;
            if (syncPositionZ) PositionZ.Value = position.z;
        }

        if (ShouldUpdateRotation(rotation, syncRotationX, syncRotationY, syncRotationZ)) {
            if (syncRotationX) RotationX.Value = rotation.x;
            if (syncRotationY) RotationY.Value = rotation.y;
            if (syncRotationZ) RotationZ.Value = rotation.z;
        }
    }

    private void SetPosition(float? x, float? y, float? z) {
        if (LerpInfo.Value.LerpPosition) return;
        Vector3 position = isLocalSpace ? transform.localPosition : transform.position;
        if (x != null) position.x = x ?? 0;
        if (y != null) position.y = y ?? 0;
        if (z != null) position.z = z ?? 0;
        if (isLocalSpace) transform.localPosition = position;
        else transform.position = position;
    }

    private void SetRotation(float? x, float? y, float? z) {
        if (LerpInfo.Value.LerpRotation) return;
        Vector3 rotation = isLocalSpace ? transform.localEulerAngles : transform.eulerAngles;
        if (x != null) rotation.x = x ?? 0;
        if (y != null) rotation.y = y ?? 0;
        if (z != null) rotation.z = z ?? 0;
        if (isLocalSpace) transform.localRotation = Quaternion.Euler(rotation);
        else transform.rotation = Quaternion.Euler(rotation);
    }

    private bool ShouldUpdatePosition(Vector3 position, bool syncX, bool syncY, bool syncZ) {
        bool shouldUpdate = false;
        if (syncX && Mathf.Abs(position.x - _lastPosition.x) >= positionThreshold) shouldUpdate = true;
        if (syncY && Mathf.Abs(position.y - _lastPosition.y) >= positionThreshold) shouldUpdate = true;
        if (syncZ && Mathf.Abs(position.z - _lastPosition.z) >= positionThreshold) shouldUpdate = true;
        if (shouldUpdate) _lastPosition = position;
        return shouldUpdate;
    }

    private bool ShouldUpdateRotation(Vector3 rotation, bool syncX, bool syncY, bool syncZ) {
        bool shouldUpdate = false;
        Vector3 lastEuler = _lastRotation.eulerAngles;
        if (syncX && Mathf.Abs(Mathf.DeltaAngle(rotation.x, lastEuler.x)) >= rotationThreshold) shouldUpdate = true;
        if (syncY && Mathf.Abs(Mathf.DeltaAngle(rotation.y, lastEuler.y)) >= rotationThreshold) shouldUpdate = true;
        if (syncZ && Mathf.Abs(Mathf.DeltaAngle(rotation.z, lastEuler.z)) >= rotationThreshold) shouldUpdate = true;
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

public struct LerpInfo : INetworkSerializable {
    public bool LerpPosition;
    public bool LerpRotation;

    public LerpInfo(bool lerpPosition, bool lerpRotation) {
        LerpPosition = lerpPosition;
        LerpRotation = lerpRotation;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref LerpPosition);
        serializer.SerializeValue(ref LerpRotation);
    }
}

public enum UpdateCycle {
    FixedUpdate,
    Update
}