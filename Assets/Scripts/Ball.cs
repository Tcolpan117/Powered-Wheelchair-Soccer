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


    private void ConstructRegistryItem()
    {
        data = new BallData(transform.position);
        item = new RegistryItem(id, data, gameObject);
    }


    void Awake(){ ConstructRegistryItem(); }
    void Start(){ WorldRegister.Instance.Register(item); }
    
    void Update()
    {
        data.location = transform.position;
    }
}
