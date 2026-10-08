using UnityEngine;

public class FieldData
{
    public Team team {get;set;}
    public Vector3 location {get;set;}
    public Bounds boundaries {get;set;}

    public FieldData(Team team, Vector3 location, Bounds boundaries)
    {
        this.team = team;
        this.location = location;
        this.boundaries = boundaries;
    }
}

public class Field : MonoBehaviour
{

    public RegistryItem item {get; private set;}

    [SerializeField] private string id;
    [SerializeField] private Team team;

    private FieldData data;

    private void ConstructRegistryItem()
    {
        data = new FieldData(team, transform.position, GetComponent<Renderer>().bounds);
        item = new RegistryItem(id, data, gameObject);
    }

    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);
}
