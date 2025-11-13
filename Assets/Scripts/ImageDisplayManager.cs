// ImageDisplayManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal; // Required for DecalProjector

public class ImageDisplayManager : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("The Decal Projector whose material will be changed.")]
    [SerializeField] private DecalProjector decalProjector;

    [Header("Performance Setup")]
    [Tooltip("The list of image cues, sorted by display time.")]
    [SerializeField] private List<ImageCue> performanceCues;

    // --- Private State ---
    private Material projectorMaterialInstance; // An instance of the material to avoid changing the project asset
    private float performanceTimer = 0f;
    private int currentCueIndex = -1;
    private bool isPerformanceRunning = false;

    /// <summary>
    /// This is the public entry point that will be called by the OnCalibrationComplete event.
    /// </summary>
    public void StartPerformance()
    {
        if (decalProjector == null || performanceCues.Count == 0)
        {
            Debug.LogError("Manager is not set up correctly. Check projector reference and cues.");
            return;
        }

        // It's crucial to create an INSTANCE of the material.
        // Otherwise, you'd be changing the material asset file directly.
        projectorMaterialInstance = decalProjector.material;

        isPerformanceRunning = true;
        performanceTimer = 0f;
        currentCueIndex = -1; // Set to -1 to ensure the first cue at time 0.0 is triggered
        Debug.Log("Performance Starting!");
    }

    void Update()
    {
        // Don't do anything if the performance hasn't started or is over
        if (!isPerformanceRunning) return;

        // Increment the timer
        performanceTimer += Time.deltaTime;

        // Check if there is a next cue in the list
        if (currentCueIndex + 1 < performanceCues.Count)
        {
            // Check if the timer has passed the next cue's display time
            if (performanceTimer >= performanceCues[currentCueIndex + 1].displayTime)
            {
                currentCueIndex++;
                ChangeProjectorImage(performanceCues[currentCueIndex].imageMaterial);
            }
        }
        else
        {
            Debug.Log("Performance Completed");
            // Optional: What to do when the performance is over
            // For now, we just stop checking.
        }
    }

    /// <summary>
    /// Changes the texture on the projector's material instance.
    /// </summary>
    private void ChangeProjectorImage(Material newMaterial)
    {
        if (newMaterial == null)
        {
            Debug.LogWarning($"Cue {currentCueIndex} has a null material. Projector will show nothing.");
            // Optionally, you could set the projector's material to null to make it disappear
            // decalProjector.material = null; 
            return;
        }

        // This is the key change: we assign the entire material
        decalProjector.material = newMaterial;
        Debug.Log($"Time: {performanceTimer:F2}s - Displaying cue {currentCueIndex}: {newMaterial.name}");
    }
}