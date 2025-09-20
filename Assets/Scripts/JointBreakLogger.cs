using UnityEngine;

public class JointBreakLogger : MonoBehaviour
{
    private void OnJointBreak(float breakForce)
    {
        Debug.Log($"{gameObject.name} rompi� un Joint con fuerza: {breakForce}");
    }
}
