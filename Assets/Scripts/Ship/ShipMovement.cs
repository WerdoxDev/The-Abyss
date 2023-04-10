using UnityEngine;
using Unity.Netcode;

public class ShipMovement : NetworkBehaviour {
    [Header("Movement")]
    [SerializeField] private AnimationCurve torqueCurve;
    [SerializeField] private float suddenTorqueMultiplier;
    [SerializeField] private float suddenTorqueLerpSpeed;
    private bool _applySuddenTorque;

    public float Speed;
    public float Turning;

    private Ship _ship;
    private Rigidbody _rb;

    private Vector3 _suddenTorque;

    private void Awake() {
        _ship = GetComponent<Ship>();
    }

    public override void OnNetworkSpawn() {
        _rb = _ship.Rb;

        if (!IsServer) {
            enabled = false;
            _rb.isKinematic = true;

            return;
        }
    }

    private void FixedUpdate() {
    }

    public void AddSuddenTorque() {
        //_applySuddenTorque = true;
        //_suddenTorque = (transform.up * Turning) * (_rb.velocity.magnitude / _ship.MaxSpeed) * suddenTorqueMultiplier;
    }
}
