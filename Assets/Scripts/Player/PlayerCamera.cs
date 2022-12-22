using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] private float normalFOV;
    [SerializeField] private float sprintFOV;
    [SerializeField] private float fovChangeDuration;
    private Transform _target;
    private bool _lastSprinting;

    public bool CanLook = true;
    public Camera Camera;

    private Vector2 _xClamp;
    private Vector2 _yClamp;
    private Vector2 _rotationInput;
    private Vector2 _rotation;
    private float lastOrientationRotation;

    [Header("Settings")]
    [SerializeField] private Transform orientation;

    private Player _player;
    private InputReader _inputReader;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Camera.gameObject.GetComponent<AudioListener>().enabled = false;
            Camera.enabled = false;
            return;
        }

        _player = GetComponent<Player>();
        _inputReader = _player.InputReader;

        SetInputState(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;
        HandleFOV();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CanLook = !CanLook;
            Cursor.lockState = CanLook ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = CanLook ? false : true;
        }
        Look();
    }

    public void SetClamp(Vector2 _xClamp, Vector2 _yClamp)
    {
        this._xClamp = _xClamp;
        this._yClamp = _yClamp;
    }

    public void SetClamp(float minX, float maxX, float minY, float maxY)
    {
        this._xClamp = new Vector2(minX, maxX);
        this._yClamp = new Vector2(minY, maxY);
    }

    public void SetOffset(Transform _target)
    {
        this._target = _target;
        _rotation.y -= _target.eulerAngles.y;
        _rotation.x += 360 - _target.eulerAngles.x;
    }

    public void SetRotation(Vector2 rotation) => _rotation = rotation;

    public void SetRotation(float x, float y) => _rotation = new Vector2(x, y);

    public void ClearOffset()
    {
        if (_target == null) return;
        _rotation.y += _target.eulerAngles.y;
        _rotation.x += 360 + _target.eulerAngles.x;
        _target = null;
    }

    private void HandleFOV()
    {
        if (_player.IsSprinting == _lastSprinting) return;
        if (_player.IsSprinting) _player.ChangeFOVLerp(Camera.fieldOfView, sprintFOV, fovChangeDuration);
        else _player.ChangeFOVLerp(Camera.fieldOfView, normalFOV, fovChangeDuration);
        _lastSprinting = _player.IsSprinting;
    }

    private void Look()
    {
        while (_rotation.x > 90) _rotation.x -= 360;
        while (_rotation.y > 360 || _rotation.y < -360) _rotation.y += _rotation.y > 360 ? -360 : 360;
        if (CanLook)
        {
            float mouseX = _rotationInput.x * Time.fixedDeltaTime * sensX;
            float mouseY = _rotationInput.y * Time.fixedDeltaTime * sensY;

            _rotation.y += mouseX;
            _rotation.x -= mouseY;
        }
        _rotation.x = Mathf.Clamp(_rotation.x, -90f, 90f);

        if (_xClamp != Vector2.zero) _rotation.x = Mathf.Clamp(_rotation.x, _xClamp.x, _xClamp.y);
        if (_yClamp != Vector2.zero) _rotation.y = Mathf.Clamp(_rotation.y, _yClamp.x, _yClamp.y);

        Vector3 _target = _player.RotationTarget != null ? _player.RotationTarget.eulerAngles : Vector3.zero;
        Camera.transform.rotation = Quaternion.Euler(_rotation.x + _target.x, _rotation.y + _target.y, 0);

        if (_rotation.y + _target.y != lastOrientationRotation)
        {
            SetOrientationServerRpc(new Vector3(0, _rotation.y + _target.y, 0));
            lastOrientationRotation = _rotation.y + _target.y;
        }
    }

    [ServerRpc]
    private void SetOrientationServerRpc(Vector3 rotation) => orientation.rotation = Quaternion.Euler(rotation);

    private void SetInputState(bool enabled)
    {
        void OnLook(Vector2 rotation) => _rotationInput = rotation;

        if (enabled) _inputReader.LookEvent += OnLook;
        else _inputReader.LookEvent -= OnLook;
    }
}
