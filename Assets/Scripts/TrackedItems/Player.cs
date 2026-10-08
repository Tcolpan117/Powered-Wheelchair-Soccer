using UnityEngine;

public class PlayerData
{
    public Vector3 location {get;set;}
    public Team team {get;set;}
    public Role role {get;set;}
    public PlayerData(Vector3 location, Role role, Team team)
    {
        this.team = team;
        this.role = role;
        this.location = location;
    }
}
public class Player : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item {get; private set;}

    [SerializeField] private string id;
    [SerializeField] private Team team;
    [SerializeField] private Role role;
    private PlayerData data;


    public void ResetPlayer()
    {
        RegistryItem item;
        FieldData fdat;
        PenData pdat;

        switch (id)
        {
            case "AwayLeftFielder":
                item = WorldRegister.GetItem("AwayLeftField");
                fdat = (FieldData)item.data;
                
                transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                transform.localPosition = fdat.location;
                break;

            case "AwayRightFielder":
                item = WorldRegister.GetItem("AwayRightField");
                fdat = (FieldData)item.data;

                transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                transform.localPosition = fdat.location;
                break;

            case "HomeLeftFielder":
                item = WorldRegister.GetItem("HomeLeftField");
                fdat = (FieldData)item.data;

                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                transform.localPosition = fdat.location;
                break;

            case "HomeRightFielder":
                item = WorldRegister.GetItem("HomeRightField");
                fdat = (FieldData)item.data;

                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                transform.localPosition = fdat.location;
                break;

            case "HomeGoaly":
                item = WorldRegister.GetItem("HomePen");
                pdat = (PenData)item.data;
                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                transform.localPosition = pdat.location;
                break;

            case "AwayGoaly":
                item = WorldRegister.GetItem("AwayPen");
                pdat = (PenData)item.data;

                transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                transform.localPosition = pdat.location;
                break;
        }
    }

    private void ConstructRegistryItem()
    {
        data = new PlayerData(transform.position, role, team);
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
