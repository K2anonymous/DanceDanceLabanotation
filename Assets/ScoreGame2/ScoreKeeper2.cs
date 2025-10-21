using UnityEngine;

public class ScoreKeeper2 : MonoBehaviour
{
    private int score;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(score);
    }

    public void GoalScored(int points)
    {
        score += points; // identical to score + points
        Debug.Log(score);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
