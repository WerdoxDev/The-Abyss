using UnityEngine;
using Unity.Netcode;

public class Capstan : NetworkBehaviour, Interactable
{
    public NetworkVariable<bool> IsDown = new NetworkVariable<bool>(true);

    [Header("Capstan")]
    [SerializeField] private int maxRotationAngle;
    [SerializeField] private float raisingSpeed;
    [SerializeField] private float loweringSpeed;
    [SerializeField] private float loweringDuration;
    private bool _lowering;
    private float _rotation;

    public int PlayerNum;

    [Header("Indicator")]
    [SerializeField] private float indicatorOffset;
    [SerializeField] private float indicatorMaxY;

    [Header("Settings")]
    [SerializeField] private Transform mesh;
    [SerializeField] private Transform indicator;
    private Ship _ship;

    public InteractHandler LeftInteract;
    public InteractHandler RightInteract;
    public InteractHandler TopInteract;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        TopInteract.interactable = this;
        LeftInteract.interactable = this;
        RightInteract.interactable = this;
    }

    // Because this object is spawned and then parented to ship
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject) => _ship = GetComponentInParent<Ship>();

    private void FixedUpdate()
    {
        if (!IsSpawned) return;

        if (IsDown.Value == true)
        {
            TopInteract.Active = false;
            TopInteract.gameObject.SetActive(false);
            LeftInteract.Active = !LeftInteract.Occupied.Value;
            RightInteract.Active = !RightInteract.Occupied.Value;
        }
        else
        {
            TopInteract.Active = true;
            TopInteract.gameObject.SetActive(true);
            LeftInteract.Active = false;
            RightInteract.Active = false;
        }

        if (!IsServer) return;

        if (_lowering || (PlayerNum == 0 && IsDown.Value)) _rotation += (loweringSpeed) * Time.fixedDeltaTime;

        _rotation = Mathf.Clamp(_rotation, -maxRotationAngle, 0);

        Vector3 localRotation = mesh.localEulerAngles;
        localRotation.y = _rotation;
        mesh.localEulerAngles = localRotation;
        indicator.localPosition = new Vector3(0, Mathf.Abs((indicatorMaxY / maxRotationAngle) * _rotation) + indicatorOffset, 0);

        if (_rotation == 0 && _lowering)
        {
            IsDown.Value = true;
            _lowering = false;
            _ship.Movement.AddSuddenTorque();
            _ship.Speed = 0;
        }
        else if (Mathf.Abs(_rotation) == 360)
            IsDown.Value = false;
    }

    public void Raise(float input, bool inverted = false)
    {
        _lowering = false;
        float amountToRotate = (inverted ? input : -input) * raisingSpeed * PlayerNum * Time.fixedDeltaTime;

        _rotation += amountToRotate;
    }

    public void Lower() => _lowering = true;

    public InteractHandler GetHandler(byte handlerData)
    {
        if (TopInteract.Data == handlerData) return TopInteract;
        if (LeftInteract.Data == handlerData) return LeftInteract;
        if (RightInteract.Data == handlerData) return RightInteract;
        return null;
    }
}
