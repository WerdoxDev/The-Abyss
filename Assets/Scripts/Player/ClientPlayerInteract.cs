using UnityEngine;
using Unity.Netcode;

public class ClientPlayerInteract : NetworkBehaviour {
    [Header("Intercation")]
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask intercatableLayer;

    private Player _player;
    private InputReader _inputReader;
    private InteractHandler _currentHandler;
    private bool busy;

    private ServerPlayerInteract _server;

    private void Awake() {
        _server = GetComponent<ServerPlayerInteract>();
        _player = GetComponent<Player>();
        _inputReader = _player.InputReader;
        SetInputState(true);
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) {
            enabled = false;
            return;
        }
    }

    private void FixedUpdate() {
        //TODO: Change gameui stuff to work with new the approach
        if (busy) return;
        Transform camTransform = _player.CLCamera.Camera.transform;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit interactHit, interactRange, intercatableLayer)) {
            if (_currentHandler == null)
                _currentHandler = interactHit.collider.gameObject.GetComponent<InteractHandler>();

            if (_currentHandler.Occupied.Value || !_currentHandler.Active) {
                // GameUIController.Instance.ClearInteractTarget();
                return;
            };

            // if (!_player.Attachable.IsAttached.Value || _player.Attachable.Handler == null)
            //     GameUIController.Instance.SetInteractTarget(_currentHandler.transform.position, _currentHandler.UiText);
        } else if (!_player.Attachable.IsAttached.Value && _currentHandler != null) {
            _currentHandler = null;
            // GameUIController.Instance.ClearInteractTarget();
        }
        // else if (_player.Attachable.IsAttached.Value && _currentHandler != null)
        //     GameUIController.Instance.ClearInteractTarget();
    }

    [ClientRpc]
    public void UnbusyClientRpc(ClientRpcParams rpcParams = default) => busy = false;

    private void SetInputState(bool enabled) {
        void OnButtonEvent(ButtonType type, bool performed) {
            if (!_player.CanInteract) return;
            if (type == ButtonType.Interact && performed) {
                if (_currentHandler == null || busy) return;
                if (!_player.Attachable.IsAttached.Value && (_currentHandler.Occupied.Value || !_currentHandler.Active)) return;
                busy = true;
                _server.InteractServerRpc(_currentHandler.interactable.NetworkObjectId, _currentHandler.Type, _currentHandler.Data);
            }
        }
        if (enabled) _inputReader.ButtonEvent += OnButtonEvent;
        else _inputReader.ButtonEvent -= OnButtonEvent;
    }
}
