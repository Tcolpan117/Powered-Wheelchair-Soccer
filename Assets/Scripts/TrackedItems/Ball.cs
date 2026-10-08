using UnityEngine;

public class Ball : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item {get; private set;}

    [SerializeField] private string id;

    private void ConstructRegistryItem() => item = new RegistryItem(id, gameObject);


    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item); 

    public Vector3 GetPosition(){return transform.position;}

    public void ResetBall()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        transform.localPosition = Vector3.zero + Vector3.up;
    }
}