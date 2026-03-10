using UnityEngine;
using System.Collections;

public class ScoreManagerScript : MonoBehaviour
{
    public static int Score { get; set; }

    int previousScore = -1;

    public Sprite[] numberSprites;
    public SpriteRenderer Units, Tens, Hundreds;

    void Start()
    {
        Score = 0;
        (Tens.gameObject as GameObject).SetActive(false);
        (Hundreds.gameObject as GameObject).SetActive(false);
        RefreshDisplay();
    }

    void Update()
    {
        if (previousScore != Score)
            RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        previousScore = Score;

        if (Score < 10)
        {
            (Tens.gameObject as GameObject).SetActive(false);
            (Hundreds.gameObject as GameObject).SetActive(false);
            Units.sprite = numberSprites[Score];
        }
        else if (Score < 100)
        {
            (Hundreds.gameObject as GameObject).SetActive(false);
            (Tens.gameObject as GameObject).SetActive(true);
            Tens.sprite = numberSprites[Score / 10];
            Units.sprite = numberSprites[Score % 10];
        }
        else
        {
            (Hundreds.gameObject as GameObject).SetActive(true);
            (Tens.gameObject as GameObject).SetActive(true);
            Hundreds.sprite = numberSprites[Score / 100];
            int rest = Score % 100;
            Tens.sprite = numberSprites[rest / 10];
            Units.sprite = numberSprites[rest % 10];
        }
    }
}