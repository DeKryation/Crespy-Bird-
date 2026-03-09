using UnityEngine;
using System.Collections;


public class Birdbird : MonoBehaviour
{

    //Audio Clips for actions.
    public AudioClip FlyAudioClip, DeathAudioClip, ScoredAudioClip;

    //prites for actions.
    public Sprite GetReadySprite;

    //Rotation Speed.
    public float RotateUpSpeed = 1, RotateDownSpeed = 1;

    //UI References.
    public GameObject IntroGUI, DeathGUI;

    //Collider for restart button in death screen.
    public Collider2D restartButtonGameCollider;

    //Speed 
    public float VelocityPerJump = 3;
    public float XSpeed = 1;

    void Start()
    {

    }

    //State to see if its flying/falling.
    YAxisTravelState flappyYAxisTravelState;

    enum YAxisTravelState
    {
        GoingUp, GoingDown
    }

    //Store the current rotation.
    Vector3 birdRotation = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        //handle back key in Windows Phone
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //Intro State.
        if (GameStateManager.GameState == GameState.Intro)
        {
            OnXAxis();
            if (OnTouch()) //If player taps, start the game.
            { 
                OnYAxis(); //Make the bird jump.
                GameStateManager.GameState = GameState.Playing; //Set the state to playing.

                //Hide intro and reset the score.
                IntroGUI.SetActive(false);
                ScoreManagerScript.Score = 0;
            }
        }

        else if (GameStateManager.GameState == GameState.Playing) //Gameplay state is playing.
        {
            OnXAxis();
            if (OnTouch()) //Tap to jump.
            {
                OnYAxis();
            }

        }

        else if (GameStateManager.GameState == GameState.Dead) //Gameplay state is dead.
        {
            Vector2 contactPoint = Vector2.zero;

            //Detect touch or mouse click.
            if (Input.touchCount > 0)
                contactPoint = Input.touches[0].position;
            if (Input.GetMouseButtonDown(0))
                contactPoint = Input.mousePosition;

            //check if user wants to restart the game
           /* if (restartButtonGameCollider == Physics2D.OverlapPoint
                (Camera.main.ScreenToWorldPoint(contactPoint)))
            {
                GameStateManager.GameState = GameState.Intro;
                Application.LoadLevel(Application.loadedLevelName);
            } */
        }

    }


    void FixedUpdate()
    {
        //Set the game state to intro.
        if (GameStateManager.GameState == GameState.Intro)
        {
            //If the bird falls too fast, apply upward force.
            if (GetComponent<Rigidbody2D>().linearVelocity.y < -1)
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, GetComponent<Rigidbody2D>().mass * 5500 * Time.deltaTime)); 
        }
        else if (GameStateManager.GameState == GameState.Playing || GameStateManager.GameState == GameState.Dead) //Adjust the rotation.
        {
            FixFlappyRotation();
        }
    }

    //Detect the player input.
    bool OnTouch()
    {
        if (Input.GetButtonUp("Jump") || Input.GetMouseButtonDown(0) || 
            (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
            return true;
        else
            return false;
    }

    //Handle horizontal movement.
    void OnXAxis()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0, 0);
    }

    //Handle jumping movement.
    void OnYAxis()
    {
        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, VelocityPerJump);
        GetComponent<AudioSource>().PlayOneShot(FlyAudioClip);
    }


    //Adjust the rotation.
    private void FixFlappyRotation()
    {
        if (GetComponent<Rigidbody2D>().linearVelocity.y > 0) flappyYAxisTravelState = YAxisTravelState.GoingUp;
        else flappyYAxisTravelState = YAxisTravelState.GoingDown;

        float degreesToAdd = 0;

        switch (flappyYAxisTravelState)
        {
            case YAxisTravelState.GoingUp:
                degreesToAdd = 6 * RotateUpSpeed;
                break;
            case YAxisTravelState.GoingDown:
                degreesToAdd = -3 * RotateDownSpeed;
                break;
            default:
                break;
        }

        //Clamp the rotation.
        birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z + degreesToAdd, -90, 45));
        transform.eulerAngles = birdRotation;
    }

    //Detect collision with pipes and floor.
    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Pipeblank") //An empty space between 2 pipes.
            {
                GetComponent<AudioSource>().PlayOneShot(ScoredAudioClip);
                ScoreManagerScript.Score++;
            }
            else if (col.gameObject.tag == "Pipe")
            {
                onDeath();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Floor")
            {
                onDeath();
            }
        }
    }

    //Handle death logic.
    void onDeath()
    {
        GameStateManager.GameState = GameState.Dead;
        DeathGUI.SetActive(true);
        GetComponent<AudioSource>().PlayOneShot(DeathAudioClip);
    }

}
