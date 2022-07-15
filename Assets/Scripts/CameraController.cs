using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public bool alwayLookForward = false;
    void FixedUpdate()
    {
        Vector3 desiredPosition;
        if (alwayLookForward)
        {
            desiredPosition = new Vector3( Target.position.x + (Target.localScale.x * offset.x), Target.position.y + offset.y, Target.position.z + offset.z);
        }
        else
        {
            desiredPosition = Target.position + offset;
        }
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
