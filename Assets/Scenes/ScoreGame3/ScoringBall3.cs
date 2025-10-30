using JetBrains.Annotations;
using UnityEngine;

public class ScoringBall3 : MonoBehaviour
{
    public ScoreKeeper3 scoreKeeper;

    private void OnCollisionEnter(Collision collision)
    {
        scoreKeeper.GoalScored(3);
        Debug.Log("Goal Scored!");
    }





}