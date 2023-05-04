using Unity.Netcode;
using UnityEngine;

public class Handle : NetworkBehaviour, Interactable {
    private readonly NetworkVariable<Vector3> HandleRotation = new();

    [Header("Settings")]
    [SerializeField] private Transform handleTransform;
    [SerializeField] private MouseAxis xAxis;
    [SerializeField] private MouseAxis yAxis;
    [SerializeField] private bool invertedX;
    [SerializeField] private bool invertedY;
    [SerializeField] private bool invertedXValue;
    [SerializeField] private bool invertedYValue;
    [SerializeField] private float xAxisMinRotation;
    [SerializeField] private float xAxisMaxRotation;
    [SerializeField] private float yAxisMinRotation;
    [SerializeField] private float yAxisMaxRotation;

    private float _xAxisAngle;
    private float _yAxisAngle;
    private ulong _heldBy;

    public float XValue;
    public float YValue;

    public InteractHandler Handler;

    public override void OnNetworkSpawn() {
        Handler.Interactable = this;

        if (IsServer) return;

        HandleRotation.OnValueChanged += (oldValue, newValue) => {
            handleTransform.localRotation = Quaternion.Euler(newValue);
        };
    }

    public void Interact(Vector2 input) {
        _xAxisAngle += xAxis != MouseAxis.None && !invertedX ? input.x : xAxis != MouseAxis.None && invertedX ? -input.x : 0;
        _yAxisAngle += yAxis != MouseAxis.None && !invertedY ? input.y : yAxis != MouseAxis.None && invertedY ? -input.y : 0;

        _xAxisAngle = Mathf.Clamp(_xAxisAngle, xAxisMinRotation, xAxisMaxRotation);
        _yAxisAngle = Mathf.Clamp(_yAxisAngle, yAxisMinRotation, yAxisMaxRotation);

        Vector3 localEuler = handleTransform.localEulerAngles;
        handleTransform.localRotation = Quaternion.Euler(
            xAxis == MouseAxis.XAxis ? _xAxisAngle : yAxis == MouseAxis.XAxis ? _yAxisAngle : localEuler.x,
            xAxis == MouseAxis.YAxis ? _xAxisAngle : yAxis == MouseAxis.YAxis ? _yAxisAngle : localEuler.y,
            xAxis == MouseAxis.ZAxis ? _xAxisAngle : yAxis == MouseAxis.ZAxis ? _yAxisAngle : localEuler.z
            );
        HandleRotation.Value = handleTransform.localEulerAngles;

        Vector2 value = new(
            Utils.Remap(_xAxisAngle, xAxisMinRotation, xAxisMaxRotation, -1, 1),
            Utils.Remap(_yAxisAngle, yAxisMinRotation, yAxisMaxRotation, -1, 1));

        XValue = invertedXValue ? -value.x : value.x;
        YValue = invertedYValue ? -value.y : value.y;
    }

    public void SetHeldBy(ulong clientId) => _heldBy = clientId;

    public InteractHandler GetHandler(byte handlerData) => Handler;
}

public enum MouseAxis {
    None,
    XAxis,
    YAxis,
    ZAxis
}