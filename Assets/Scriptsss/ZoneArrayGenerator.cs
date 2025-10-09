using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates 27 Labanotation zones dynamically scaled to the player's size.
/// Calibration is based on player head height and hand-to-center distance.
/// </summary>
public class LabanotationZoneGenerator : MonoBehaviour
{
    [Header("Zone Configuration")]
    [Tooltip("Prefab for the zone marker. Must have a 'Hitbox' component.")]
    public GameObject zonePrefab;

    [Tooltip("Optional: A parent transform to keep the generated zones organized.")]
    public Transform zoneParent;

    [Header("Dynamic Calibration")]
    [Tooltip("Reference to the player's head (e.g., the Main Camera in an XR rig).")]
    public Transform playerHead;

    [Tooltip("Reference to one of the player's hands (e.g., a controller transform).")]
    public Transform playerHand;

    [Tooltip("Multiplier to adjust the radial distance based on arm length. 1.0 is full reach.")]
    [Range(0.5f, 1.2f)]
    public float armLengthMultiplier = 0.9f;

    [Tooltip("Multiplier to adjust vertical spacing based on player height.")]
    [Range(0.2f, 0.6f)]
    public float heightSpacingMultiplier = 0.4f;

    // Place this method in your LabanotationZoneGenerator class

    void Start()
    {
        // This will automatically run your zone generation logic 
        // as soon as the game begins.
        CalibrateAndGenerateZones();
    }

    // --- Private Fields ---
    private List<Hitbox> _generatedZones = new List<Hitbox>();
    private float _calibratedRadius;
    private float _calibratedHeightSpacing;
    private bool _isCalibrated = false;

    // --- Constant Data Arrays ---
    private readonly string[] heightNames = { "Low", "Middle", "High" };
    private readonly float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private readonly string[] directionalNames = {
        "Forward", "Forward Right Diagonal", "Right", "Back Right Diagonal",
        "Backward", "Back Left Diagonal", "Left", "Forward Left Diagonal"
    };

    /// <summary>
    /// Calibrates the zone dimensions based on player size and then generates the zones.
    /// This is the primary method to call to create the zones.
    /// </summary>
    [ContextMenu("Calibrate and Generate Zones")]
    public void CalibrateAndGenerateZones()
    {
        // 1. --- CALIBRATION STEP ---
        if (playerHead == null || playerHand == null)
        {
            Debug.LogError("Player Head and Player Hand transforms must be assigned for calibration!");
            return;
        }

        // Height defines the vertical spacing. We use the head's Y position as the reference.
        // The origin of the XR rig is assumed to be on the floor.
        _calibratedHeightSpacing = playerHead.localPosition.y * heightSpacingMultiplier;

        // Arm length defines the radial distance. We calculate the horizontal distance
        // from the player's center (at X=0, Z=0) to their hand.
        Vector3 handPosition = playerHand.localPosition;
        Vector2 handHorizontalPosition = new Vector2(handPosition.x, handPosition.z);
        _calibratedRadius = handHorizontalPosition.magnitude * armLengthMultiplier;

        _isCalibrated = true;
        Debug.Log($"Calibration complete: HeightSpacing={_calibratedHeightSpacing:F2}, Radius={_calibratedRadius:F2}");

        // 2. --- GENERATION STEP ---
        GenerateZonesInternal();
    }

    /// <summary>
    /// Generates the array of zones using the calibrated dimensions.
    /// </summary>
    private void GenerateZonesInternal()
    {
        if (!_isCalibrated)
        {
            Debug.LogWarning("Zones have not been calibrated. Using default values or last known calibration.");
            if (_calibratedRadius <= 0) _calibratedRadius = 1.0f; // Fallback
            if (_calibratedHeightSpacing <= 0) _calibratedHeightSpacing = 0.75f; // Fallback
        }

        ClearExistingZones();

        if (zonePrefab == null)
        {
            Debug.LogError("Zone Prefab is not assigned! Cannot generate zones.");
            return;
        }

        if (zonePrefab.GetComponent<Hitbox>() == null)
        {
            Debug.LogError("The Zone Prefab must have a 'Hitbox' script component attached!");
            return;
        }

        Transform parent = zoneParent != null ? zoneParent : transform;

        for (int i = 0; i < 3; i++) // Loop through Low, Middle, High
        {
            // Middle level (i=1) is at y=0, relative to the zoneParent.
            float yPos = (i - 1) * _calibratedHeightSpacing;
            string currentHeightName = heightNames[i];

            // A. Generate the 8 Directional Zones
            for (int j = 0; j < 8; j++)
            {
                float angleRad = angles[j] * Mathf.Deg2Rad;
                float xPos = _calibratedRadius * Mathf.Sin(angleRad);
                float zPos = _calibratedRadius * Mathf.Cos(angleRad);

                Vector3 position = new Vector3(xPos, yPos, zPos);
                string zoneName = $"{currentHeightName} - {directionalNames[j]}";

                InstantiateZone(position, zoneName, parent);
            }

            // B. Generate the 1 Central 'Place' Zone
            Vector3 centerPosition = new Vector3(0f, yPos, 0f);
            string centerZoneName = $"{currentHeightName} - Place";
            if (i == 1) centerZoneName += " (Center)";

            InstantiateZone(centerPosition, centerZoneName, parent);
        }

        Debug.Log($"Successfully generated {_generatedZones.Count} Labanotation Zones.");
    }

    /// <summary>
    /// Destroys all previously generated zone objects.
    /// </summary>
    [ContextMenu("Clear Zones")]
    public void ClearExistingZones()
    {
        // Iterate backwards because we are removing items from the list
        for (int i = _generatedZones.Count - 1; i >= 0; i--)
        {
            if (_generatedZones[i] != null)
            {
                // Use DestroyImmediate in Edit mode, Destroy in Play mode
                if (Application.isEditor && !Application.isPlaying)
                {
                    DestroyImmediate(_generatedZones[i].gameObject);
                }
                else
                {
                    Destroy(_generatedZones[i].gameObject);
                }
            }
        }

        int count = _generatedZones.Count;
        _generatedZones.Clear();

        if (count > 0)
        {
            Debug.Log($"Cleared {count} existing zone objects.");
        }
    }

    /// <summary>
    /// Helper function to instantiate, name, and register a zone object.
    /// </summary>
    private void InstantiateZone(Vector3 localPosition, string zoneName, Transform parent)
    {
        GameObject newZoneObj = Instantiate(zonePrefab, parent);
        newZoneObj.transform.localPosition = localPosition;
        newZoneObj.transform.localRotation = Quaternion.identity;
        newZoneObj.name = zoneName;

        // Get the hitbox component and add it to our list for management
        Hitbox hitbox = newZoneObj.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            // Optional: Initialize the hitbox with its name or other data
            // hitbox.Initialize(zoneName); 
            _generatedZones.Add(hitbox);
        }
    }
}