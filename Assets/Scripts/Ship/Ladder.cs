using UnityEngine;
using Unity.Netcode;

public class Ladder : NetworkBehaviour, Interactable
{
    [Header("Climbing")]
    public float climbSpeed;

    [Header("Settings")]
    public Transform MidTransform;
    public Transform LandTransform;

    public InteractHandler TopInteract;
    public InteractHandler BottomInteract;

    public float PlayerOffset;

    [HideInInspector] public Ship ship;

    public Vector3 MidPos { get => MidTransform.position; }
    public Vector3 LandPos { get => LandTransform.position; }

    public Vector3 LocalMidPos { get => MidTransform.localPosition; }
    public Vector3 LocalLandPos { get => LandTransform.localPosition; }

    public override void OnNetworkSpawn()
    {
        TopInteract.interactable = this;
        BottomInteract.interactable = this;
    }

    public InteractHandler GetHandler(byte handlerData)
    {
        if (TopInteract.Data == handlerData) return TopInteract;
        if (BottomInteract.Data == handlerData) return BottomInteract;
        return null;
    }
}

