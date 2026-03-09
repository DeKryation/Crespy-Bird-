using UnityEngine;
using System.Collections;

public class SpawnerScript : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        // Select a random pipe prefab from the available spawn objects

        SpawnObject = SpawnObjects[Random.Range(0, SpawnObjects.Length)];
        Spawn();
    }

    void Spawn()
    {
        //Only span pipes when the game is in the play state.
        if (GameStateManager.GameState == GameState.Playing)
        {
            //Generate a random vertical position.
            float y = Random.Range(-0.5f, 1f);

            //Instantiate the pipe at the spawner's position with the random vertical position.
            GameObject go = Instantiate(SpawnObject, this.transform.position + new Vector3(0, y, 0), Quaternion.identity) as GameObject;
        }
        //Call the spawn again after a delay.
        Invoke("Spawn", Random.Range(timeMin, timeMax));
    }

    //Store the possible pipe prefabs.
    private GameObject SpawnObject;
    public GameObject[] SpawnObjects;

    //Min and max time between spawns.
    public float timeMin = 0.7f;
    public float timeMax = 2f;
}
