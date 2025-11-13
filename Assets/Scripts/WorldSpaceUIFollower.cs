using UnityEngine;

/// <summary>
/// Attaches to a world-space UI's parent object to make it follow the player's camera
/// with a smooth, configurable PID (Proportional-Integral-Derivative) controller.
/// </summary>
public class WorldSpaceUIFollower : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The main player camera transform to follow.")]
    public Transform playerCamera;

    public enum DisplayMode
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Freeform
    }

    [Tooltip("The active display mode determining the UI's target position.")]
    public DisplayMode currentMode = DisplayMode.TopLeft;

    [Header("Targeting Offsets")]
    [Tooltip("The default forward distance from the camera.")]
    public float followDistance = 1.8f;

    [Tooltip("The 2D offset (Right/Left, Up/Down) for the Top Left corner, relative to the camera's center view.")]
    public Vector2 topLeftOffset = new Vector2(-0.5f, 0.3f);

    [Tooltip("The 2D offset for the Top Right corner.")]
    public Vector2 topRightOffset = new Vector2(0.5f, 0.3f);

    [Tooltip("The 2D offset for the Bottom Left corner.")]
    public Vector2 bottomLeftOffset = new Vector2(-0.5f, -0.3f);

    [Tooltip("The 2D offset for the Bottom Right corner.")]
    public Vector2 bottomRightOffset = new Vector2(0.5f, -0.3f);

    [Tooltip("A specific transform to follow when in 'Freeform' mode. If this is null, Freeform mode will not function.")]
    public Transform freeformTarget;

    [Header("PID Position Controller")]
    [Tooltip("Proportional gain. How strongly the UI reacts to the current distance error.")]
    public float pGain = 2.0f;

    [Tooltip("Integral gain. How strongly the UI corrects for accumulated past errors (helps eliminate steady drift).")]
    public float iGain = 0.5f;

    [Tooltip("Derivative gain. How strongly the UI dampens its movement to prevent overshooting.")]
    public float dGain = 0.25f;

    [Tooltip("The maximum magnitude of the integral term to prevent 'wind-up' (runaway acceleration).")]
    public float maxIntegral = 10f;

    [Header("Rotation Smoothing")]
    [Tooltip("How quickly the UI Slerps (spherically interpolates) to its target rotation.")]
    public float rotationSpeed = 6.0f;

    // --- Private PID State ---
    private Vector3 integral;
    private Vector3 lastPositionError;

    // --- Private Targets ---
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void OnEnable()
    {
        // When enabled, reset the PID controller and snap to the target position
        // to avoid a sudden "zip" from its last position.
        ResetPID();
        if (playerCamera != null)
        {
            CalculateTargetTransform();
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("WorldSpaceUIFollower: Player Camera not set!", this);
            return;
        }

        // 1. Calculate the desired position and rotation
        CalculateTargetTransform();

        // 2. Update position using the PID controller
        UpdatePosition();

        // 3. Update rotation using Slerp for smoothing
        UpdateRotation();
    }

    /// <summary>
    /// Calculates the targetPosition and targetRotation based on the current display mode.
    /// </summary>
    void CalculateTargetTransform()
    {
        if (currentMode == DisplayMode.Freeform && freeformTarget != null)
        {
            // In Freeform mode, just follow the specified target
            targetPosition = freeformTarget.position;
            targetRotation = freeformTarget.rotation;
        }
        else
        {
            // For corner modes, calculate offset from the camera
            Vector2 offset = Vector2.zero;
            switch (currentMode)
            {
                case DisplayMode.TopLeft:
                    offset = topLeftOffset;
                    break;
                case DisplayMode.TopRight:
                    offset = topRightOffset;
                    break;
                case DisplayMode.BottomLeft:
                    offset = bottomLeftOffset;
                    break;
                case DisplayMode.BottomRight:
                    offset = bottomRightOffset;
                    break;
                // Note: Freeform case is handled above, but good to be explicit
                case DisplayMode.Freeform:
                    // This will be used if freeformTarget is null
                    Debug.LogWarning("Freeform mode selected but no freeformTarget is assigned.", this);
                    // Fallback to a default position (e.g., center)
                    offset = Vector2.zero;
                    break;
            }

            // Calculate the target rotation (face the same direction as the camera)
            targetRotation = playerCamera.rotation;

            // Calculate the base position (in front of the camera)
            Vector3 basePosition = playerCamera.position + (playerCamera.forward * followDistance);

            // Apply the 2D offsets using the camera's local right and up directions
            Vector3 rightOffset = playerCamera.right * offset.x;
            Vector3 upOffset = playerCamera.up * offset.y;

            targetPosition = basePosition + rightOffset + upOffset;
        }
    }

    /// <summary>
    /// Runs the PID logic to calculate and apply the corrective force for position.
    /// </summary>
    void UpdatePosition()
    {
        // Proportional term (current error)
        Vector3 positionError = targetPosition - transform.position;

        // Integral term (accumulated error)
        integral += positionError * Time.deltaTime;
        // Apply anti-windup clamp
        integral = Vector3.ClampMagnitude(integral, maxIntegral);

        // Derivative term (rate of change of error)
        // Avoid division by zero if Time.deltaTime is 0
        Vector3 derivative = Vector3.zero;
        if (Time.deltaTime > 0)
        {
            derivative = (positionError - lastPositionError) / Time.deltaTime;
        }
        lastPositionError = positionError;

        // Calculate the total force from PID terms
        Vector3 force = (pGain * positionError) + (iGain * integral) + (dGain * derivative);

        // Apply the force as a velocity change
        transform.position += force * Time.deltaTime;
    }

    /// <summary>
    /// Smoothly interpolates the rotation towards the target.
    /// </summary>
    void UpdateRotation()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// Resets the PID controller's state variables.
    /// </summary>
    public void ResetPID()
    {
        integral = Vector3.zero;
        lastPositionError = Vector3.zero;
    }
}