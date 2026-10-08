using UnityEngine;

public class PenData
{
    public Team team {get;set;}
    public Vector3 location {get;set;}
    public Bounds boundaries {get;set;}

    public PenData(Team team, Vector3 location, Bounds boundaries)
    {
        this.team = team;
        this.location = location;
        this.boundaries = boundaries;
    }
}

public class Pen : MonoBehaviour
{

    public RegistryItem item {get; private set;}

    [SerializeField] private string id;
    [SerializeField] private Team team;

    private PenData data;

    private void ConstructRegistryItem()
    {
        data = new PenData(team, transform.position, GetComponent<Renderer>().bounds);
        item = new RegistryItem(id, data, gameObject);
    }

    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);
}
