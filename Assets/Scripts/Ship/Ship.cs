using UnityEngine;
using Unity.Netcode;

public class Ship : NetworkBehaviour {
    [Header("Autoset Fields")]
    public ShipMovement Movement;
    public ShipPlayerAnchor PlayerAnchor;
    public Rigidbody Rb;

    private void Awake() {
        Rb = GetComponent<Rigidbody>();
        PlayerAnchor = GetComponent<ShipPlayerAnchor>();
        Movement = GetComponent<ShipMovement>();
    }

    public void SetSpawnables() {        
    }
}
