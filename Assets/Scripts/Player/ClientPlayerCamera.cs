using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class ClientPlayerCamera : NetworkBehaviour {
    [Header("Camera")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] private float normalFOV;
    [SerializeField] private float sprintFOV;
    [SerializeField] private float fovChangeDuration;
    private bool _lastSprinting;
    private float _lastCameraRotation;

    private Player _player;
    private InputReader _inputReader;

    public Camera Camera;
    public bool CanSetTarget = true;

    private Vector2 _rotationInput;
    private Vector2 _rotation;
    private Vector3 _eulerAngles;
    private Vector3? _lastEulerAngles;
    private Vector2 _xClamp;
    private Vector2 _yClamp;

    private ServerPlayerCamera _server;

    private void Awake() {
        _server = GetComponent<ServerPlayerCamera>();
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) {
            enabled = false;
            Camera.gameObject.GetComponent<AudioListener>().enabled = false;
            Camera.gameObject.SetActive(false);
            return;
        }

        _inputReader = _player.InputReader;

        SetInputState(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _server.TargetInfo.OnValueChanged += (CameraTargetInfo oldInfo, CameraTargetInfo newInfo) => {
            if (CanSetTarget) _eulerAngles = newInfo.TargetEulerAngles;

            if (newInfo.ClearOffset) ClearOffset();
            else _lastEulerAngles = _eulerAngles;

            if (newInfo.SetOffset) SetOffset();

            if (newInfo.ClearOffset || newInfo.SetOffset) _server.ConfirmNewInfoServerRpc();

            _player.RotationTargetChanged();

            if (CanSetTarget && !IsServer) RotateCamera(_eulerAngles);
            // _callLook = false;
        };
    }

    public override void OnDestroy() => SetInputState(false);

    private void Update() {
        HandleFOV();
    }

    private void FixedUpdate() {
        Look();
    }

    public void SetOffset() {
        float x = _eulerAngles.x;
        if (x > 180) x -= 360;

        _rotation.y -= _eulerAngles.y;
        _rotation.x -= x;
    }

    public void ClearOffset() {
        if (_lastEulerAngles == null) return;

        float x = _lastEulerAngles?.x ?? 0;
        if (x > 180) x -= 360;

        _rotation.y += _lastEulerAngles?.y ?? 0;
        _rotation.x += x;
        _lastEulerAngles = null;
    }

    public void SetClamp(Vector2 _xClamp, Vector2 _yClamp) {
        this._xClamp = _xClamp;
        this._yClamp = _yClamp;
    }

    public void SetClamp(float minX, float maxX, float minY, float maxY) => SetClamp(new Vector2(minX, maxX), new Vector2(minY, maxY));

    public void SetRotation(Vector2 rotation) => _rotation = rotation;

    public void SetRotation(float x, float y) => SetRotation(new Vector2(x, y));

    private void HandleFOV() {
        if (_player.IsSprinting == _lastSprinting) return;
        if (_player.IsSprinting) _player.ChangeFOVLerp(Camera.fieldOfView, sprintFOV, fovChangeDuration);
        else _player.ChangeFOVLerp(Camera.fieldOfView, normalFOV, fovChangeDuration);
        _lastSprinting = _player.IsSprinting;
    }

    private void Look() {
        while (_rotation.y > 360 || _rotation.y < -360) _rotation.y += _rotation.y > 360 ? -360 : 360;

        if (_player.CanLook) {
            float mouseX = _rotationInput.x * Time.fixedDeltaTime * sensX;
            float mouseY = _rotationInput.y * Time.fixedDeltaTime * sensY;

            _rotation.y += mouseX;
            _rotation.x -= mouseY;
        }

        Vector3 target = _eulerAngles;

        if (target.x > 180) target.x -= 360;
        float xInvert = target.x * -1;

        _rotation.x = Mathf.Clamp(_rotation.x, -90f + xInvert, 90f + xInvert);

        if (_xClamp != Vector2.zero) _rotation.x = Mathf.Clamp(_rotation.x, _xClamp.x, _xClamp.y);
        if (_yClamp != Vector2.zero) _rotation.y = Mathf.Clamp(_rotation.y, _yClamp.x, _yClamp.y);

        RotateCamera(target);

        if (Camera.transform.localEulerAngles.y != _lastCameraRotation) {
            _server.SetOrientationServerRpc(Camera.transform.localEulerAngles);
            _lastCameraRotation = Camera.transform.localEulerAngles.y;
        }
    }

    private void RotateCamera(Vector2 target) {
        Camera.transform.rotation = Quaternion.Euler(_rotation.x + target.x, _rotation.y + target.y, 0);
    }

    private void SetInputState(bool enabled) {
        if (!IsOwner) return;

        void OnLook(Vector2 rotation) => _rotationInput = rotation;

        if (enabled) _inputReader.LookEvent += OnLook;
        else _inputReader.LookEvent -= OnLook;
    }
}