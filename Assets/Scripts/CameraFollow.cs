using UnityEngine;
using System.Collections;

//This script is used to make the camera move horizontally.
public class CameraFollow : MonoBehaviour {

    //Stores the camera 's z position so it doesn't change when the player moves.
    void Start () {
        cameraZ = transform.position.z; //Store player references
	}

    float cameraZ;


	void Update () {
        //Save the camera initial z position.
        transform.position = new Vector3(Player.position.x + 0.5f, 0, cameraZ);
       
	}

    
    public Transform Player;
}
