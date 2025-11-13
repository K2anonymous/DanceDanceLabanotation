using UnityEngine;
using UnityEngine.InputSystem;

public class TestDebug : MonoBehaviour
{
   public void OnDebug(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("This input is working!");
        }
    }
       
}
