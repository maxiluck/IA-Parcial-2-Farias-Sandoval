using System.Collections.Generic;
using UnityEngine;

public class InterestObjectSpawner : MonoBehaviour
{
    [Header("Objetos")]
    [SerializeField] private PointOfInterest interestPrefab;
    [SerializeField] private Transform objectsRoot;
    [SerializeField, Range(1, 5)] private int maxActiveObjects = 5;
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;

    [Header("Posición dentro del coto")]
    [SerializeField] private float heightAboveGround = 0.35f;
    [SerializeField, Min(0.01f)] private float clearanceRadius = 0.4f;
    [SerializeField, Min(1)] private int spawnAttempts = 10;
    [SerializeField] private LayerMask blockingLayers;

    // Sólo registra los objetos de este generador, nunca agentes de la escena.
    private readonly List<PointOfInterest> spawnedObjects = new List<PointOfInterest>();
    private float spawnTimer;

    public int ActiveCount => CountActiveObjects();

    private void Awake()
    {
        InitializeSpawner();
    }

    public void Tick(Bounds area)
    {
        if (area == null || interestPrefab == null || objectsRoot == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        spawnTimer = spawnInterval;
        RemoveDestroyedObjects();

        if (ActiveCount >= maxActiveObjects) return;

        TrySpawnObject(area);
    }

    private void InitializeSpawner()
    {
        spawnTimer = spawnInterval;
        if (objectsRoot == null) return;

        // Incluye los objetos de prueba que ya estén en el contenedor asignado.
        spawnedObjects.AddRange(objectsRoot.GetComponentsInChildren<PointOfInterest>(true));
    }

    private int CountActiveObjects()
    {
        int count = 0;

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            PointOfInterest item = spawnedObjects[i];
            if (item != null && item.gameObject.activeInHierarchy)
                count++;
        }

        return count;
    }

    private void RemoveDestroyedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
                spawnedObjects.RemoveAt(i);
        }
    }

    private void TrySpawnObject(Bounds area)
    {
        for (int i = 0; i < spawnAttempts; i++)
        {
            Vector3 position = area.GetRandomPosition(heightAboveGround, clearanceRadius);
            if (!HasSpace(position)) continue;

            PointOfInterest item = Instantiate(
                interestPrefab, position, Quaternion.identity, objectsRoot);

            spawnedObjects.Add(item);
            return;
        }
    }

    private bool HasSpace(Vector3 position)
    {
        return !Physics.CheckSphere(
            position, clearanceRadius, blockingLayers, QueryTriggerInteraction.Collide);
    }
}
