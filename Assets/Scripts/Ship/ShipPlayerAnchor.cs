using System.Collections.Generic;
using UnityEngine;

public class ShipPlayerAnchor : MonoBehaviour
{
    [Header("Anchoring")]

    [Tooltip("Allowed distance between player and anchor position when player is grounded")]
    [SerializeField] private float allowedDistance;

    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject dummyPrefab;
    [SerializeField] private Transform dummyHolder;
    [SerializeField] private Vector3 sizeOffset;

    public List<Player> Players = new List<Player>();
    private List<GameObject> dummyObjects = new List<GameObject>();

    private void FixedUpdate()
    {
        if (dummyObjects.Count != Players.Count)
        {
            foreach (GameObject dummy in dummyObjects) Destroy(dummy);
            dummyObjects.Clear();
            for (int i = 0; i < Players.Count; i++)
                dummyObjects.Add(Instantiate(dummyPrefab, Vector3.zero, Quaternion.identity, dummyHolder));
        }

        int index = 0;
        foreach (Player player in Players)
        {
            if (player == null) return;
            Vector3 playerSize = player.PlayerSize - sizeOffset;
            Vector3 playerPos = player.transform.position;
            dummyObjects[index].transform.position = new Vector3(playerPos.x, dummyObjects[index].transform.position.y, playerPos.z);

            List<Vector3> positions = new List<Vector3>();
            if (Physics.Raycast(dummyObjects[index].transform.position + new Vector3(playerSize.x, 1f, playerSize.z), Vector3.down, out RaycastHit hit1, 2, groundLayer)) positions.Add(hit1.point);
            if (Physics.Raycast(dummyObjects[index].transform.position + new Vector3(playerSize.x, 1f, -playerSize.z), Vector3.down, out RaycastHit hit2, 2, groundLayer)) positions.Add(hit2.point);
            if (Physics.Raycast(dummyObjects[index].transform.position + new Vector3(-playerSize.x, 1f, -playerSize.z), Vector3.down, out RaycastHit hit3, 2, groundLayer)) positions.Add(hit3.point);
            if (Physics.Raycast(dummyObjects[index].transform.position + new Vector3(-playerSize.x, 1f, playerSize.z), Vector3.down, out RaycastHit hit4, 2, groundLayer)) positions.Add(hit4.point);
            if (Physics.Raycast(dummyObjects[index].transform.position + new Vector3(0, 1f, 0), Vector3.down, out RaycastHit hit5, 2, groundLayer)) positions.Add(hit5.point);

            Vector3 highestPoint = positions.Count > 0 ? positions[0] : Vector3.zero;
            for (int i = 0; i < positions.Count; i++)
                if (positions[i].y > highestPoint.y) highestPoint = positions[i];

            if (highestPoint != Vector3.zero) dummyObjects[index].transform.position = highestPoint;
            else dummyObjects[index].transform.position = playerPos - new Vector3(0, (playerSize.y / 2), 0);

            if (hit1.point != Vector3.zero) Debug.DrawLine(hit1.point, hit1.point + new Vector3(0, 2, 0), Color.cyan, 0.1f);
            if (hit2.point != Vector3.zero) Debug.DrawLine(hit2.point, hit2.point + new Vector3(0, 2, 0), Color.cyan, 0.1f);
            if (hit3.point != Vector3.zero) Debug.DrawLine(hit3.point, hit3.point + new Vector3(0, 2, 0), Color.cyan, 0.1f);
            if (hit4.point != Vector3.zero) Debug.DrawLine(hit4.point, hit4.point + new Vector3(0, 2, 0), Color.cyan, 0.1f);
            if (hit5.point != Vector3.zero) Debug.DrawLine(hit5.point, hit5.point + new Vector3(0, 2, 0), Color.cyan, 0.1f);
            Debug.DrawLine(highestPoint, highestPoint + new Vector3(0, 2, 0), Color.yellow, 0.1f);

            float playerDistance = (playerPos.y - dummyObjects[index].transform.position.y) - player.Offset;
            Debug.Log(playerDistance);

            bool canAnchor = player.IsGrounded && !player.IsOnAttachable(InteractType.Ladder);
            if (canAnchor && playerDistance > allowedDistance)
            {
                dummyObjects[index].transform.position = playerPos - new Vector3(0, (playerSize.y / 2), 0);
                canAnchor = false;
            }

            if (canAnchor && highestPoint != Vector3.zero)
            {
                playerPos.y = highestPoint.y + player.Offset;
                player.transform.position = playerPos;
            }

            index++;
        }
    }

    private void OnDrawGizmos()
    {
        if (dummyObjects.Count == 0) return;
        for (int i = 0; i < dummyObjects.Count; i++)
        {
            Gizmos.DrawRay(dummyObjects[i].transform.position + new Vector3(0, 1f, 0), Vector3.down * 2);
        }
    }
}
