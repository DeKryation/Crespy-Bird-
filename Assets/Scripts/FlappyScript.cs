// ============================================================
//  Birdbird.cs
//  Restores full game state snapshot on respawn.
// ============================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Birdbird : MonoBehaviour
{
    public AudioClip FlyAudioClip;
    public AudioClip DeathAudioClip;
    public AudioClip ScoredAudioClip;
    public Sprite GetReadySprite;

    public float VelocityPerJump = 3f;
    public float XSpeed = 1f;
    public float RotateUpSpeed = 1f;
    public float RotateDownSpeed = 1f;

    public GameObject IntroGUI;
    public GameObject DeathGUI;
    public Collider2D restartButtonGameCollider;

    [Header("Respawn")]
    public float respawnInvincibilityDuration = 1.5f;

    // ── Pipe prefabs to re-instantiate on restore ─────────────
    [Header("Pipe Prefabs (for snapshot restore)")]
    [Tooltip("Drag all your pipe prefabs here in the SAME ORDER as SpawnObjects on the Spawner.")]
    public GameObject[] pipePrefabs;

    private enum YAxisTravelState { GoingUp, GoingDown }
    private YAxisTravelState yState = YAxisTravelState.GoingDown;
    private Vector3 birdRotation = Vector3.zero;
    private bool isInvincible = false;

    void Start() { }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (GameStateManager.GameState == GameState.Intro)
        {
            OnXAxis();
            if (OnTouch())
            {
                OnYAxis();
                GameStateManager.GameState = GameState.Playing;
                IntroGUI.SetActive(false);
                ScoreManagerScript.Score = 0;
            }
        }
        else if (GameStateManager.GameState == GameState.Playing)
        {
            OnXAxis();
            if (OnTouch()) OnYAxis();
        }
    }

    void FixedUpdate()
    {
        if (GameStateManager.GameState == GameState.Intro)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb.linearVelocity.y < -1f)
                rb.AddForce(new Vector2(0, rb.mass * 5500f * Time.deltaTime));
        }
        else if (GameStateManager.GameState == GameState.Playing ||
                 GameStateManager.GameState == GameState.Dead)
        {
            FixFlappyRotation();
        }
    }

    bool OnTouch()
    {
        return Input.GetButtonUp("Jump")
            || Input.GetMouseButtonDown(0)
            || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended);
    }

    void OnXAxis()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0f, 0f);
    }

    void OnYAxis()
    {
        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0f, VelocityPerJump);
        GetComponent<AudioSource>().PlayOneShot(FlyAudioClip);
    }

    void FixFlappyRotation()
    {
        yState = GetComponent<Rigidbody2D>().linearVelocity.y > 0f
            ? YAxisTravelState.GoingUp : YAxisTravelState.GoingDown;

        float deg = (yState == YAxisTravelState.GoingUp)
            ? 6f * RotateUpSpeed : -3f * RotateDownSpeed;

        birdRotation = new Vector3(0f, 0f,
            Mathf.Clamp(birdRotation.z + deg, -90f, 45f));
        transform.eulerAngles = birdRotation;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState != GameState.Playing) return;
        if (isInvincible) return;

        if (col.gameObject.CompareTag("Pipeblank"))
        {
            GetComponent<AudioSource>().PlayOneShot(ScoredAudioClip);
            ScoreManagerScript.Score++;
        }
        else if (col.gameObject.CompareTag("Pipe"))
        {
            OnDeath();
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState != GameState.Playing) return;
        if (isInvincible) return;
        if (col.gameObject.CompareTag("Floor")) OnDeath();
    }

    void OnDeath()
    {
        GetComponent<AudioSource>().PlayOneShot(DeathAudioClip);

        CheckpointManager.GameSnapshot snapshot = default(CheckpointManager.GameSnapshot);
        bool shouldRespawn = (CheckpointManager.Instance != null)
                          && CheckpointManager.Instance.HandleDeath(out snapshot);

        if (shouldRespawn)
            StartCoroutine(RespawnRoutine(snapshot));
        else
        {
            GameStateManager.GameState = GameState.Dead;
            if (DeathGUI != null) DeathGUI.SetActive(true);
        }
    }

    IEnumerator RespawnRoutine(CheckpointManager.GameSnapshot snap)
    {
        GameStateManager.GameState = GameState.Dead;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;

        // ── 1. Destroy all current pipes ─────────────────────
        HashSet<int> destroyed = new HashSet<int>();
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Pipe"))
        {
            GameObject root = p.transform.root.gameObject;
            if (!destroyed.Contains(root.GetInstanceID()))
            {
                destroyed.Add(root.GetInstanceID());
                Destroy(root);
            }
        }
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Pipeblank"))
        {
            GameObject root = p.transform.root.gameObject;
            if (!destroyed.Contains(root.GetInstanceID()))
            {
                destroyed.Add(root.GetInstanceID());
                Destroy(root);
            }
        }

        // Wait one frame for Destroy to flush, then a short visual pause
        yield return null;
        yield return new WaitForSeconds(0.4f);

        // ── 2. Reset score to 0 ───────────────────────────────
        ScoreManagerScript.Score = 0;
        FindObjectOfType<ScoreManagerScript>()?.RefreshDisplay();

        // ── 3. Re-instantiate pipes from snapshot ─────────────
        // BUG FIX: the original code destroyed pipes but never put them back.
        if (snap.pipes != null && pipePrefabs != null)
        {
            foreach (CheckpointManager.PipeSnapshot ps in snap.pipes)
            {
                if (ps.prefabIndex < 0 || ps.prefabIndex >= pipePrefabs.Length) continue;
                if (pipePrefabs[ps.prefabIndex] == null) continue;
                Instantiate(pipePrefabs[ps.prefabIndex], ps.position, ps.rotation);
            }
        }
        else if (pipePrefabs == null || pipePrefabs.Length == 0)
        {
            Debug.LogWarning("[Birdbird] pipePrefabs is empty – assign pipe prefabs in the Inspector!");
        }

        // ── 4. Restore player position & physics ─────────────
        transform.position = snap.playerPosition;
        birdRotation = Vector3.zero;
        transform.eulerAngles = birdRotation;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // ── 4b. Reset checkpoint state so player must re-earn the next one ──
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.ResetCheckpoints();

        // ── 5. Resume ─────────────────────────────────────────
        GameStateManager.GameState = GameState.Playing;

        // ── 6. Invincibility frames ───────────────────────────
        isInvincible = true;
        yield return new WaitForSeconds(respawnInvincibilityDuration);
        isInvincible = false;

        Debug.Log("[Birdbird] Respawn complete. Score=" + snap.score +
                  " Pipes restored=" + (snap.pipes != null ? snap.pipes.Count : 0));
    }
}