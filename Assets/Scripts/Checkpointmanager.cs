using UnityEngine;
using TMPro;
using System.Collections.Generic;

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
    // BUG FIX: flagWasMissed was never cleared after a successful checkpoint,
    // meaning one missed flag would permanently lock out respawning.
    // Now it only blocks respawn if a flag was missed AFTER the last checkpoint.
    private bool flagMissedSinceLastCheckpoint = false;
    private int nextFlagID = 0;
    private HashSet<int> collectedFlagIDs = new HashSet<int>();

    private GameSnapshot savedSnapshot;
    private bool hasSnapshot = false;

    public struct PipeSnapshot
    {
        public Vector3 position;
        public Quaternion rotation;
        public int prefabIndex;   // index into Birdbird.pipePrefabs
    }

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

    void Start() { ResetCheckpoints(); }

    void Update()
    {
        if (GameStateManager.GameState != GameState.Playing) return;

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

    public int GetNextFlagID() { return nextFlagID++; }

    public bool IsFlagAlreadyCollected(int id) { return collectedFlagIDs.Contains(id); }

    public void OnFlagSpawned()
    {
        FlagIsActive = true;
        ReadyToSpawn = false;
        timer = 0f;
    }

    // Called by CheckpointFlag when player touches it.
    public void SaveCheckpoint(GameObject player, int flagID)
    {
        if (collectedFlagIDs.Contains(flagID)) return;
        collectedFlagIDs.Add(flagID);

        GameSnapshot snap = new GameSnapshot();

        snap.playerPosition = player.transform.position;
        snap.score = ScoreManagerScript.Score;
        snap.flagsCollected = FlagsCollected + 1;

        // Snapshot all pipe roots currently in scene
        snap.pipes = new List<PipeSnapshot>();
        HashSet<int> seenRoots = new HashSet<int>();
        Birdbird bird = player.GetComponent<Birdbird>();

        foreach (GameObject pipeChild in GameObject.FindGameObjectsWithTag("Pipe"))
        {
            GameObject root = pipeChild.transform.root.gameObject;
            if (seenRoots.Contains(root.GetInstanceID())) continue;
            seenRoots.Add(root.GetInstanceID());

            // Match root to a prefab index by name
            int prefabIdx = FindPrefabIndex(root, bird);
            if (prefabIdx < 0) continue;

            snap.pipes.Add(new PipeSnapshot
            {
                position = root.transform.position,
                rotation = root.transform.rotation,
                prefabIndex = prefabIdx
            });
        }

        savedSnapshot = snap;
        hasSnapshot = true;

        FlagsCollected++;
        HasCheckpoint = true;
        FlagIsActive = false;

        // BUG FIX: clear the missed flag when a new checkpoint is earned
        flagMissedSinceLastCheckpoint = false;
        timer = 0f;

        Debug.Log("[CheckpointManager] Snapshot saved! Flags: " + FlagsCollected + "/" + flagsToWin);
        UpdateFlagUI();

        if (FlagsCollected >= flagsToWin)
            TriggerWin();
    }

    // Attempt to match an instantiated pipe root to a prefab by name prefix.
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

    public void OnFlagMissed()
    {
        FlagIsActive = false;
        // BUG FIX: only set flag missed relative to current checkpoint, not globally
        flagMissedSinceLastCheckpoint = true;
        timer = 0f;
        Debug.Log("[CheckpointManager] Flag missed.");
    }

    // Called by Birdbird on death. Returns true + snapshot if player should respawn.
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
        // Do NOT reset flagMissedSinceLastCheckpoint here; it's cleared on SaveCheckpoint.

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