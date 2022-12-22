using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.Netcode.Transports.UTP;


public class Player : NetworkBehaviour {
    [Header("Settings")]
    [SerializeField] private LayerMask shipLayer;
    [SerializeField] private string playerLayer;

    public InputReader InputReader;
    public Transform RotationTarget;
    public Vector3 PlayerSize;
    public Vector3 OffsetVector = new Vector3(0, 2, 0);
    public float Offset = 2;

    [Header("Autoset Fields")]
    public PlayerData Data;
    public PlayerCamera PCamera;
    public ServerPlayerMovement Movement;
    public ServerPlayerAttachable Attachable;
    public Rigidbody Rb;
    public Ship CurrentShip;

    private GameObject _lastShip;

    public bool ApplyGravity { get => Movement.ApplyGravity; set => Movement.ApplyGravity = value; }
    public bool UseFloater { get => Movement.UseFloater; set => Movement.UseFloater = value; }
    public float CameraFOV { get => PCamera.Camera.fieldOfView; set => PCamera.Camera.fieldOfView = value; }

    public bool CanMove { get => Movement.CanMove; set => Movement.CanMove = value; }
    public bool CanJump { get => Movement.CanJump; set => Movement.CanJump = value; }

    public bool IsGrounded { get => Movement.IsGrounded; }
    public bool IsSprinting { get => Movement.IsSprinting.Value; }

    private void Awake() {
        Rb = GetComponent<Rigidbody>();
        PCamera = GetComponent<PlayerCamera>();
        Attachable = GetComponent<ServerPlayerAttachable>();
        Movement = GetComponent<ServerPlayerMovement>();
        Data = GetComponent<PlayerData>();
    }

    public override void OnNetworkSpawn() {
        GameManager.Instance.PlayerSpawned(this, IsOwner);
        if (!IsOwner) {
            SetLayerRecursively(transform, LayerMask.NameToLayer(playerLayer));
            return;
        }

        InputReader.Init();
        Application.targetFrameRate = 60;
        //TODO: Change gameui stuff to work with new the approach
        // GameUIController.Instance.PlayerCamera = PCamera.Camera;
    }

    private void FixedUpdate() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, shipLayer)) {
            if (CurrentShip == null || CurrentShip.gameObject != _lastShip.gameObject) {
                CurrentShip = hit.collider.transform.GetComponentInParent<Ship>();
                CurrentShip.PlayerAnchor.Players.Add(this);
                if (IsOwner) ResetRotationTarget();
                _lastShip = CurrentShip.gameObject;
            }
        } else if (CurrentShip != null && (!IsOwner || RotationTarget.name == CurrentShip.name)) {
            CurrentShip.PlayerAnchor.Players.Remove(this);
            CurrentShip = null;
            if (IsOwner) SetRotationTarget(null);
        }
    }

    public bool IsOnAttachable(InteractType type) {
        if (Attachable.Handler == null) return false;
        return Attachable.Handler.Type == type && Attachable.IsAttached.Value;
    }

    public void SetRotationTarget(Transform RotationTarget) {
        if (RotationTarget != null) PCamera.SetOffset(RotationTarget);
        else PCamera.ClearOffset();
        this.RotationTarget = RotationTarget;
    }

    public void SetMovementState(bool enabled) {
        if (!IsServer) return;
        if (enabled) {
            ApplyGravity = true;
            UseFloater = true;
            CanMove = true;
            CanJump = true;
            return;
        }

        ApplyGravity = false;
        UseFloater = false;
        CanMove = false;
        CanJump = false;
    }

    public void ResetRotationTarget() {
        SetRotationTarget(null);
        if (CurrentShip != null) SetRotationTarget(CurrentShip.transform);
        PCamera.SetClamp(Vector2.zero, Vector2.zero);
    }

    public void ChangeFOVLerp(float startFOV, float endFOV, float duration) => StartCoroutine(ChangeFOV(startFOV, endFOV, duration));

    private IEnumerator ChangeFOV(float startFOV, float endFOV, float duration) {
        float timeElapsed = 0;

        while (timeElapsed < duration) {
            CameraFOV = Mathf.Lerp(startFOV, endFOV, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        CameraFOV = endFOV;
    }

    private void SetLayerRecursively(Transform parent, int newLayer) {
        parent.gameObject.layer = newLayer;

        for (int i = 0, count = parent.childCount; i < count; i++) {
            SetLayerRecursively(parent.GetChild(i), newLayer);
        }
    }
}
