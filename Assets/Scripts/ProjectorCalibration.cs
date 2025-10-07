// ProjectorCalibration.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro; // Add this if you use TextMeshPro for UI

public class ProjectorCalibration : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("The Decal Projector to be calibrated.")]
    [SerializeField] private UnityEngine.Rendering.Universal.DecalProjector decalProjector;
    [Tooltip("The parent GameObject for the calibration UI canvas.")]
    [SerializeField] private GameObject calibrationUI;
    [Tooltip("A UI text element to display the current mode.")]
    [SerializeField] private TextMeshProUGUI modeText;

    [Header("Input Actions")]
    [Tooltip("Vector2 action for movement (e.g., Left Thumbstick).")]
    [SerializeField] private InputActionReference moveAction;
    [Tooltip("Vector2 action for rotation and scaling (e.g., Right Thumbstick).")]
    [SerializeField] private InputActionReference transformAction;
    [Tooltip("Button action to cycle through calibration modes.")]
    [SerializeField] private InputActionReference cycleModeAction;
    [Tooltip("Button action to confirm the calibration.")]
    [SerializeField] private InputActionReference confirmAction;

    [Header("Calibration Settings")]
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float rotateSpeed = 30.0f;
    [SerializeField] private float scaleSpeed = 0.5f;

    [Header("Events")]
    [Tooltip("This event is invoked when calibration is confirmed.")]
    public UnityEvent OnCalibrationComplete;

    // Enum to manage which transformation is active
    private enum CalibrationMode { Position, Rotation, Scale }
    private CalibrationMode currentMode;
    private bool isCalibrating = false;

    // PlayerPrefs keys for saving/loading
    private const string CALIBRATION_SAVED_KEY = "ProjectorCalibrationSaved";

    void Awake()
    {
        // Ensure projector and UI are assigned
        if (decalProjector == null) decalProjector = GetComponent<UnityEngine.Rendering.Universal.DecalProjector>();
        if (calibrationUI != null) calibrationUI.SetActive(false);

        // Attempt to load previously saved calibration data
        //LoadCalibration();
        StartCalibration(); 
    }

    private void OnEnable()
    {
        // Subscribe to input events when the script is active
        cycleModeAction.action.performed += CycleMode;
        confirmAction.action.performed += ConfirmCalibration;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors
        cycleModeAction.action.performed -= CycleMode;
        confirmAction.action.performed -= ConfirmCalibration;
    }

    /// <summary>
    /// Public method to begin the calibration process.
    /// </summary>
    public void StartCalibration()
    {
        isCalibrating = true;
        this.enabled = true; // Ensure the component is active to receive updates
        if (calibrationUI != null) calibrationUI.SetActive(true);
        currentMode = CalibrationMode.Position;
        UpdateUIMode();
        Debug.Log("Calibration started. Mode: Position");
    }

    void Update()
    {
        // Only process input if in calibration mode
        if (!isCalibrating) return;

        HandleMovement();
        HandleTransform();
    }

    private void HandleMovement()
    {
        if (currentMode != CalibrationMode.Position) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        // Move along the XZ plane (floor)
        Vector3 movement = new Vector3(input.x, 0, input.y) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    private void HandleTransform()
    {
        Vector2 input = transformAction.action.ReadValue<Vector2>();

        switch (currentMode)
        {
            case CalibrationMode.Rotation:
                // Rotate around the Y-axis (yaw) using the X-axis of the thumbstick
                transform.Rotate(0, input.x * rotateSpeed * Time.deltaTime, 0, Space.World);
                break;

            case CalibrationMode.Scale:
                // Adjust size on X and Z axes
                Vector3 sizeChange = new Vector3(input.x, 0, input.y) * scaleSpeed * Time.deltaTime;
                decalProjector.size += sizeChange;

                // Prevent negative scaling
                decalProjector.size = new Vector3(
                    Mathf.Max(decalProjector.size.x, 0.1f),
                    decalProjector.size.y,
                    Mathf.Max(decalProjector.size.z, 0.1f)
                );
                break;
        }
    }

    private void CycleMode(InputAction.CallbackContext context)
    {
        if (!isCalibrating) return;

        // Cycle through Position -> Rotation -> Scale -> Position ...
        currentMode = (CalibrationMode)(((int)currentMode + 1) % System.Enum.GetValues(typeof(CalibrationMode)).Length);
        UpdateUIMode();
        Debug.Log($"Mode changed to: {currentMode}");
    }

    private void ConfirmCalibration(InputAction.CallbackContext context)
    {
        if (!isCalibrating) return;

        Debug.Log("Calibration confirmed and saved.");
        isCalibrating = false;
        if (calibrationUI != null) calibrationUI.SetActive(false);

        // Save the final transform and size
        SaveCalibration();

        // Fire the event to notify other scripts
        OnCalibrationComplete?.Invoke();

        // Disable this script as its job is done
        this.enabled = false;
    }

    private void SaveCalibration()
    {
        // Save position, rotation, and size using PlayerPrefs
        PlayerPrefs.SetFloat("ProjectorPosX", transform.position.x);
        PlayerPrefs.SetFloat("ProjectorPosY", transform.position.y);
        PlayerPrefs.SetFloat("ProjectorPosZ", transform.position.z);
        PlayerPrefs.SetFloat("ProjectorRotY", transform.eulerAngles.y);
        PlayerPrefs.SetFloat("ProjectorSizeX", decalProjector.size.x);
        PlayerPrefs.SetFloat("ProjectorSizeZ", decalProjector.size.z);
        PlayerPrefs.SetInt(CALIBRATION_SAVED_KEY, 1); // Flag that we have saved data
        PlayerPrefs.Save();
    }

    private void LoadCalibration()
    {
        if (PlayerPrefs.GetInt(CALIBRATION_SAVED_KEY, 0) == 1)
        {
            Debug.Log("Loading saved calibration data.");
            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat("ProjectorPosX"),
                PlayerPrefs.GetFloat("ProjectorPosY"),
                PlayerPrefs.GetFloat("ProjectorPosZ")
            );
            Vector3 eulerAngles = new Vector3(0, PlayerPrefs.GetFloat("ProjectorRotY"), 0);
            Vector3 size = new Vector3(
                PlayerPrefs.GetFloat("ProjectorSizeX"),
                decalProjector.size.y, // Keep the original depth
                PlayerPrefs.GetFloat("ProjectorSizeZ")
            );

            transform.position = position;
            transform.eulerAngles = eulerAngles;
            decalProjector.size = size;

            // Immediately invoke the completion event since we loaded data
            OnCalibrationComplete?.Invoke();
            this.enabled = false; // No need for calibration
        }
        else
        {
            // If no saved data, start the calibration process automatically
            StartCalibration();
        }
    }

    private void UpdateUIMode()
    {
        if (modeText != null)
        {
            modeText.text = $"Mode: {currentMode}";
        }
    }
}