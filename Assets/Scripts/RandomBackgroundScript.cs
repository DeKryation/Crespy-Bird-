using UnityEngine;
using System.Collections;

public class RandomBackgroundScript : MonoBehaviour {


    // Get the SpriteRenderer component attached to this object
    // Then assign a random sprite from the Backgrounds array
    // Random.Range selects an index between 0 and the array length

    void Start () {
        (GetComponent<Renderer>() as SpriteRenderer).sprite = Backgrounds[Random.Range(0, Backgrounds.Length)];
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Assigned in inspector to store multiple background sprites for random selection.
    public Sprite[] Backgrounds;
}
