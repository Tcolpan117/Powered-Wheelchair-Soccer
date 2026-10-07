using UnityEngine;

public class BallData
{
    public Vector3 location {get;set;}

    public BallData(Vector3 location)
    {
        this.location = location;
    }
}
public class Ball : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item {get; private set;}

    [SerializeField] private string id;
    private BallData data;


    public void ResetBall()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        transform.localPosition = Vector3.zero + Vector3.up;
    }

    private void ConstructRegistryItem()
    {
        data = new BallData(transform.position);
        item = new RegistryItem(id, data, gameObject);
    }


    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);
    
    void Update()
    {
        data.location = transform.position;
    }

    
}
