using UnityEngine;
using System.Collections;

public class ScoreManagerScript : MonoBehaviour {

    //To access and modify the score.
    public static int Score { get; set; }

	// Use this for initialization
	void Start () {

        //Only the units digit is visible when the score is below 10.
        (Tens.gameObject as GameObject).SetActive(false);
        (Hundreds.gameObject as GameObject).SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {

        if (previousScore != Score) //Update Ui when the score changes.
        { 
            if(Score < 10) //If score is less than 10, displayt he units digits.
            {
                //just draw units
                Units.sprite = numberSprites[Score];
            }
            else if(Score >= 10 && Score < 100)
            {
                (Tens.gameObject as GameObject).SetActive(true); //Enable the tens digit.
                Tens.sprite = numberSprites[Score / 10]; //Tens digit is obstained by dividing by 10.
                Units.sprite = numberSprites[Score % 10]; //Units digits is the remainder after division.
            }
            else if(Score >= 100) //If the score is 100 or more Enable the hundreds digit.
            {
                (Hundreds.gameObject as GameObject).SetActive(true);
                Hundreds.sprite = numberSprites[Score / 100];
                int rest = Score % 100;
                Tens.sprite = numberSprites[rest / 10];
                Units.sprite = numberSprites[rest % 10];
            }
        }

	}

    //Store the previous score value to detect changes.
    int previousScore = -1;

    //Array of number of sprittes to display the digits.
    public Sprite[] numberSprites;

    //Sprite rednerers for each digit position.
    public SpriteRenderer Units, Tens, Hundreds;
}
