using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSettings : MonoBehaviour
{
    public SpawnProperties CapstanProperties;

    public SpawnProperties SteeringProperties;

    public SpawnProperties[] LadderProperties;
    
    public SpawnProperties[] SailControlProperties;
}

[System.Serializable]
public class SpawnProperties
{
    public GameObject Prefab;
    public Transform SpawnTransform;
    public SpawnDirection Direction;
}

public enum SpawnDirection { Left, Right, Middle }
// public enum ShipSpawnable { Capstan, Steering, Ladder, SailControl }
