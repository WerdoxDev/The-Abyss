using UnityEngine;
using Unity.Netcode;

public class Steering : NetworkBehaviour, Interactable
{
    [Header("Turning")]
    [SerializeField] private float turnSpeed;
    [SerializeField] private float maxShipTurn;
    [SerializeField] private int maxRotationAngle;
    [SerializeField] private int angleThreshold;
    private float _rotation;

    [Header("Settings")]
    [SerializeField] private Transform mesh;
    private Ship _ship;

    public InteractHandler interact;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        interact.interactable = this;
    }

    // Because this object is spawned and then parented to ship
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject) => _ship = GetComponentInParent<Ship>();

    private void FixedUpdate()
    {
        if (!IsServer || !IsSpawned || _ship == null) return;

        _rotation = Mathf.Clamp(_rotation, -maxRotationAngle, maxRotationAngle);

        Vector3 localRotation = mesh.localEulerAngles;
        localRotation.z = _rotation;
        mesh.localEulerAngles = localRotation;

        if (Mathf.Abs(_rotation) > angleThreshold)
            _ship.Turning = (maxShipTurn / maxRotationAngle) * -_rotation;
        else
            _ship.Turning = 0;
    }

    public void Turn(float input, bool inverted = false)
    {
        float amountToRotate = (inverted ? input : -input) * turnSpeed * Time.fixedDeltaTime;

        _rotation += amountToRotate;
    }

    public InteractHandler GetHandler(byte handlerData) => interact;
}
