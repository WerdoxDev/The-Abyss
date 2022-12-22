using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> displayName = new NetworkVariable<FixedString32Bytes>();

    public Transform NamePos;

    public override void OnNetworkSpawn()
    {
        //TODO: Change gameui stuff to work with new the approach
        displayName.OnValueChanged += (FixedString32Bytes oldValue, FixedString32Bytes newValue) =>
        {
            Debug.Log("Changed to:" + newValue);
            // GameUIController.Instance.AddPlayerName(newValue.ToString(), NamePos);
        };

        if (!IsServer)
        {
            // GameUIController.Instance.AddPlayerName(displayName.Value.ToString(), NamePos);
            Debug.Log("here");
            return;
        };

        PlayerConnData? playerData = TheAbyssNetworkManager.Instance.GetPlayerData(OwnerClientId);
        if (playerData.HasValue)
        {
            displayName.Value = playerData.Value.PlayerName;
            Debug.Log(playerData.Value.PlayerName);
        }
    }

    public override void OnDestroy()
    {
        //TODO: Change gameui stuff to work with new the approach
        // GameUIController.Instance.RemovePlayerName(displayName.Value.ToString());
    }
}
