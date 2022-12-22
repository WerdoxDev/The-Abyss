using UnityEngine;
using Unity.Netcode;

public class SailControl : NetworkBehaviour
{
    public NetworkVariable<float> SailPercentage = new NetworkVariable<float>();

    [Header("Sail Control")]
    [SerializeField] private Transform sailTop;
    [SerializeField] private GameObject[] sails;
    [SerializeField] private float speedLerpMultiplier;
    [SerializeField] private float sailUpSpeed;
    [SerializeField] private float sailDownSpeed;
    private float _desiredSpeed;
    private float _perSectionPercentage;

    public float ShipMaxSpeed;

    private Ship _ship;

    public override void OnNetworkSpawn()
    {
        _ship = GetComponent<Ship>();
        _perSectionPercentage = 100 / sails.Length;
    }

    private void Update()
    {
        int sailIndex = Mathf.CeilToInt(SailPercentage.Value / _perSectionPercentage);

        sailIndex = Mathf.Clamp(sailIndex, 1, sails.Length);
        for (int i = 0; i < sails.Length; i++) sails[i].SetActive(i == sailIndex - 1 ? true : false);

        if (!IsHost) return;

        if (_ship.Capstan != null && _ship.Capstan.IsDown.Value) return;
        _ship.Speed = Mathf.Lerp(_ship.Speed, _desiredSpeed, Time.deltaTime * speedLerpMultiplier);
        if (Mathf.Abs(_ship.Speed - _desiredSpeed) <= 0.1f) _ship.Speed = _desiredSpeed;
    }

    public void SetSails(Vector2 input, bool inverted = false)
    {
        if (input.y < 0) SailPercentage.Value += Mathf.Abs(input.y) * sailDownSpeed * Time.fixedDeltaTime;
        else if (input.y > 0) SailPercentage.Value -= input.y * sailUpSpeed * Time.fixedDeltaTime;

        SailPercentage.Value = Mathf.Clamp(SailPercentage.Value, 0, 100);
        _desiredSpeed = (ShipMaxSpeed / 100) * SailPercentage.Value;
    }
}
