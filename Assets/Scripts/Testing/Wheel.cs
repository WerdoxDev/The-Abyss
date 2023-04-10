using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Wheel : NetworkBehaviour, Interactable {
    private readonly NetworkVariable<float> WheelRotation = new();

    [SerializeField] private float dragForceMultiplier;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private LineRenderer lineRenderer;

    private Transform _initTransform;
    private Transform _indicatorTransform;
    private Rigidbody _rb;
    private Vector3 _dragForce;

    private ulong _heldBy;

    public InteractHandler Handler;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn() {
        Handler.Interactable = this;

        if (!IsServer) {
            enabled = false;

            WheelRotation.OnValueChanged += (float newValue, float oldValue) => {
                transform.rotation = Quaternion.Euler(0, 0, newValue);
            };
        }
    }

    private void FixedUpdate() {
        WheelRotation.Value = transform.eulerAngles.z;

        if (_dragForce == Vector3.zero || _initTransform == null) return;

        _rb.AddForceAtPosition(_dragForce, _initTransform.position, ForceMode.Force);
    }

    public void CalculateAndSendForce(Vector3 initPosition, Vector3 initNormal, Vector3 cameraPosition, Vector3 cameraForward) {
        if (_initTransform == null) return;

        Ray ray = new(cameraPosition, cameraForward);
        if (new Plane(initNormal, initPosition).Raycast(ray, out float enter)) {
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector3 force = (hitPoint - _initTransform.position).normalized * dragForceMultiplier;
            SetDragForceServerRpc(force);

            lineRenderer.SetPositions(new[] { _initTransform.position, hitPoint });
        }
    }

    public void SpawnInitial(Vector3 initPosition, bool spawnIndicator) {
        if (_initTransform == null) {
            _initTransform = new GameObject("Initial").transform;
            _initTransform.parent = transform;
            _initTransform.position = initPosition;
        }

        if (!spawnIndicator || _indicatorTransform != null) return;
        _indicatorTransform = Instantiate(indicatorPrefab, _initTransform.position, Quaternion.identity, _initTransform).transform;

        lineRenderer.enabled = true;
        lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
    }

    public void SetHeldBy(ulong clientId) => _heldBy = clientId;

    public void ResetWheel() {
        if (_initTransform == null) return;

        Destroy(_initTransform.gameObject);

        _initTransform = null;
        _indicatorTransform = null;
        lineRenderer.enabled = false;

        if (IsServer) return;
        ResetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDragForceServerRpc(Vector3 force) => _dragForce = force;

    [ServerRpc(RequireOwnership = false)]
    public void ResetServerRpc() => ResetWheel();

    private void OnDrawGizmos() {
        if (_initTransform == null) return;
        Gizmos.DrawSphere(_initTransform.position, 0.1f);
    }

    public InteractHandler GetHandler(byte handlerData) => Handler;
}
