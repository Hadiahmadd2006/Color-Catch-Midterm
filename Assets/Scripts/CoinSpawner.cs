using UnityEngine;
using System.Collections.Generic;

public class CoinSpawner : MonoBehaviour
{
    [Header("Basics")]
    public GameObject Coin;          
    public int count = 15;                 
    public Vector3 areaSize = new Vector3(40f, 0f, 40f); 
    public float y = 0.5f;                 

    [Header("Simple rules")]
    public float minSpacing = 1.2f;        
    public LayerMask obstacleMask;         

    [Header("Optional")]
    public bool randomizeColor = true;     
    public Transform parentForCoins;       

    void Start()
    {
        if (!Coin) { Debug.LogError("[CoinSpawner] Assign coinPrefab."); return; }
        if (Coin.GetComponent<CoinSpawner>())
        {
            Debug.LogError("[CoinSpawner] coinPrefab is a SPAWNER. Assign the COIN prefab, not this object.");
            return;
        }

        var col = Coin.GetComponent<Collider>();
        if (col) col.isTrigger = true;

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
            foreach (var p in placed)
                if ((pos - p).sqrMagnitude < (minSpacing * minSpacing)) { ok = false; break; }
            if (!ok) continue;

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
