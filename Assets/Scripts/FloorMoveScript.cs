using UnityEngine;
using System.Collections;

public class FloorMoveScript : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //If the player is dead, stop moving the floor.
        if (GameStateManager.GameState == GameState.Dead)
            return;

        //If the player moved too far left, reset the floor's position to the right.    
        if (transform.localPosition.x < -3.9f)
        {
            transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
        }
        //Move the floor left smoothyl.
        transform.Translate(-Time.deltaTime, 0, 0);
    }


}
