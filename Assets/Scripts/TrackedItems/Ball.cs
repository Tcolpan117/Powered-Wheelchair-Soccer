using UnityEngine;

public class Ball : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item {get; private set;}
    [SerializeField] private Prop prop;
    [SerializeField] private Zone zone;

    private string ConstructId(){return prop.ToString();}  
    private void ConstructRegistryItem() => item = new RegistryItem(ConstructId(), gameObject);


    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item); 

    public Vector3 GetPosition(){return transform.position;}

    public void ResetBall()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        transform.localPosition = WorldRegister.GetItem(zone.ToString()).Get<Center>().GetPosition();
    }
}