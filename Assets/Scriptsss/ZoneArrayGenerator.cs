using UnityEngine;
using System.Collections.Generic;

public class ZoneArrayGenerator : MonoBehaviour
{
    // --- Inspector Variables ---

    [Header("Generation Settings")]
    public GameObject cubePrefab; // Assign your cube prefab here
    public int rings = 8;         // Number of horizontal rings (latitude)
    public int cubesPerRing = 12; // Number of cubes in a single ring (longitude)

    [Header("Player Dimensions for Scaling")]
    // These will be manually set or calculated from XR data later
    public float baseRadius = 2.0f; // Initial distance from player center
    public float armLengthFactor = 1.5f; // Factor to apply to arm length for radius adjustment
    public float playerHeightFactor = 0.5f; // Factor to apply to player height for radius adjustment

    [Header("Runtime Data (Optional)")]
    private List<GameObject> spawnedCubes = new List<GameObject>();

    // Reference to the main XR camera (usually the "Head" transform)
    private Transform xrCamera;

    // --- Unity Life Cycle Methods ---

    void Start()
    {
        // **IMPORTANT:** Find the main camera/head transform of the XR Rig
        // This is often a child of the XR Origin. 
        // You may need to adjust the path based on your specific XR setup.
        // A common pattern is finding the 'Camera' or 'Head' child.
        xrCamera = Camera.main.transform;

        GenerateArray();
    }

    // Use Update to adjust the radius dynamically based on player dimensions
    void Update()
    {
        AdjustRadiusBasedOnPlayer();
    }

    // --- Core Logic ---

    void AdjustRadiusBasedOnPlayer()
    {
        if (xrCamera == null) return;

        // **1. Determine Player Dimensions**
        // A simple way to estimate player height is the camera's Y position 
        // relative to the XR Origin's Y (assuming the origin is on the floor).
        // Since this script is a child of XR Origin, transform.position is the origin.
        float playerHeight = xrCamera.localPosition.y;

        // Arm Length is a difficult measurement to get without a more complex script
        // tracking controllers. For simplicity, we'll use a fixed value or a proxy 
        // for initial testing. 
        // Later, you can replace 'SimulatedArmLength' with a value from XR Controller data.
        float simulatedArmLength = 0.8f; // Placeholder value in meters

        // **2. Calculate Dynamic Radius**
        float dynamicRadius = baseRadius +
                              (playerHeight * playerHeightFactor) +
                              (simulatedArmLength * armLengthFactor);

        // **3. Apply New Radius (Reposition Cubes)**
        // To avoid re-instantiating all objects, we just update their local positions.
        UpdateCubePositions(dynamicRadius);
    }

    void GenerateArray()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab is not assigned in the Inspector!");
            return;
        }

        // Clear previous cubes
        foreach (var cube in spawnedCubes)
        {
            Destroy(cube);
        }
        spawnedCubes.Clear();

        // Calculate delta angles for even distribution
        float deltaPhi = Mathf.PI / rings; // Vertical angle (Polar angle)

        // Start generating from top to bottom
        for (int i = 0; i <= rings; i++)
        {
            // Vertical angle (from 0 to PI radians)
            float phi = i * deltaPhi;

            // Cubes at the very top (i=0) and bottom (i=rings) are a single point.
            int currentCubesPerRing = (i == 0 || i == rings) ? 1 : cubesPerRing;

            // Horizontal angle delta (Azimuthal angle)
            float deltaTheta = (2 * Mathf.PI) / currentCubesPerRing;

            for (int j = 0; j < currentCubesPerRing; j++)
            {
                float theta = j * deltaTheta; // Horizontal angle (from 0 to 2*PI radians)

                // **Spherical to Cartesian Conversion (Local Position)**
                Vector3 localPos = SphericalToCartesian(baseRadius, phi, theta);

                // **Instantiate Cube**
                GameObject newCube = Instantiate(cubePrefab, transform); // 'transform' is the parent
                newCube.transform.localPosition = localPos;

                // Optional: Make the cube face outward (away from the center)
                newCube.transform.localRotation = Quaternion.LookRotation(localPos.normalized);

                spawnedCubes.Add(newCube);
            }
        }
    }

    // This method updates positions for existing cubes when the radius changes
    void UpdateCubePositions(float newRadius)
    {
        if (spawnedCubes.Count == 0) return;

        // This is a simplified version. For a more robust solution, 
        // you would store the original spherical angles for each cube.
        // For now, we'll iterate through children and re-normalize their direction.

        foreach (GameObject cube in spawnedCubes)
        {
            // Get the current local direction (normalized)
            Vector3 direction = cube.transform.localPosition.normalized;

            // Multiply the direction by the new radius to get the new position
            cube.transform.localPosition = direction * newRadius;
        }
    }

    // Utility function for spherical to Cartesian coordinates conversion
    // Unity uses a Y-up system.
    private Vector3 SphericalToCartesian(float r, float phi, float theta)
    {
        // x = r * sin(phi) * cos(theta)
        // y = r * cos(phi)
        // z = r * sin(phi) * sin(theta)

        float x = r * Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = r * Mathf.Cos(phi);
        float z = r * Mathf.Sin(phi) * Mathf.Sin(theta);

        return new Vector3(x, y, z);
    }

    // Draw the array structure in the Scene view for easy debugging
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, baseRadius); // Draw the base sphere

        // Draw the dynamically adjusted sphere in play mode
        if (Application.isPlaying)
        {
            // Note: This relies on 'Update' running to calculate the dynamic radius, 
            // which can't happen in the editor. For best results, use 'OnDrawGizmosSelected' 
            // and an approximation of the dynamic radius here if needed in edit mode.
        }
    }
}