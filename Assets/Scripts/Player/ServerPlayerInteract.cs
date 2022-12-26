using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ServerPlayerInteract : NetworkBehaviour {
    private Player _player;
    private ClientRpcParams _clientRpcParams;

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

        _clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };
    }

    [ServerRpc]
    public void InteractServerRpc(ulong interactObjId, InteractType type, byte handlerData) {
        if (!_player.CanInteract) return;

        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(interactObjId, out var interactObj);
        if (interactObj == null) {
            _client.UnbusyClientRpc(_clientRpcParams);
            return;
        }

        InteractHandler handler = null;
        handler = interactObj.GetComponent<Interactable>().GetHandler(handlerData);

        if (handler == null) {
            Debug.LogError("InteractHanlder was not found");
            _client.UnbusyClientRpc(_clientRpcParams);
            return;
        }

        handler?.Interact();

        _player.Attachable.SetHandler(handler);
        if (_player.Attachable.Handler != null) {
            if (!_player.Attachable.IsAttached.Value) _player.Attachable.Attach();
            else _player.Attachable.Detach();
        }
        _client.UnbusyClientRpc(_clientRpcParams);
    }
}
