using UnityEngine;
using System.Linq;
using Unity.Netcode;

public class ServerPlayerAttachable : NetworkBehaviour {
    public InteractHandler Handler;

    public NetworkVariable<bool> IsAttached = new(false);

    private Player _player;
    private Vector2 _movementInput;
    private Transform _standTransform;

    private Attachable _attachable;

    private ClientPlayerAttachable _client;

    private void Awake() {
        _client = GetComponent<ClientPlayerAttachable>();
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            enabled = false;
            return;
        }
    }

    private void FixedUpdate() {
        if (!IsAttached.Value) return;

        //else if (_attachable.Ladder != null) {
        //    Ladder Ladder = _attachable.Ladder;
        //    Vector3 upVector = Ladder.MidPos + Ladder.MidTransform.up * Ladder.PlayerOffset;
        //    Ladder.PlayerOffset += _movementInput.y * 0.1f * Ladder.climbSpeed;

        //    transform.position = upVector;
        //    Vector3 localPos = Ladder.transform.InverseTransformPoint(transform.position);

        //    if (localPos.y - _player.Offset > Ladder.TopInteract.LocalStandPos.y) {
        //        transform.position = Ladder.LandPos + _player.OffsetVector;
        //        Detach();
        //    }
        //    else if (localPos.y < Ladder.BottomInteract.LocalStandPos.y) Detach();
        //}

        //if (Handler != null) {
        //    if (Handler.Type != InteractType.Ladder) transform.position = _standTransform.position + _standTransform.up * _player.Offset;
        //    transform.rotation = _standTransform.rotation;
        //}
    }

    public void Attach() {
        if (Handler == null) return;

        _attachable.Reset();
        
        //if (Handler.Type == InteractType.Ladder) {
        //    Ladder Ladder = (Ladder)Handler.interactable;

        //    Vector3 localPos = Ladder.transform.InverseTransformPoint(transform.position);
        //    Ladder.PlayerOffset = Handler.Data == 0 ? localPos.y - 1 : localPos.y;
        //    Ladder.PlayerOffset = Mathf.Clamp(Ladder.PlayerOffset, Ladder.BottomInteract.LocalStandPos.y + 0.5f, Ladder.TopInteract.LocalStandPos.y);
        //    Vector3 position = Ladder.MidPos;
        //    position.y = Ladder.MidPos.y + Ladder.PlayerOffset;
        //    transform.position = position;

        //    _standTransform = Ladder.transform;

        //    // Just to avoid long lines
        //    _attachable.Ladder = Ladder;
        //    _player.ServerTransform.SetLerpInfo(false, false);
        //}        

        //_standTransform = Handler.StandTransform;

        //_player.SetRotationTarget(Handler.StandTransform);
        //_client.SetAttachableSettingsClientRpc(Handler.interactable.NetworkObjectId, true, Handler.Type, Handler.Data, _player.ClientRpcParams);
        //_player.SetMovementState(false);
        //Handler.Occupied.Value = true;
        //IsAttached.Value = true;
    }

    public void Detach() {
        AttachableSetting settings = _client.GetAttachableSetting(Handler.Type, Handler.Data);
        if (settings.resetRotationTarget) _player.ResetRotationTarget();
        if (settings.resetRotation) _player.transform.rotation = Quaternion.identity;

        _client.SetAttachableSettingsClientRpc(Handler.Interactable.NetworkObjectId, false, Handler.Type, Handler.Data, _player.ClientRpcParams);
        _player.SetMovementState(true);
        Handler.Occupied.Value = false;
        IsAttached.Value = false;

        Handler = null;
        _attachable.Reset();
    }

    public void SetHandler(InteractHandler Handler) {
        if (Handler == null) {
            this.Handler = null;
            return;
        }
        //if (Handler.Type == InteractType.Capstan ||
        //    Handler.Type == InteractType.Ladder ||
        //    Handler.Type == InteractType.SailControl ||
        //    Handler.Type == InteractType.Steering) this.Handler = Handler;
    }

    [ServerRpc]
    public void SetMovementInputServerRpc(Vector2 input) => _movementInput = input;
}

public struct Attachable {
    public Ladder Ladder;

    public void Reset() {        
        Ladder = null;
    }
}