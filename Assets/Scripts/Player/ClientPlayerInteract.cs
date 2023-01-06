using UnityEngine;
using Unity.Netcode;

public class ClientPlayerInteract : NetworkBehaviour {
    [Header("Intercation")]
    [SerializeField] private float interactRange;
    [SerializeField] private float interactCooldown;
    [SerializeField] private LayerMask intercatableLayer;
    private bool _interactCooldownFinished = true;

    private Player _player;
    private InputReader _inputReader;
    private InteractHandler _currentHandler;
    private bool busy;

    private ServerPlayerInteract _server;

    private void Awake() {
        _server = GetComponent<ServerPlayerInteract>();
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) {
            enabled = false;
            return;
        }

        _inputReader = _player.InputReader;
        SetInputState(true);
    }

    public override void OnDestroy() => SetInputState(false);

    private void FixedUpdate() {
        if (busy) return;
        Transform camTransform = _player.CLCamera.Camera.transform;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit interactHit, interactRange, intercatableLayer)) {
            if (_currentHandler == null)
                _currentHandler = interactHit.collider.gameObject.GetComponent<InteractHandler>();

            if (_currentHandler.Occupied.Value || !_currentHandler.Active) {
                UIManager.Instance.InteractionPanel.ClearTarget();
                return;
            };

            if (!_player.Attachable.IsAttached.Value || _player.Attachable.Handler == null)
                UIManager.Instance.InteractionPanel.SetTarget(_currentHandler.transform.position, "F", _currentHandler.UIText);
        } else if (!_player.Attachable.IsAttached.Value && _currentHandler != null) {
            _currentHandler = null;
            UIManager.Instance.InteractionPanel.ClearTarget();
        } else if (_player.Attachable.IsAttached.Value && _currentHandler != null)
            UIManager.Instance.InteractionPanel.ClearTarget();
    }

    [ClientRpc]
    public void UnbusyClientRpc(ClientRpcParams rpcParams = default) => busy = false;

    private void ResetInteractCooldown() => _interactCooldownFinished = true;

    private void SetInputState(bool enabled) {
        if (!IsOwner) return;

        void OnButtonEvent(ButtonType type, bool performed) {
            if (!_player.CanInteract) return;
            if (type == ButtonType.Interact && performed) {
                if (_currentHandler == null || busy) return;
                if (!_player.Attachable.IsAttached.Value && (_currentHandler.Occupied.Value || !_currentHandler.Active)) return;
                if (!_interactCooldownFinished) return;

                busy = true;
                _server.InteractServerRpc(_currentHandler.interactable.NetworkObjectId, _currentHandler.Type, _currentHandler.Data);

                _interactCooldownFinished = false;
                Invoke(nameof(ResetInteractCooldown), interactCooldown);
            }
        }
        if (enabled) _inputReader.ButtonEvent += OnButtonEvent;
        else _inputReader.ButtonEvent -= OnButtonEvent;
    }
}
