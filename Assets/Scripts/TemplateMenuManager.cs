using UnityEngine;
using UnityEngine.SceneManagement; // Required for changing scenes
using TMPro; // Required for TextMeshPro

public class TemplateMenuManager : MonoBehaviour
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

    // --- INITIALIZATION ---

    private void Start()
    {
        // Ensure the description is reset when the game starts
        ClearDescription();
    }

    // --- TEMPLATE FUNCTIONS FOR BUTTONS (Assign these in OnClick) ---

    /// <summary>
    /// TEMPLATE: Scene Change.
    /// Assign this to a 'Start Game' button. 
    /// In the inspector, type the name of the scene you want to load.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        PlayClickSound();
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"Loading Scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("LoadScene called with empty scene name.");
        }
    }

    /// <summary>
    /// TEMPLATE: Open Secondary Menu (Options/Settings).
    /// Assign this to an 'Options' button.
    /// Drag the Options Panel GameObject into the parameter slot in the inspector.
    /// This will turn ON the target panel.
    /// </summary>
    public void OpenPanel(GameObject panelToOpen)
    {
        PlayClickSound();
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }

    /// <summary>
    /// TEMPLATE: Close Menu / Back Button.
    /// Assign this to a 'Back' or 'Close' button inside a secondary menu.
    /// Drag the panel itself into the parameter slot in the inspector.
    /// This will turn OFF the target panel.
    /// </summary>
    public void ClosePanel(GameObject panelToClose)
    {
        PlayClickSound();
        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
        }
    }

    /// <summary>
    /// TEMPLATE: Switch Menus (Swap).
    /// Useful if you want to hide the Main Menu AND show Options at the same time.
    /// </summary>
    public void SwitchPanels(GameObject panelToClose, GameObject panelToOpen)
    {
        PlayClickSound();
        if (panelToClose != null) panelToClose.SetActive(false);
        if (panelToOpen != null) panelToOpen.SetActive(true);
    }

    /// <summary>
    /// TEMPLATE: Quit Application.
    /// Assign this to the 'Quit' button.
    /// </summary>
    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Quitting Game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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