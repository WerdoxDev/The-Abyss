using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerData : NetworkBehaviour {
    public NetworkVariable<PlayerDataInfo> PlayerDataInfo = new NetworkVariable<PlayerDataInfo>();

    private PlayerCustomization _playerCustomization;
    private Player _player;

    private void Awake() {
        _playerCustomization = GetComponent<PlayerCustomization>();
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn() {
        PlayerDataInfo.OnValueChanged += (PlayerDataInfo oldInfo, PlayerDataInfo newInfo) => {
            Debug.Log("Changed to:" + newInfo);
            _playerCustomization.UpdatePlayer(newInfo.DisplayName.ToString(), newInfo.Customization);
            UIManager.Instance.AddPlayerName(transform, newInfo.DisplayName.ToString(), _playerCustomization.NameOffset);
        };


        if (IsServer) {
            PlayerConnData? playerData = TheAbyssNetworkManager.Instance.GetPlayerData(OwnerClientId);
            if (playerData.HasValue)
                PlayerDataInfo.Value = new PlayerDataInfo(playerData.Value.PlayerName, playerData.Value.PlayerCustomization);

        } else {
            _playerCustomization.UpdatePlayer(PlayerDataInfo.Value.DisplayName.ToString(), PlayerDataInfo.Value.Customization);
            UIManager.Instance.AddPlayerName(transform, PlayerDataInfo.Value.DisplayName.ToString(), _playerCustomization.NameOffset);
        }
        _player.FullySpawned();
    }

    public override void OnDestroy() {
        UIManager.Instance.RemovePlayerName(PlayerDataInfo.Value.DisplayName.ToString());
    }
}

public struct PlayerDataInfo : INetworkSerializable {
    public FixedString32Bytes DisplayName;
    public PlayerCustomizationInfo Customization;

    public PlayerDataInfo(FixedString32Bytes displayName, PlayerCustomizationInfo customization) {
        DisplayName = displayName;
        Customization = customization;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref DisplayName);
        Customization.NetworkSerialize(serializer);
    }
}