using UnityEngine;

public class JointBreakLogger : MonoBehaviour
{
    private void OnJointBreak(float breakForce)
    {
        Debug.Log($"{gameObject.name} rompió un Joint con fuerza: {breakForce}");
    }
}
