using UnityEngine;

//Handle spawning of entities in the game.
public class SpawnerScript : MonoBehaviour
{
    //For pipe.
    private GameObject SpawnObject;
    public GameObject[] SpawnObjects;

    //Maximum and minimum time between spawns.
    public float timeMin = 0.7f;
    public float timeMax = 2f;

    //For checkpoint flag.
    [Header("Checkpoint Flag")]
    public GameObject checkpointFlagPrefab;

    void Start()
    {
        //Rnadomly pick one of the pipe prefabs to spawn.
        SpawnObject = SpawnObjects[Random.Range(0, SpawnObjects.Length)];
        Spawn();
    }

    //Spawn entities.
    void Spawn()
    {
        //Only spawn in the playing state.
        if (GameStateManager.GameState == GameState.Playing)
        {
            // Random vertical offset for the pipe position
            float pipeY = Random.Range(-0.5f, 1f);
            Vector3 pipePos = transform.position + new Vector3(0, pipeY, 0);

            // Instantiate the pipe in the scene
            GameObject pipe = Instantiate(SpawnObject, pipePos, Quaternion.identity);

            //Spawn checkpoint flag.
            if (checkpointFlagPrefab != null
                && CheckpointManager.Instance != null
                && CheckpointManager.Instance.ReadyToSpawn)
            {
                //Find the centre of the gap between the pipe.
                Vector3 flagPos = FindGapCentre(pipe, pipePos);

                //Create the flag.
                GameObject flag = Instantiate(checkpointFlagPrefab, flagPos, Quaternion.identity);
                flag.transform.SetParent(pipe.transform);

                //Assign an id.
                CheckpointFlag cf = flag.GetComponent<CheckpointFlag>();
                if (cf != null)
                    cf.flagID = CheckpointManager.Instance.GetNextFlagID();

                //Tell the manager about the new flag.
                CheckpointManager.Instance.OnFlagSpawned();
                Debug.Log("[SpawnerScript] Flag " + (cf != null ? cf.flagID : -1) +
                          " placed at gap centre " + flagPos);
            }
        }

        //Schedule the next spawn.
        Invoke("Spawn", Random.Range(timeMin, timeMax));
    }

    //Calculate the vertical centre of the gap between top and bottom pipes.
    Vector3 FindGapCentre(GameObject pipe, Vector3 fallback)
    {
       
        float topOfBottomPipe = float.MinValue;
        float bottomOfTopPipe = float.MaxValue;
        float pipeX = fallback.x;
        bool found = false;

        //Loop through all child colliders to locate the pipes.
        foreach (Collider2D col in pipe.GetComponentsInChildren<Collider2D>())
        {
            if (!col.CompareTag("Pipe")) continue;
            found = true;
            Bounds b = col.bounds;
            pipeX = b.center.x;

            //The pipe BELOW the gap has its TOP as the lower gap edge
            if (b.center.y < fallback.y)
                topOfBottomPipe = Mathf.Max(topOfBottomPipe, b.max.y);
            //The pipe ABOVE the gap has its BOTTOM as the upper gap edge
            else
                bottomOfTopPipe = Mathf.Min(bottomOfTopPipe, b.min.y);
        }

        // If both top and bottom edges are found, calculate the gap centre
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
          //  Debug.Log("[SpawnerScript] Using Pipeblank centre fallback: " + c);
            return new Vector3(c.x, c.y, fallback.z);
        }

        //Debug.LogWarning("[SpawnerScript] Could not find gap – using spawn position.");
        return fallback;
    }
}