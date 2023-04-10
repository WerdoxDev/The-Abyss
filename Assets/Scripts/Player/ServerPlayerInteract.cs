using Unity.Netcode;
using UnityEngine;

public class ServerPlayerInteract : NetworkBehaviour {
    private Player _player;

    private ClientPlayerInteract _client;

    private void Awake() {
        _client = GetComponent<ClientPlayerInteract>();
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            enabled = false;
            return;
        }
    }

    [ServerRpc]
    public void InteractServerRpc(ulong interactObjId, InteractType type, byte handlerData) {
        if (!_player.CanInteract) return;

        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(interactObjId, out var interactObj);
        if (interactObj == null) {
            _client.UnbusyClientRpc(_player.ClientRpcParams);
            return;
        }

        InteractHandler handler = interactObj.GetComponent<Interactable>().GetHandler(handlerData);

        if (handler == null) {
            Debug.LogError("InteractHandler was not found");
            _client.UnbusyClientRpc(_player.ClientRpcParams);
            return;
        }

        handler.Interact();

        //_player.Attachable.SetHandler(handler);
        //if (_player.Attachable.Handler != null) {
        //    if (!_player.Attachable.IsAttached.Value) _player.Attachable.Attach();
        //    else _player.Attachable.Detach();
        //}
        _client.UnbusyClientRpc(_player.ClientRpcParams);
    }

    [ServerRpc]
    public void StartDragServerRpc(ulong interactObjId, InteractType type, byte handlerData, Vector3 initPosition, Vector3 initNormal) {
        if (!_player.CanInteract) return;

        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(interactObjId, out var interactObj);
        if (interactObj == null) {
            _client.UnbusyClientRpc(_player.ClientRpcParams);
            return;
        }

        InteractHandler handler = interactObj.GetComponent<Interactable>().GetHandler(handlerData);

        if (handler == null) {
            Debug.LogError("InteractHandler was not found");
            _client.UnbusyClientRpc(_player.ClientRpcParams);
            return;
        }

        if (type == InteractType.Wheel) {
            _player.SDragHandler.SetHandler(handler);
            _player.SDragHandler.StartDrag(initPosition, initNormal);
        }

        _client.UnbusyClientRpc(_player.ClientRpcParams);
    }

    [ServerRpc]
    public void StopDragServerRpc(InteractType type, byte handlerData) {
        _player.SDragHandler.StopDrag();
    }
}
