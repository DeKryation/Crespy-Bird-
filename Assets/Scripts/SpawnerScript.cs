// ============================================================
//  SpawnerScript.cs
// ============================================================
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    private GameObject SpawnObject;
    public GameObject[] SpawnObjects;
    public float timeMin = 0.7f;
    public float timeMax = 2f;

    [Header("Checkpoint Flag")]
    public GameObject checkpointFlagPrefab;

    void Start()
    {
        SpawnObject = SpawnObjects[Random.Range(0, SpawnObjects.Length)];
        Spawn();
    }

    void Spawn()
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            float pipeY = Random.Range(-0.5f, 1f);
            Vector3 pipePos = transform.position + new Vector3(0, pipeY, 0);
            GameObject pipe = Instantiate(SpawnObject, pipePos, Quaternion.identity);

            if (checkpointFlagPrefab != null
                && CheckpointManager.Instance != null
                && CheckpointManager.Instance.ReadyToSpawn)
            {
                Vector3 flagPos = FindGapCentre(pipe, pipePos);

                GameObject flag = Instantiate(checkpointFlagPrefab, flagPos, Quaternion.identity);
                flag.transform.SetParent(pipe.transform);

                CheckpointFlag cf = flag.GetComponent<CheckpointFlag>();
                if (cf != null)
                    cf.flagID = CheckpointManager.Instance.GetNextFlagID();

                CheckpointManager.Instance.OnFlagSpawned();
                Debug.Log("[SpawnerScript] Flag " + (cf != null ? cf.flagID : -1) +
                          " placed at gap centre " + flagPos);
            }
        }

        Invoke("Spawn", Random.Range(timeMin, timeMax));
    }

    Vector3 FindGapCentre(GameObject pipe, Vector3 fallback)
    {
        // Strategy: find all colliders tagged "Pipe" (the solid pipes),
        // get their world-space top and bottom edges, then the gap is between them.
        float topOfBottomPipe = float.MinValue;
        float bottomOfTopPipe = float.MaxValue;
        float pipeX = fallback.x;
        bool found = false;

        foreach (Collider2D col in pipe.GetComponentsInChildren<Collider2D>())
        {
            if (!col.CompareTag("Pipe")) continue;
            found = true;
            Bounds b = col.bounds;
            pipeX = b.center.x;

            // The pipe BELOW the gap has its TOP as the lower gap edge
            if (b.center.y < fallback.y)
                topOfBottomPipe = Mathf.Max(topOfBottomPipe, b.max.y);
            // The pipe ABOVE the gap has its BOTTOM as the upper gap edge
            else
                bottomOfTopPipe = Mathf.Min(bottomOfTopPipe, b.min.y);
        }

        if (found && topOfBottomPipe > float.MinValue && bottomOfTopPipe < float.MaxValue)
        {
            float gapCentreY = (topOfBottomPipe + bottomOfTopPipe) / 2f;
            Debug.Log($"[SpawnerScript] Gap: bottom pipe top={topOfBottomPipe:F2}, top pipe bottom={bottomOfTopPipe:F2}, centre={gapCentreY:F2}");
            return new Vector3(pipeX, gapCentreY, fallback.z);
        }

        // Fallback: use the Pipeblank collider centre
        foreach (Collider2D col in pipe.GetComponentsInChildren<Collider2D>())
        {
            if (!col.CompareTag("Pipeblank")) continue;
            Vector3 c = col.transform.TransformPoint(col.offset);
            Debug.Log("[SpawnerScript] Using Pipeblank centre fallback: " + c);
            return new Vector3(c.x, c.y, fallback.z);
        }

        Debug.LogWarning("[SpawnerScript] Could not find gap – using spawn position.");
        return fallback;
    }
}