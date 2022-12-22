using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    // private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        // position.OnValueChanged += (Vector3 prevValue, Vector3 newValue) =>
        // {
        //     transform.position = newValue;
        //     Debug.Log("new position");
        // };
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector3 moveDireciton = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) moveDireciton.z = 1f;
        if (Keyboard.current.sKey.isPressed) moveDireciton.z = -1f;
        if (Keyboard.current.aKey.isPressed) moveDireciton.x = -1f;
        if (Keyboard.current.dKey.isPressed) moveDireciton.x = 1f;

        float moveSpeed = 3f;
        transform.position += moveDireciton * moveSpeed * Time.deltaTime;
    }
}
