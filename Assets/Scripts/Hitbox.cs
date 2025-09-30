using UnityEngine;

[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
public class Hitbox : MonoBehaviour
{
    [Header("Feedback Colors")]
    public Color idleColor;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    private MeshRenderer meshRenderer;

    [Header("Settings")]
    public bool isCorrectPosition = false;   // Used by VR gameplay
    public bool testingMode = false;         // Toggle this in Inspector for testing
    public KeyCode testKeyCorrect = KeyCode.C;   // Press to force correct color
    public KeyCode testKeyIncorrect = KeyCode.I; // Press to force incorrect color
    public KeyCode testKeyIdle = KeyCode.Space;  // Press to reset

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetColor(idleColor);
    }

    private void Update()
    {
        if (testingMode)
        {
            if (Input.GetKeyDown(testKeyCorrect))
            {
                SetColor(correctColor);
                Debug.Log($"{gameObject.name} forced to CORRECT color.");
            }
            else if (Input.GetKeyDown(testKeyIncorrect))
            {
                SetColor(incorrectColor);
                Debug.Log($"{gameObject.name} forced to INCORRECT color.");
            }
            else if (Input.GetKeyDown(testKeyIdle))
            {
                SetColor(idleColor);
                Debug.Log($"{gameObject.name} reset to IDLE color.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!testingMode && other.CompareTag("PlayerArm"))
        {
            if (isCorrectPosition)
            {
                SetColor(correctColor);
                Debug.Log($"{gameObject.name}: Correct arm position!");
            }
            else
            {
                SetColor(incorrectColor);
                Debug.Log($"{gameObject.name}: Incorrect arm position.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!testingMode && other.CompareTag("PlayerArm"))
        {
            SetColor(idleColor);
        }
    }

    private void SetColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }
}