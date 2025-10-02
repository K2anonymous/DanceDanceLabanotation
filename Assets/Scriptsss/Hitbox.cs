using UnityEngine;

[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
public class Hitbox : MonoBehaviour
{
    [Header("Feedback Colors")]
    public Color idleColor = Color.white;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;

    [Header("Modes")]
    public bool standaloneTestMode = false;  // toggle in Inspector when no GameManager yet

    private MeshRenderer meshRenderer;
    private bool isActiveTarget = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetColor(idleColor);

        if (standaloneTestMode)
        {
            isActiveTarget = true; // treat as always correct in test mode
        }
    }

    /// <summary>
    /// Called by the GameManager to activate/deactivate this hitbox for the round.
    /// Ignored if standaloneTestMode is enabled.
    /// </summary>
    public void SetActiveTarget(bool active)
    {
        if (standaloneTestMode) return;

        isActiveTarget = active;
        SetColor(idleColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collision is hit");

        //if (!other.CompareTag("PlayerArm")) return;
        Debug.Log("PlayerArm tag");
        if (isActiveTarget && other.tag == "PlayerArm")
        {
            Debug.Log("active target");
            SetColor(correctColor);
        }
        else if(other.tag == "blah blah")
        {
            return;
        }
        else
        {
            SetColor(incorrectColor);
            Debug.Log("incorrect color target");
        }
    }

    private void OnTriggerExit(Collider other)
    {
         SetColor(idleColor);
    }

    private void SetColor(Color color)
    {
        if (meshRenderer != null)
            meshRenderer.material.color = color;


    }

}

