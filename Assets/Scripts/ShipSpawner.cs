using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipSpawner : NetworkBehaviour {
    public static ShipSpawner Instance;
    [SerializeField] GameObject smallShipPrefab;
    [SerializeField] GameObject smallShipAssetHolderPrefab;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public Ship SpawnShip(Vector3 position, Quaternion rotation) {
        GameObject shipObj = Instantiate(smallShipPrefab, position, Quaternion.identity);
        shipObj.GetComponent<NetworkObject>().Spawn();

        GameObject assetHolder = SpawnAssetHolder(shipObj, smallShipAssetHolderPrefab, position);

        Ship ship = shipObj.GetComponent<Ship>();
        SpawnerSettings settings = assetHolder.GetComponent<SpawnerSettings>();

        Capstan capstan = SpawnCapstan(assetHolder, settings.CapstanProperties);
        Steering steering = SpawnSteering(assetHolder, settings.SteeringProperties);
        SpawnLadders(assetHolder, settings.LadderProperties);
        SpawnSailControls(assetHolder, settings.SailControlProperties);

        ship.SetSpawnables(capstan, steering);

        shipObj.transform.rotation = rotation;

        return ship;
    }

    private GameObject SpawnAssetHolder(GameObject shipObj, GameObject prefab, Vector3 position) {
        GameObject spawnablesObj = Instantiate(smallShipAssetHolderPrefab, position, Quaternion.identity);
        SpawnObjInNet(spawnablesObj, shipObj);
        return spawnablesObj;
    }

    private Capstan SpawnCapstan(GameObject assetHolder, SpawnProperties properties) {
        GameObject capstanObj = SpawnObj(properties);
        SpawnObjInNet(capstanObj, assetHolder);
        return capstanObj.GetComponent<Capstan>();
    }

    private Steering SpawnSteering(GameObject assetHolder, SpawnProperties properties) {
        GameObject steeringObj = SpawnObj(properties);
        SpawnObjInNet(steeringObj, assetHolder);
        return steeringObj.GetComponent<Steering>();
    }

    private void SpawnLadders(GameObject assetHolder, SpawnProperties[] properties) {
        for (int i = 0; i < properties.Length; i++) {
            GameObject ladderObj = SpawnObj(properties[i]);
            SpawnObjInNet(ladderObj, assetHolder);
        }
    }

    private void SpawnSailControls(GameObject assetHolder, SpawnProperties[] properties) {
        for (int i = 0; i < properties.Length; i++) {
            GameObject sailControlObj = SpawnObj(properties[i]);
            if (properties[i].Direction == SpawnDirection.Right) {
                Transform standPos = sailControlObj.transform.GetChild(0);
                standPos.localPosition = new Vector3(-standPos.localPosition.x, standPos.localPosition.y, standPos.localPosition.z);
            }
            SpawnObjInNet(sailControlObj, assetHolder);
        }
    }

    private GameObject SpawnObj(SpawnProperties properties) {
        Transform spawnTransform = properties.SpawnTransform;
        return Instantiate(properties.Prefab, spawnTransform.position, spawnTransform.rotation);
    }

    private void SpawnObjInNet(GameObject obj, GameObject parent) {
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        netObj.Spawn();
        netObj.TrySetParent(parent);
    }
}
