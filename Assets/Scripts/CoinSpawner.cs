using UnityEngine;
using System.Collections.Generic;

public class CoinSpawner : MonoBehaviour
{
    [Header("Basics")]
    public GameObject Coin;          // assign the COIN prefab (not the spawner!)
    public int count = 15;                 // how many coins
    public Vector3 areaSize = new Vector3(40f, 0f, 40f); // XZ area centered on this object
    public float y = 0.5f;                 // height to place coins at

    [Header("Simple rules")]
    public float minSpacing = 1.2f;        // keep coins apart
    public LayerMask obstacleMask;         // optional: set to your Walls layer (leave 0 to ignore)

    [Header("Optional")]
    public bool randomizeColor = true;     // needs Coin.cs on the prefab
    public Transform parentForCoins;       // keep hierarchy tidy

    void Start()
    {
        // --- Guard rails (tiny but important) ---
        if (!Coin) { Debug.LogError("[CoinSpawner] Assign coinPrefab."); return; }
        if (Coin.GetComponent<CoinSpawner>())
        {
            Debug.LogError("[CoinSpawner] coinPrefab is a SPAWNER. Assign the COIN prefab, not this object.");
            return;
        }

        // Make sure coin will be visible & collectible
        var col = Coin.GetComponent<Collider>();
        if (col) col.isTrigger = true;

        List<Vector3> placed = new List<Vector3>();
        int spawned = 0, attempts = 0, maxAttempts = count * 30;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;

            // Random spot in a box on XZ
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                y,
                Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
            );

            // Optional: avoid walls/obstacles
            if (obstacleMask.value != 0 && Physics.CheckSphere(pos, 0.35f, obstacleMask))
                continue;

            // Keep spacing from other coins
            bool ok = true;
            foreach (var p in placed)
                if ((pos - p).sqrMagnitude < (minSpacing * minSpacing)) { ok = false; break; }
            if (!ok) continue;

            // Spawn
            var go = Instantiate(Coin, pos, Quaternion.identity, parentForCoins);
            go.tag = "Coin";
            if (go.TryGetComponent(out Collider instCol)) instCol.isTrigger = true;

            if (randomizeColor && go.TryGetComponent(out Coin c)) c.RandomizeColor();

            placed.Add(pos);
            spawned++;
        }

        if (spawned < count)
            Debug.LogWarning($"[CoinSpawner] Spawned {spawned}/{count}. Increase areaSize or lower minSpacing.");
        else
            Debug.Log($"[CoinSpawner] Spawned {spawned} coins.");
    }

    // just a simple visual helper
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.05f, new Vector3(areaSize.x, 0.1f, areaSize.z));
    }

    public void Respawn(int newCount)
    {
        count = newCount;

        var existing = GameObject.FindGameObjectsWithTag("Coin");
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i].scene.IsValid())
                Destroy(existing[i]);
        }

        List<Vector3> placed = new List<Vector3>();
        int spawned = 0, attempts = 0, maxAttempts = count * 30;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 pos = transform.position + new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                y,
                Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
            );

            if (obstacleMask.value != 0 && Physics.CheckSphere(pos, 0.35f, obstacleMask))
                continue;

            bool ok = true;
            for (int j = 0; j < placed.Count; j++)
                if ((pos - placed[j]).sqrMagnitude < (minSpacing * minSpacing)) { ok = false; break; }
            if (!ok) continue;

            var go = Instantiate(Coin, pos, Quaternion.identity, parentForCoins);
            go.tag = "Coin";
            if (go.TryGetComponent(out Collider instCol)) instCol.isTrigger = true;
            if (randomizeColor && go.TryGetComponent(out Coin c)) c.RandomizeColor();

            placed.Add(pos);
            spawned++;
        }

        if (spawned < count)
            Debug.LogWarning($"[CoinSpawner] Respawned {spawned}/{count}. Increase areaSize or lower minSpacing.");
        else
            Debug.Log($"[CoinSpawner] Respawned {spawned} coins.");
    }

}
