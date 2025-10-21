using UnityEngine;


public class ScoringBall2 : MonoBehaviour
{
    public ScoreKeeper2 scoreKeeper;

    private void OnCollisionEnter(Collision collision)
    {
        scoreKeeper.GoalScored(3);
    }
}
