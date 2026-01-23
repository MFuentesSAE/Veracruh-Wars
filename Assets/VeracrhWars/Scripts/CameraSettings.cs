using UnityEngine;

//Top down camera following a target with an offset
public class CameraSettings : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f);
    [SerializeField] private float smoothSpeed = 10f;
    
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

    }

}
