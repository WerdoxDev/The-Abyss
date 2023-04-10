using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour {
    [Header("Settings")]
    [SerializeField] private LayerMask shipLayer;
    [SerializeField] private string playerLayer;

    public InputReader InputReader;
    public Vector3 PlayerSize;
    public Vector3 OffsetVector = new(0, 2, 0);
    public float Offset = 2;

    [Header("Autoset Fields")]
    public PlayerData Data;
    public ServerPlayerCamera SRCamera;
    public ClientPlayerCamera CLCamera;
    public ServerPlayerMovement Movement;
    public ServerPlayerAttachable Attachable;
    public ServerPlayerDrag SRDragHandler;
    public ClientPlayerDrag CLDragHandler;
    public ServerPlayerInteract SRInteract;
    public ClientPlayerInteract CLInteract;
    public ServerTransform ServerTransform;
    public Rigidbody Rb;
    public Ship CurrentShip;

    public ClientRpcParams ClientRpcParams;

    public event Action OnRotationTargetChanged;

    private GameObject _lastShip;

    public bool ApplyGravity { get => Movement.ApplyGravity; set => Movement.ApplyGravity = value; }
    public bool UseFloater { get => Movement.UseFloater; set => Movement.UseFloater = value; }
    public float CameraFOV { get => CLCamera.Camera.fieldOfView; set => CLCamera.Camera.fieldOfView = value; }

    public bool CanMove = true;
    public bool CanUseAttachable = true;
    public bool CanJump = true;
    public bool CanSprint = true;
    public bool CanLook = true;
    public bool CanInteract = true;
    public bool CanDrag = true;

    public bool IsGrounded { get => Movement.IsGrounded; }
    public bool IsSprinting { get => Movement.IsSprinting.Value; }

    private void Awake() {
        Rb = GetComponent<Rigidbody>();
        SRCamera = GetComponent<ServerPlayerCamera>();
        CLCamera = GetComponent<ClientPlayerCamera>();
        Attachable = GetComponent<ServerPlayerAttachable>();
        SRDragHandler = GetComponent<ServerPlayerDrag>();
        CLDragHandler = GetComponent<ClientPlayerDrag>();
        SRInteract = GetComponent<ServerPlayerInteract>();
        CLInteract = GetComponent<ClientPlayerInteract>();
        Movement = GetComponent<ServerPlayerMovement>();
        ServerTransform = GetComponent<ServerTransform>();
        Data = GetComponent<PlayerData>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) {
            SetLayerRecursively(transform, LayerMask.NameToLayer(playerLayer));
            return;
        }

        ClientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };

        Debug.Log(OwnerClientId + " Player");
        GameManager.Instance.OnPause += () => DisableKeybinds();
        GameManager.Instance.OnResume += () => EnableKeybinds();

        // ChatManager.Instance.OnOpened += () => DisableKeybinds();
        // ChatManager.Instance.OnClosed += () => EnableKeybinds();

        //#if UNITY_EDITOR
        //        if (!IsServer) return;
        //        Ship ship = ShipSpawner.Instance.SpawnShip(new Vector3(0, -25, -25), Quaternion.Euler(-10, 0, 0));
        //        ship.Rb.constraints = RigidbodyConstraints.FreezeAll;
        //#endif
    }

    public override void OnDestroy() {
        GameManager.Instance.PlayerDespawned(this);
    }

    private void FixedUpdate() {
        if (!IsServer) return;

        if (Keyboard.current.kKey.wasPressedThisFrame && IsOwner && !ChatManager.Instance.IsOpen) {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y += 20;
            ShipSpawner.Instance.SpawnShip(spawnPosition, Quaternion.Euler(0, transform.eulerAngles.y, 0));
        }

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, shipLayer)) {
            if (CurrentShip == null || CurrentShip.gameObject != _lastShip) {
                CurrentShip = hit.collider.transform.GetComponentInParent<Ship>();
                CurrentShip.PlayerAnchor.Players.Add(this);
                ServerTransform.SetLerpInfo(false, false);
                ResetRotationTarget();
                _lastShip = CurrentShip.gameObject;
            }
        }
        else if (CurrentShip != null && SRCamera.Target.gameObject == CurrentShip.gameObject) {
            CurrentShip.PlayerAnchor.Players.Remove(this);
            CurrentShip = null;
            ServerTransform.SetLerpInfo(true, false);
            SetRotationTarget(null);
        }
    }

    public void FullySpawned() {
        GameManager.Instance.PlayerSpawned(this, IsOwner);
    }

    public bool IsOnAttachable(InteractType type) {
        if (Attachable.Handler == null) return false;
        return Attachable.Handler.Type == type && Attachable.IsAttached.Value;
    }

    public void SetRotationTarget(Transform rotationTarget) {
        SRCamera.SetTarget(rotationTarget);
    }

    public void ResetRotationTarget() {
        if (CurrentShip != null) SetRotationTarget(CurrentShip.transform);
        else SetRotationTarget(null);
    }

    public void SetMovementState(bool enabled) {
        if (!IsServer) return;
        if (enabled) {
            ApplyGravity = true;
            UseFloater = true;
            CanMove = true;
            CanJump = true;
            CanSprint = true;
            return;
        }

        ApplyGravity = false;
        UseFloater = false;
        CanMove = false;
        CanJump = false;
        CanSprint = false;
    }

    public void DisableKeybinds() {
        CanMove = false;
        CanLook = false;
        CanUseAttachable = false;
        CanInteract = false;
        CanJump = false;
        CanSprint = false;
        CanDrag = false;
    }

    public void EnableKeybinds() {
        CanMove = true;
        CanLook = true;
        CanUseAttachable = true;
        CanInteract = true;
        CanJump = true;
        CanSprint = true;
        CanDrag = true;
    }

    public void ChangeFOVLerp(float startFOV, float endFOV, float duration) => StartCoroutine(ChangeFOV(startFOV, endFOV, duration));

    public void RotationTargetChanged() {
        OnRotationTargetChanged?.Invoke();
    }

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
