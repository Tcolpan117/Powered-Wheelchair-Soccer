using UnityEngine;

public class BallData
{
    public Vector3 location {get;set;}

    public BallData(Vector3 location)
    {
        this.location = location;
    }
}
public class Ball : MonoBehaviour
{
    [SerializeField] private string id;
    private BallData data;

    private RegistryItem ConstructRegistryItem()
    {
        data = new BallData(transform.position);
        return new RegistryItem(id, data);
    }
    void Start()
    {
        WorldRegister.Instance.Register(ConstructRegistryItem());
    }

    void Update()
    {
        data.location = transform.position;
    }
}
