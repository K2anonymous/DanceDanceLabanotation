using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Generates 24 Labanotation "cone" zones, all originating from the player's chest
/// and pointing outwards. Calibration sets the cone length.
/// </summary>
public class LabanotationConeGenerator : MonoBehaviour
{
    [Header("Zone Configuration")]
    [Tooltip("Prefab for the cone zone. Must have its point at 0,0,0 and 'point up' its Y-axis.")]
    public GameObject zonePrefab; // This should be your cone prefab
    [Tooltip("A parent transform to keep the generated zones organized.")]
    public Transform zoneParent;

    [Header("Dynamic Calibration")]
    [Tooltip("Reference to the player's head (e.g., the Main Camera in an XR rig).")]
    public Transform playerHead;
    [Tooltip("Reference to one of the player's hands (e.g., a controller transform).")]
    public Transform playerHand;
    [Tooltip("Multiplier for arm length, which determines cone length.")]
    [Range(0.5f, 1.2f)]
    public float armLengthMultiplier = 1.0f;

    // --- We no longer need heightSpacingMultiplier ---

    [Header("Runtime Calibration")]
    [Tooltip("Positions the origin of all cones (e.g., 0.75 = chest).")]
    [Range(0.5f, 1.0f)]
    public float verticalOffsetMultiplier = 0.75f; // This is now the "chest" origin point

    // --- Private Fields ---
    private List<Hitbox> _generatedZones = new List<Hitbox>();

    // --- Constant Data Arrays ---
    private readonly string[] heightNames = { "Low", "Middle", "High" };
    private readonly float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private readonly string[] directionalNames = {
        "Forward", "Forward Right Diagonal", "Right", "Back Right Diagonal",
        "Backward", "Back Left Diagonal", "Left", "Forward Left Diagonal"
    };

    // (OnCalibrate method is unchanged)
    public void OnCalibrate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("'Calibrate' action performed! Calibrating and generating zones...");
            CalibrateAndGenerateZones();
        }
    }

    /// <summary>
    /// Sets the origin position and calibrates cone length, then generates the zones.
    /// </summary>
    [ContextMenu("Calibrate and Generate Zones")]
    public void CalibrateAndGenerateZones()
    {
        if (playerHead == null || playerHand == null)
        {
            Debug.LogError("Player Head and Hand transforms must be assigned!");
            return;
        }

        Transform parent = zoneParent != null ? zoneParent : transform;

        // --- 1. SET ORIGIN POSITION ---
        // This is the "chest" spot where all cone points will be
        float verticalCenter = playerHead.position.y * verticalOffsetMultiplier;
        parent.position = new Vector3(parent.position.x, verticalCenter, parent.position.z);
        Debug.Log($"Cone origin set to Y: {verticalCenter:F2}");

        // --- 2. CALIBRATE CONE LENGTH ---
        // We use the arm length to determine how long the cones should be
        Vector3 handPosition = playerHand.localPosition;
        Vector2 handHorizontalPosition = new Vector2(handPosition.x, handPosition.z);
        float calibratedLength = handHorizontalPosition.magnitude * armLengthMultiplier;

        Debug.Log($"Calibration complete: Cone Length={calibratedLength:F2}");

        // --- 3. GENERATE ZONES ---
        GenerateZonesInternal(calibratedLength); // Pass the length to the generator
    }

    /// <summary>
    /// Generates the 24 cone zones using the calibrated length.
    /// </summary>
    private void GenerateZonesInternal(float coneLength)
    {
        ClearExistingZones();

        if (zonePrefab == null) { return; }
        if (zonePrefab.GetComponent<Hitbox>() == null) { return; }

        Transform parent = zoneParent != null ? zoneParent : transform;

        // Loop through the three height levels
        for (int i = 0; i < 3; i++)
        {
            // --- NEW: Define the vertical direction ---
            // i=0 (Low) -> Y = -1
            // i=1 (Middle) -> Y = 0
            // i=2 (High) -> Y = 1
            float yDir = (i - 1);
            string currentHeightName = heightNames[i];

            // Loop through the 8 horizontal directions
            for (int j = 0; j < 8; j++)
            {
                // --- NEW: Define the horizontal direction ---
                float angleRad = angles[j] * Mathf.Deg2Rad;
                float xDir = Mathf.Sin(angleRad);
                float zDir = Mathf.Cos(angleRad);

                // --- NEW: Create the final direction vector ---
                Vector3 direction = new Vector3(xDir, yDir, zDir).normalized;

                // --- NEW: Calculate the rotation to point the cone ---
                // We assume the cone prefab points "up" along its Y-axis
                // This calculates the rotation from "up" to our target direction
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);

                string zoneName = $"{currentHeightName} - {directionalNames[j]}";

                // --- MODIFIED: Pass rotation and length to the new function ---
                InstantiateZone(zoneName, parent, rotation, coneLength);
            }
        }

        Debug.Log($"Successfully generated {_generatedZones.Count} Labanotation Zones.");
    }

    // (ClearExistingZones method is unchanged)
    public void ClearExistingZones()
    {
        for (int i = _generatedZones.Count - 1; i >= 0; i--)
        {
            if (_generatedZones[i] != null)
            {
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
        if (count > 0) { Debug.Log($"Cleared {count} existing zone objects."); }
    }


    /// <summary>
    /// Helper function to instantiate, position, rotate, and scale a zone object.
    /// </summary>
    private void InstantiateZone(string zoneName, Transform parent, Quaternion rotation, float length)
    {
        GameObject newZoneObj = Instantiate(zonePrefab, parent);

        // --- NEW LOGIC ---
        newZoneObj.transform.localPosition = Vector3.zero; // All cones start at the origin
        newZoneObj.transform.localRotation = rotation;     // Point them in the calculated direction
        newZoneObj.name = zoneName;

        // --- NEW: Set cone length ---
        // This assumes your cone prefab is 1m tall along its Y-axis.
        // We set the Y-scale to the calibrated arm length.
        // You can adjust the X/Z scale here if you want wider/narrower cones.
        newZoneObj.transform.localScale = new Vector3(1, length, 1);

        // (Hitbox logic is unchanged)
        Hitbox hitbox = newZoneObj.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            _generatedZones.Add(hitbox);
        }
    }
}
