using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Required for TextMeshPro

public class RealSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The text component that displays button descriptions.")]
    public TextMeshProUGUI descriptionText;

    [Tooltip("The default text to show when nothing is hovered.")]
    [TextArea(2, 3)]
    public string defaultDescription = "Select an option...";

    [Header("Audio (Optional)")]
    public AudioSource uiAudioSource;
    public AudioClip clickSound;
    public AudioClip hoverSound;

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

    public void OpenPanel(GameObject panelToOpen)
    {
        PlayClickSound();
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }

    public void ClosePanel(GameObject panelToClose)
    {
        PlayClickSound();
        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
        }
    }

    // --- HOVER DESCRIPTION FUNCTIONS (Call these via EventTriggers) ---

    /// <summary>
    /// Call this from an EventTrigger (PointerEnter).
    /// Type the description text directly into the inspector box.
    /// </summary>
    public void SetDescription(string text)
    {
        if (descriptionText != null)
        {
            descriptionText.text = text;
        }
        PlayHoverSound();
    }

    /// <summary>
    /// Call this from an EventTrigger (PointerExit).
    /// </summary>
    public void ClearDescription()
    {
        if (descriptionText != null)
        {
            descriptionText.text = defaultDescription;
        }
    }

    // --- AUDIO HELPER ---

    private void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    private void PlayHoverSound()
    {
        if (uiAudioSource != null && hoverSound != null)
        {
            uiAudioSource.PlayOneShot(hoverSound);
        }
    }
}
