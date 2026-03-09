// ============================================================
//  CheckpointFlag.cs
// ============================================================
using UnityEngine;

public class CheckpointFlag : MonoBehaviour
{
    public GameObject collectEffectPrefab;

    [HideInInspector] public int flagID = -1;

    private bool collected = false;
    private bool missedReported = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("[CheckpointFlag] Hit by: " + col.name + " tag: " + col.tag);

        if (collected) return;
        if (GameStateManager.GameState != GameState.Playing) return;
        if (!col.CompareTag("Flappy")) return;

        if (CheckpointManager.Instance != null &&
            CheckpointManager.Instance.IsFlagAlreadyCollected(flagID)) return;

        collected = true;

        // Pass the whole player GameObject so manager can snapshot everything
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.SaveCheckpoint(col.gameObject, flagID);

        if (collectEffectPrefab != null)
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void Update()
    {
        if (collected) return;
        if (GameStateManager.GameState != GameState.Playing) return;

        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPos.x < 0f)
        {
            ReportMissed();
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (!collected && !missedReported && GameStateManager.GameState == GameState.Playing)
            ReportMissed();
    }

    void ReportMissed()
    {
        if (missedReported) return;
        missedReported = true;
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.OnFlagMissed();
    }
}