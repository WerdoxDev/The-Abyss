using UnityEngine;
using Unity.Netcode;

public class Ship : NetworkBehaviour
{
    [Header("Autoset Fields")]
    public Steering Steering;
    public Capstan Capstan;
    public ShipMovement Movement;
    public ShipPlayerAnchor PlayerAnchor;
    public SailControl SailControl;
    public Rigidbody Rb;

    public float Turning { get => Movement.Turning; set => Movement.Turning = value; }
    public float Speed { get => Movement.Speed; set => Movement.Speed = value; }

    public float MaxSpeed { get => SailControl.ShipMaxSpeed; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        PlayerAnchor = GetComponent<ShipPlayerAnchor>();
        Movement = GetComponent<ShipMovement>();
        SailControl = GetComponent<SailControl>();
    }

    public void SetSpawnables(Capstan Capstan, Steering Steering)
    {
        this.Capstan = Capstan;
        this.Steering = Steering;
    }
}
