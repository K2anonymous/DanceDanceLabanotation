using UnityEngine;
using UnityEngine.SceneManagement;

public class RealSceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"GameSceneManager: Index {sceneIndex} is out of range.");
        }
    }

    //public void Test(int number)
    //{
    //    Debug.Log(number);
    //}

    public void QuitGame()
    {
        Debug.Log("GameSceneManager: Quitting Application...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
