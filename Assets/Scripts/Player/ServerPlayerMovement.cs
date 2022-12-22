using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class ServerPlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float maxSlopeAngle;
    private bool _lockMovement = false;
    private float _currentSpeed;

    public bool CanMove = true;

    [Header("Sprinting")]
    [SerializeField] private float sprintSpeed;

    public NetworkVariable<bool> IsSprinting = new NetworkVariable<bool>();

    [Header("Jumping")]
    [SerializeField] private float jumpAmount;
    [SerializeField] private float jumpCooldown;
    private bool _isJumping = false;

    public bool CanJump = true;
    public bool IsGrounded;

    [Header("Gravity")]
    [SerializeField] private float yUpMultiplier;
    [SerializeField] private float yDownMultiplier;
    private float _yVelocity;

    public bool ApplyGravity = true;

    [Header("Floating")]
    [SerializeField] private float allowedDistanceToWave;
    [SerializeField] private float floatingYOffset;

    public bool UseFloater = true;

    [Header("Ground Control")]

    [Tooltip("How much should be the y velocity before grounded can be true when jumping")]
    [SerializeField] private float yVelocityThreshold;

    [SerializeField] private float groundSize;
    [SerializeField] private float airSize;
    private float _currentSize;

    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform orientation;

    private Player _player;
    private Rigidbody _rb;
    private RaycastHit _slopeHit;

    private Vector3 _debugVelocity;
    private Vector2 _movementInput;
    private Vector3 _moveDirection;
    private Vector3 _shipPointVelocity;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        _rb = _player.Rb;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        if (IsGrounded) _rb.drag = groundDrag;
        else _rb.drag = 0;

        HandleMovementState();

        if (Keyboard.current.qKey.wasPressedThisFrame) _lockMovement = !_lockMovement;
    }

    private void FixedUpdate()
    {
        Move();
        _debugVelocity = _rb.velocity;

        if (Keyboard.current.kKey.wasPressedThisFrame && IsOwner)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y += 20;
            ShipSpawner.Instance.SpawnShip(spawnPosition, Quaternion.identity);
        }
    }

    private void HandleMovementState()
    {
        if (IsSprinting.Value) _currentSpeed = sprintSpeed;
        else _currentSpeed = moveSpeed;
    }

    private void Move()
    {
        if (CanMove && !_lockMovement) _moveDirection = orientation.forward * _movementInput.y + orientation.right * _movementInput.x;
        else if (!CanMove) _moveDirection = new Vector3(0, 0, 0);

        if (_player.CurrentShip != null)
        {
            Vector3 playerPos = transform.position - _player.OffsetVector;

            _shipPointVelocity = _player.CurrentShip.Rb.GetPointVelocity(playerPos);

            _rb.velocity = new Vector3(_shipPointVelocity.x, 0, _shipPointVelocity.z);
        }
        else
            _rb.velocity = new Vector3(0, 0, 0);

        if (_isJumping) Jump();
        if (IsGrounded)
        {
            _rb.velocity += GetMoveDirection() * _currentSpeed;
            _yVelocity = 0;
            _currentSize = groundSize;
        }
        else
        {
            _rb.velocity += _moveDirection.normalized * _currentSpeed * airMultiplier;

            float waveHeight = WaveManager.Instance.GetWaveHeight(transform.position.x, transform.position.z);
            float distance = Mathf.Abs(transform.position.y - waveHeight);
            if ((transform.position.y <= waveHeight || distance <= allowedDistanceToWave) && UseFloater && _player.CurrentShip == null)
            {
                ApplyGravity = false;
                transform.position = new Vector3(transform.position.x, waveHeight + floatingYOffset, transform.position.z);
            }
            else if (UseFloater) ApplyGravity = true;

            if (ApplyGravity)
            {
                if (Physics.Raycast(transform.position, Vector3.up, _player.PlayerSize.y * Mathf.Clamp(_yVelocity, 0, 1) * 0.5f + 0.1f, groundLayer))
                    _yVelocity = 0;

                _rb.velocity = new Vector3(
                    _rb.velocity.x,
                    (_player.CurrentShip != null ? _shipPointVelocity.y : 0) + (_yVelocity * yUpMultiplier),
                    _rb.velocity.z);

                _yVelocity -= Time.fixedDeltaTime * yDownMultiplier;
                _yVelocity = Mathf.Clamp(_yVelocity, Physics.gravity.y, jumpAmount);
            }
            else
            {
                _yVelocity = 0;
                _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            }
        }
        IsGrounded = Physics.BoxCast(transform.position, new Vector3(0.9f, _currentSize, 0.9f), Vector3.down, Quaternion.identity, _player.PlayerSize.y * 0.5f + _currentSize, groundLayer) && (_currentSize == airSize ? _rb.velocity.y < yVelocityThreshold + (_player.CurrentShip != null ? _shipPointVelocity.y : 0) : true);
    }

    private void Jump()
    {
        if (!CanJump || !IsGrounded) return;

        IsGrounded = false;
        CanJump = false;
        _currentSize = airSize;

        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _yVelocity = (transform.up * jumpAmount).y;

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump() => CanJump = true;

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, _player.CurrentShip != null ? -_player.CurrentShip.transform.up : Vector3.down, out _slopeHit, _player.PlayerSize.y * 0.5f + 2, groundLayer))
        {
            float angle = Vector3.Angle(_player.CurrentShip != null ? _player.CurrentShip.transform.up : Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetMoveDirection()
    {
        Physics.Raycast(transform.position, _player.CurrentShip != null ? -_player.CurrentShip.transform.up : Vector3.down, out _slopeHit, _player.PlayerSize.y * 0.5f + 2, groundLayer);
        return Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
    }

    private void OnDrawGizmos()
    {
        if (!IsOwner) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(0, -(_player.PlayerSize.y * 0.5f + _currentSize), 0), new Vector3(1.8f, (_currentSize) * 2, 1.8f));
    }

    [ServerRpc]
    public void SetMovementInputServerRpc(Vector2 input) => _movementInput = input;
    [ServerRpc]
    public void SetJumpingStateServerRpc(bool isJumping) => _isJumping = isJumping;
    [ServerRpc]
    public void SetSprintingStateServerRpc(bool IsSprinting) => this.IsSprinting.Value = IsSprinting;

}
