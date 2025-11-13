using UnityEngine;
using UnityEngine.UI; // Required for Button
using TMPro; // Required for TextMeshPro
using UnityEngine.SceneManagement; // For loading scenes
using UnityEngine.Events; // Required for UnityEvent
using UnityEngine.EventSystems; // Required for EventTrigger
using System.Collections.Generic; // Required for List

/// <summary>
/// This is a custom data class that defines a single, managed button.
/// It is [System.Serializable] so it can appear in the Unity Inspector.
/// </summary>
[System.Serializable]
public class MenuButtonConfig
{
    [Tooltip("The actual Button component from your UI.")]
    public Button button;

    [Tooltip("A name for reference. Will be auto-filled from the Button's GameObject name if left empty.")]
    public string buttonName;

    [Tooltip("The description to show when this button is hovered over.")]
    [TextArea(3, 5)]
    public string tooltipDescription;

    [Header("Button Action")]
    [Tooltip("Does this button load a new scene?")]
    public bool doesSceneChange;

    [Tooltip("If 'doesSceneChange' is true, which scene should be loaded?")]
    public string sceneName;

    // --- THIS IS THE NEWLY ADDED FIELD ---
    [Tooltip("If 'doesSceneChange' is false, does this button quit the application? This takes priority over 'otherAction'.")]
    public bool isQuitButton;

    [Tooltip("If 'doesSceneChange' and 'isQuitButton' are false, what other actions should this button perform?")]
    public UnityEvent otherAction;
}


/// <summary>
/// Manages a list of UI buttons, automatically assigning their
/// hover-to-show-description and click actions based on the
/// configuration provided in the 'managedButtons' list.
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshPro UI element that will display the descriptions.")]
    public TextMeshProUGUI descriptionText;

    [Tooltip("Optional AudioSource for UI sounds.")]
    public AudioSource uiAudioSource;

    [Header("UI Sounds")]
    [Tooltip("Sound to play on button hover.")]
    public AudioClip hoverSound;

    [Tooltip("Sound to play on button click.")]
    public AudioClip clickSound;

    [Header("Default State")]
    [Tooltip("The default text to display when not hovering over any button.")]
    [TextArea(3, 5)]
    public string defaultDescription = "Select an option to see more details.";

    [Header("Button Configuration")]
    [Tooltip("The list of buttons this menu will control.")]
    public List<MenuButtonConfig> managedButtons;

    // --- Unity Methods ---

    void Start()
    {
        // Set the initial description text
        ResetDescription();

        // Loop through all managed buttons and "wire them up"
        foreach (MenuButtonConfig config in managedButtons)
        {
            if (config.button == null)
            {
                Debug.LogWarning("A slot in ManagedButtons is empty.", this);
                continue;
            }

            // Auto-populate the name if it's empty
            if (string.IsNullOrEmpty(config.buttonName))
            {
                config.buttonName = config.button.gameObject.name;
            }

            // --- 1. Wire up CLICK event ---
            MenuButtonConfig localConfig = config;
            config.button.onClick.AddListener(() =>
            {
                OnManagedButtonClick(localConfig);
            });

            // --- 2. Wire up HOVER events (Pointer Enter / Exit) ---
            EventTrigger trigger = config.button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = config.button.gameObject.AddComponent<EventTrigger>();
            }

            // Create the "PointerEnter" event
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((eventData) =>
            {
                OnManagedHoverEnter(localConfig.tooltipDescription);
            });

            // Create the "PointerExit" event
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((eventData) =>
            {
                ResetDescription();
            });

            // Add the new events to the trigger's list
            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);
        }
    }

    // --- Private Listeners (Called by the auto-wired events) ---

    /// <summary>
    /// This single function handles all button clicks and executes
    /// the correct action based on the button's configuration.
    /// --- THIS METHOD IS UPDATED ---
    /// </summary>
    private void OnManagedButtonClick(MenuButtonConfig config)
    {
        PlayClickSound();

        // Priority 1: Scene Change
        if (config.doesSceneChange)
        {
            if (!string.IsNullOrEmpty(config.sceneName))
            {
                Debug.Log($"Loading scene: {config.sceneName}...");
                SceneManager.LoadScene(config.sceneName);
            }
            else
            {
                Debug.LogWarning($"Button '{config.buttonName}' is set to change scene, but no sceneName was provided.", this);
            }
        }
        // Priority 2: Quit Button (The new logic)
        else if (config.isQuitButton)
        {
            QuitGame();
        }
        // Priority 3: Other Custom Action
        else
        {
            config.otherAction?.Invoke();
        }
    }

    /// <summary>
    /// Shows the specified description text.
    /// </summary>
    private void OnManagedHoverEnter(string description)
    {
        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
        PlayHoverSound();
    }

    /// <summary>
    /// Resets the description text to the default.
    /// </summary>
    public void ResetDescription()
    {
        if (descriptionText != null)
        {
            descriptionText.text = defaultDescription;
        }
    }

    // --- Public Methods (For the 'otherAction' UnityEvent) ---

    /// <summary>
    /// A public function to quit the game.
    /// It's called automatically by 'isQuitButton' or can be 
    /// manually added to the 'otherAction' UnityEvent.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
    }

    // --- Helper Methods ---

    private void PlayHoverSound()
    {
        if (uiAudioSource != null && hoverSound != null)
        {
            uiAudioSource.PlayOneShot(hoverSound);
        }
    }

    private void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }
}