using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Rounds Settings")]
    // An Inspector slider for the total number of rounds in this play session.
    [Range(1, 9)]
    public int totalRounds;

    public bool randomRounds = false;
    // The current round that the player is on.
    private int currentRound = -1;

    [Header("Time Delay Settings")]
    public bool timeDelay = false;
    public int timeBetweenRounds = 3;

    // The current state of each arm.
    [HideInInspector]
    public int leftArm = -1, rightArm = -1;

    public Dictionary<int, int> leftArmPositions;
    public Dictionary<int, int> rightArmPositions;

    // LATER: accessed from hitbox script.
    [HideInInspector]
    public bool lastRoundSuccess = true;

    [Header("Display Settings")]
    public GameObject UICanvas;
    private UI_Script UIManager;

    [Header("Hitboxes")]
    public Hitbox[] hitboxes;

    private void Start()
    {
        if (currentRound < 0)
        {
            currentRound = 0;
        }

        leftArmPositions = new Dictionary<int, int>();
        rightArmPositions = new Dictionary<int, int>();

        UIManager = UICanvas.GetComponent<UI_Script>();
        if (UIManager == null)
        {
            Debug.LogWarning("UIManager not found.");
        }

        if (randomRounds)
        {
            CreateLists();
        }

        if (timeDelay)
        {
            StartCoroutine(UpdateLeftArmPositions());
            StartCoroutine(UpdateRightArmPositions());
        }
        else
        {
            UpdateArmPositions(lastRoundSuccess);
        }
    }

    /// <summary>
    /// Optional function to randomly generate lists for number of rounds.
    /// </summary>
    private void CreateLists()
    {
        for (int i = 0; i < totalRounds; i++)
        {
            leftArmPositions[i] = Random.Range(-1, 27);
        }
        for (int j = 0; j < totalRounds; j++)
        {
            rightArmPositions[j] = Random.Range(-1, 27);
        }
        
        // Writes the created lists to the console.
        /**
        foreach (KeyValuePair<int,int> entry in leftArmPositions)
        {
            Debug.Log($"Round: {entry.Key+1}\nLeft Arm: {entry.Value}");
        }
        foreach(KeyValuePair<int,int> entry in rightArmPositions)
        {
            Debug.Log($"Round: {entry.Key+1}\nRight Arm: {entry.Value}");
        }
        **/
    }

    /// <summary>
    /// Updates leftArm after a number of seconds.
    /// </summary>
    IEnumerator UpdateLeftArmPositions()
    {
        foreach(KeyValuePair<int,int> entry in leftArmPositions)
        {
            leftArm = entry.Value;
            Debug.Log($"Left Arm: {leftArm}");
            UIManager.ShowLeftSymbol(leftArm);
            
            foreach (Hitbox hitbox in hitboxes)
            {
                if (hitbox.id == leftArm)
                {
                    hitbox.isActiveTarget = true;
                }
            }

            yield return new WaitForSeconds(timeBetweenRounds);
        }
    }

    /// <summary>
    /// Updates rightArm after a number of seconds.
    /// </summary>
    IEnumerator UpdateRightArmPositions()
    {
        foreach(KeyValuePair<int,int> entry in rightArmPositions)
        {
            rightArm = entry.Value;
            Debug.Log($"Right Arm: {rightArm}");
            UIManager.ShowRightSymbol(rightArm);

            foreach (Hitbox hitbox in hitboxes)
            {
                if (hitbox.id == leftArm)
                {
                    hitbox.isActiveTarget = true;
                }
            }

            yield return new WaitForSeconds(timeBetweenRounds);
        }
    }

    /// <summary>
    /// Updates leftArm and rightArm variables as the previous positions are met.
    /// </summary>
    /// <param name="lastRoundSuccess">True if the previous round was successfully completed; False if not.</param>
    public void UpdateArmPositions(bool lastRoundSuccess)
    {
        for (int i = 0;i < totalRounds; i++)
        {
            if (lastRoundSuccess)
            {
                leftArm = leftArmPositions[currentRound];
                UIManager.ShowLeftSymbol(leftArm);
                rightArm = rightArmPositions[currentRound];
                UIManager.ShowRightSymbol(rightArm);
                currentRound++;
            }
            Debug.Log($"Left Arm: {leftArm}");
            Debug.Log($"Right Arm: {rightArm}");
            lastRoundSuccess = true;    // Exists only until actual value is accessed from hitbox script.
        }
    }
}
