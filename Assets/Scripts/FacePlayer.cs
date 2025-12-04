using UnityEngine;

/// <summary>
/// Makes an object rotate to always face the player's camera.
/// Ideal for World Space UI, tooltips, and health bars.
/// </summary>
public class FacePlayer : MonoBehaviour
{
    [Tooltip("The camera to look at. If empty, uses Camera.main.")]
    public Transform targetCamera;

    [Tooltip("If true, the object will only rotate left/right, keeping it upright.")]
    public bool lockVerticalRotation = false;

    [Tooltip("If your UI text appears backwards, toggle this.")]
    public bool reverseFace = false;

    void Start()
    {
        // Automatically find the main camera if none is assigned
        if (targetCamera == null)
        {
            if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("FacePlayer: No Main Camera found. Please assign Target Camera manually.", this);
            }
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // Calculate the direction FROM the camera TO this object.
        // This causes the object's "Forward" (Z+) axis to point away from the camera.
        // Since standard Unity UI faces "backwards" (towards -Z), this makes the UI face the camera correctly.
        Vector3 direction = transform.position - targetCamera.position;

        // If reverseFace is true, we point the Z axis AT the camera instead.
        if (reverseFace)
        {
            direction = targetCamera.position - transform.position;
        }

        // If we want to keep the text upright (like a signpost), flatten the Y direction
        if (lockVerticalRotation)
        {
            direction.y = 0;
        }

        // Apply the rotation
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}