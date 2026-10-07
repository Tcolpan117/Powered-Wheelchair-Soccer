using System;
using UnityEngine;

public class GoalData
{
    public Team team {get;set;}
    public Vector3 location {get;set;}
    public Vector3 dimentions {get;set;}

    public GoalData(Team team, Vector3 location, Vector3 dimentions)
    {
        this.team = team;
        this.location = location;
        this.dimentions = dimentions;
    }
}

public class Goal : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item {get; private set;}

    [SerializeField] private string id;
    [SerializeField] private Team team;
    private GoalData data;

    public static event Action<string, string> TriggerEvent;


    private void OnTriggerEnter(Collider other)
    {  
        var cause = other.GetComponentInParent<IRegistryProvider>();
        if (cause == null) return;
        TriggerEvent?.Invoke(id, cause.item.id);
    }

    public static void Raise(string trigger, string cause)
    {
        TriggerEvent?.Invoke(trigger, cause);
    }
    

    private void ConstructRegistryItem()
    {
        data = new GoalData(team, transform.position, GetComponentInChildren<MeshRenderer>().bounds.size);
        item = new RegistryItem(id, data, gameObject);
    }

    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);
}