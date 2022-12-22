using UnityEngine;
using Unity.Netcode;

public class ShipMovement : NetworkBehaviour
{
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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        _ship = GetComponent<Ship>();
        _rb = _ship.Rb;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (_applySuddenTorque && _ship.Capstan != null && _ship.Capstan.IsDown.Value)
        {
            _rb.AddTorque(_suddenTorque, ForceMode.Acceleration);
            _suddenTorque = Vector3.Lerp(_suddenTorque, Vector3.zero, suddenTorqueLerpSpeed * Time.fixedDeltaTime);
            if (Vector3.Distance(_suddenTorque, Vector3.zero) < 0.1f)
            {
                _suddenTorque = Vector3.zero;
                _applySuddenTorque = false;
            }
        }

        if (_ship.Capstan == null || _ship.Capstan.IsDown.Value) return;

        _rb.AddForce(transform.forward * Speed, ForceMode.Acceleration);
        Vector3 torque = transform.up * Turning * torqueCurve.Evaluate(Speed / _ship.MaxSpeed);
        _rb.AddTorque(torque, ForceMode.Acceleration);
    }

    public void AddSuddenTorque()
    {
        _applySuddenTorque = true;
        _suddenTorque = (transform.up * Turning) * (_rb.velocity.magnitude / _ship.MaxSpeed) * suddenTorqueMultiplier;
    }
}
