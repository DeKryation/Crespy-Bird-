using UnityEngine;

//Checkpoint flag behaviour. Handle flag collection and missed flag.
public class CheckpointFlag : MonoBehaviour
{
    //Unique id assigned by the manager.
    [HideInInspector] public int flagID = -1;

    //Prevent the flag to be collected more than once.
    private bool collected = false;

    //Ensure the missed event is only reported once.
    private bool missedReported = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("[CheckpointFlag] Hit by: " + col.name + " tag: " + col.tag);

        //Stop if is already colelcted.
        if (collected) return;

        //Only active during gameplay.
        if (GameStateManager.GameState != GameState.Playing) return;

        //Only the player can collect the flag.
        if (!col.CompareTag("Flappy")) return;

        //Avoid duplicated collection.
        if (CheckpointManager.Instance != null &&
            CheckpointManager.Instance.IsFlagAlreadyCollected(flagID)) return;

        collected = true;

        //Send the player game object to the manager to save the checkpoint data.
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.SaveCheckpoint(col.gameObject, flagID);

        //Destroy the flag after collection.    
        Destroy(gameObject);
    }

    void Update()
    {
        //Ignore the flag if collected.
        if (collected) return;

        //Check during gameplay only.
        if (GameStateManager.GameState != GameState.Playing) return;


        //Convert flag position to viewport coordinates.
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);

        // If the flag has moved past the left edge of the screen
        // it means the player missed the checkpoint
        if (screenPos.x < 0f)
        {
            ReportMissed();
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // If the flag was not collected and not already reported missed
        if (!collected && !missedReported && GameStateManager.GameState == GameState.Playing)
            ReportMissed();
    }

    // Sends a "checkpoint missed" notification to the CheckpointManager
    void ReportMissed()
    {
        if (missedReported) return;
        missedReported = true;
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.OnFlagMissed();
    }
}