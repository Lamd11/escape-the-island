using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("What to spawn")]
    [Tooltip("Prefab that already has CollectResource + collider/trigger setup.")]
    public GameObject foodPrefab;

    [Header("How many")]
    public int count = 20;

    [Header("Where to spawn (local space box)")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(40f, 0f, 40f);

    [Header("Placement rules")]
    [Tooltip("Try this many random points per food pickup before giving up.")]
    public int maxAttemptsPerFood = 25;
    [Tooltip("Minimum distance from other spawned food pickups.")]
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

    [ContextMenu("Spawn Food")]
    public void Spawn()
    {
        if (foodPrefab == null)
        {
            Debug.LogWarning("FoodSpawner: Assign a foodPrefab first.");
            return;
        }

        int spawnedCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (TryFindSpawnPoint(out Vector3 pos))
            {
                GameObject go = Instantiate(foodPrefab, pos, Quaternion.identity, transform);
                spawned.Add(go);
                spawnedPositions.Add(pos);
                spawnedCount++;
            }
        }

        Debug.Log($"FoodSpawner: Spawned {spawnedCount}/{count} food pickups.");
    }

    [ContextMenu("Clear Spawned Food")]
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

        for (int attempt = 0; attempt < maxAttemptsPerFood; attempt++)
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
        Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.35f);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.z));
        Gizmos.matrix = old;
    }
}

