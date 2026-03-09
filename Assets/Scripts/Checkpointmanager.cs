using UnityEngine;
using TMPro;
using System.Collections.Generic;

//Manage the whole checkpoint system, including saving/restoring snapshots and win conditions.
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("Win Settings")]
    public int flagsToWin = 5;
    public float gracePeriod = 10f;

    [Header("UI")]
    public GameObject winGUI;
    public GameObject deathGUI;
    public TMP_Text flagCountText;

    public bool ReadyToSpawn { get; private set; }
    public bool FlagIsActive { get; private set; }
    public int FlagsCollected { get; private set; }
    public bool HasCheckpoint { get; private set; }

    private float timer = 0f;

    private bool flagMissedSinceLastCheckpoint = false;
    private int nextFlagID = 0;
    private HashSet<int> collectedFlagIDs = new HashSet<int>();

    private GameSnapshot savedSnapshot;
    private bool hasSnapshot = false;

    //Store the state of the single pipe when the checkpoin is saved.
    public struct PipeSnapshot
    {
        public Vector3 position;
        public Quaternion rotation;
        public int prefabIndex;   // index into Birdbird.pipePrefabs
    }

    //Store the gaem state.
    public struct GameSnapshot
    {
        public Vector3 playerPosition;
        public int score;
        public int flagsCollected;
        public List<PipeSnapshot> pipes;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    //When this script run, reset all checkpoint-related variables to their default states.
    void Start() { ResetCheckpoints(); }

    void Update()
    {
        //Run checkpoint timer during gameplay.
        if (GameStateManager.GameState != GameState.Playing) return;


        //If no flag is currently active and the system is not ready to spawn another, continue the grace period.
        if (!FlagIsActive && !ReadyToSpawn)
        {
            timer += Time.deltaTime;
            if (timer >= gracePeriod)
            {
                ReadyToSpawn = true;
                Debug.Log("[CheckpointManager] Ready to spawn next flag.");
            }
        }
    }

    //Flag Management.
    public int GetNextFlagID() { return nextFlagID++; }

    public bool IsFlagAlreadyCollected(int id) { return collectedFlagIDs.Contains(id); }

    public void OnFlagSpawned()
    {
        FlagIsActive = true;
        ReadyToSpawn = false;
        timer = 0f;
    }

    //Checkpoint Saving
    public void SaveCheckpoint(GameObject player, int flagID)
    {
        if (collectedFlagIDs.Contains(flagID)) return;
        collectedFlagIDs.Add(flagID);

        GameSnapshot snap = new GameSnapshot();

        //Save player position.
        snap.playerPosition = player.transform.position;

        //Save score.
        snap.score = ScoreManagerScript.Score;

        //Save flag count.
        snap.flagsCollected = FlagsCollected + 1;

        //Snapshot all pipe roots currently in scene
        snap.pipes = new List<PipeSnapshot>();


        HashSet<int> seenRoots = new HashSet<int>();
        Birdbird bird = player.GetComponent<Birdbird>();

        foreach (GameObject pipeChild in GameObject.FindGameObjectsWithTag("Pipe"))
        {
            GameObject root = pipeChild.transform.root.gameObject;
            if (seenRoots.Contains(root.GetInstanceID())) continue;
            seenRoots.Add(root.GetInstanceID());

            //Match root to a prefab index by name
            int prefabIdx = FindPrefabIndex(root, bird);
            if (prefabIdx < 0) continue;

            snap.pipes.Add(new PipeSnapshot
            {
                position = root.transform.position,
                rotation = root.transform.rotation,
                prefabIndex = prefabIdx
            });
        }

        //Store snapshot.
        savedSnapshot = snap;
        hasSnapshot = true;

        //Update checkpoint stats.
        FlagsCollected++;
        HasCheckpoint = true;
        FlagIsActive = false;

        //Clear missed flag state.
        flagMissedSinceLastCheckpoint = false;
        timer = 0f;

        Debug.Log("[CheckpointManager] Snapshot saved! Flags: " + FlagsCollected + "/" + flagsToWin);
        UpdateFlagUI();

        if (FlagsCollected >= flagsToWin)
            TriggerWin();
    }

    //Pipe identification.
    private int FindPrefabIndex(GameObject root, Birdbird bird)
    {
        if (bird == null || bird.pipePrefabs == null) return -1;
        for (int i = 0; i < bird.pipePrefabs.Length; i++)
        {
            if (bird.pipePrefabs[i] == null) continue;
            // Unity appends "(Clone)" to instantiated objects
            if (root.name.StartsWith(bird.pipePrefabs[i].name))
                return i;
        }
        return -1;
    }

    //Missed checkpoint.
    public void OnFlagMissed()
    {
        FlagIsActive = false;
        flagMissedSinceLastCheckpoint = true;
        timer = 0f;
        Debug.Log("[CheckpointManager] Flag missed.");
    }

    //Called by Birdbird on death. Returns true + snapshot if player should respawn.
    public bool HandleDeath(out GameSnapshot snapshot)
    {
        snapshot = savedSnapshot;

        if (!HasCheckpoint || !hasSnapshot)
        {
            Debug.Log("[CheckpointManager] No checkpoint – Game Over.");
            ResetCheckpoints();
            return false;
        }

        if (flagMissedSinceLastCheckpoint)
        {
            Debug.Log("[CheckpointManager] Missed flag since last checkpoint – Game Over.");
            ResetCheckpoints();
            return false;
        }

        FlagIsActive = false;
        timer = 0f;
        ReadyToSpawn = false;

        Debug.Log("[CheckpointManager] Restoring snapshot.");
        return true;
    }

    public void ResetCheckpoints()
    {
        timer = 0f;
        FlagIsActive = false;
        ReadyToSpawn = false;
        HasCheckpoint = false;
        FlagsCollected = 0;
        flagMissedSinceLastCheckpoint = false;
        hasSnapshot = false;
        nextFlagID = 0;
        collectedFlagIDs.Clear();
        UpdateFlagUI();
    }

    void TriggerWin()
    {
        GameStateManager.GameState = GameState.Won;
        Debug.Log("[CheckpointManager] Player WON!");
        if (winGUI != null) winGUI.SetActive(true);
        if (deathGUI != null) deathGUI.SetActive(false);
    }

    void UpdateFlagUI()
    {
        if (flagCountText != null)
            flagCountText.text = FlagsCollected + " / " + flagsToWin;
        else
            Debug.LogWarning("[CheckpointManager] flagCountText is not assigned in the Inspector!");
    }
}