using UnityEngine;
using Unity.Netcode;

public class SailControlHandle : NetworkBehaviour, Interactable
{
    [Header("Settings")]
    public Ship ConnectedShip;

    public InteractHandler Interact;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        Interact.interactable = this;
    }

    // Because this object is spawned and then parented to ConnectedShip
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject) => ConnectedShip = GetComponentInParent<Ship>();

    public InteractHandler GetHandler(byte handlerData) => Interact;
}
