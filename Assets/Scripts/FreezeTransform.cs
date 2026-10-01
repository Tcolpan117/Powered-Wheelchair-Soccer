using UnityEngine;

public class FreezeTransform : MonoBehaviour
{
    private Vector3 offset;
    private Quaternion rotation;
    private Transform target;

    void Start()
    {
        target = transform.parent;
        offset = transform.position - target.position;
        rotation = transform.rotation;
        
    }

    void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = rotation;
    }
}
