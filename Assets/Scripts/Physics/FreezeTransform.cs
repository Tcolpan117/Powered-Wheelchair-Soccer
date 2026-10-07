using UnityEngine;

public class FreezeTransform : MonoBehaviour
{
    private Vector3 offset;
    private Quaternion rotation;
    private Transform target;
    private Vector3 scale;
    private Vector3 parentScale;
    
    void Start()
    {
        target = transform.parent;
        offset = transform.position - target.position;
        rotation = transform.rotation;
        
    }

    void OnValidate(){
        scale = transform.lossyScale;
        parentScale = transform.parent.localScale;
        transform.localScale = new Vector3(
            scale.x / parentScale.x,
            scale.y / parentScale.y,
            scale.z / parentScale.z
        );
    }
    void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = rotation;
        
    }
}
