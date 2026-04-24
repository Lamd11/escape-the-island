using System.Collections.Generic;
using UnityEngine;

public class RopeSpawner : MonoBehaviour
{
    [Header("What to spawn")]
    [Tooltip("Prefab that already has CollectResource + collider/trigger setup.")]
    public GameObject ropePrefab;

    [Header("How many")]
    public int count = 15;

    [Header("Where to spawn (local space box)")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(40f, 0f, 40f);

    [Header("Placement rules")]
    [Tooltip("Try this many random points per rope before giving up.")]
    public int maxAttemptsPerRope = 25;
    [Tooltip("Minimum distance from other spawned rope pickups.")]
    public float minSpacing = 2.0f;
    [Tooltip("Raycast down to place on ground.")]
    public bool snapToGround = true;
    public LayerMask groundMask = ~0;
    public float raycastHeight = 50f;
    public float raycastDistance = 200f;

    [Header("When to spawn")]
    public bool spawnOnStart = false;

    readonly List<GameObject> spawned = new();
    readonly List<Vector3> spawnedPositions = new();

    void Start()
    {
        if (spawnOnStart)
        {
            Spawn();
        }
    }

    [ContextMenu("Spawn Rope")]
    public void Spawn()
    {
        if (ropePrefab == null)
        {
            Debug.LogWarning("RopeSpawner: Assign a ropePrefab first.");
            return;
        }

        int spawnedCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (TryFindSpawnPoint(out Vector3 pos))
            {
                GameObject go = Instantiate(ropePrefab, pos, Quaternion.identity, transform);
                spawned.Add(go);
                spawnedPositions.Add(pos);
                spawnedCount++;
            }
        }

        Debug.Log($"RopeSpawner: Spawned {spawnedCount}/{count} rope pickups.");
    }

    [ContextMenu("Clear Spawned Rope")]
    public void Clear()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null)
            {
                DestroyImmediate(spawned[i]);
            }
        }
        spawned.Clear();
        spawnedPositions.Clear();
    }

    bool TryFindSpawnPoint(out Vector3 worldPos)
    {
        Vector3 half = areaSize * 0.5f;

        for (int attempt = 0; attempt < maxAttemptsPerRope; attempt++)
        {
            Vector3 local = new Vector3(
                Random.Range(areaCenter.x - half.x, areaCenter.x + half.x),
                areaCenter.y,
                Random.Range(areaCenter.z - half.z, areaCenter.z + half.z)
            );

            Vector3 candidate = transform.TransformPoint(local);

            if (snapToGround)
            {
                Vector3 rayStart = candidate + Vector3.up * raycastHeight;
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
                {
                    candidate = hit.point;
                }
                else
                {
                    continue;
                }
            }

            if (minSpacing > 0f && IsTooCloseToExisting(candidate, minSpacing))
            {
                continue;
            }

            worldPos = candidate;
            return true;
        }

        worldPos = default;
        return false;
    }

    bool IsTooCloseToExisting(Vector3 candidate, float spacing)
    {
        float sqr = spacing * spacing;
        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            if ((spawnedPositions[i] - candidate).sqrMagnitude < sqr)
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.35f);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.z));
        Gizmos.matrix = old;
    }
}

