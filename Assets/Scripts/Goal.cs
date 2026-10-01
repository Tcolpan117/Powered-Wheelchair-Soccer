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

public class Goal : MonoBehaviour
{

    [SerializeField] private string id;
    [SerializeField] private Team team;

    private GoalData data;

    public static event Action<GameObject, GameObject> TriggerEvent;

    private void OnTriggerEnter(Collider other)
    {  
        TriggerEvent?.Invoke(gameObject, other.gameObject);
    }

    public static void Raise(GameObject trigger, GameObject cause)
    {
        TriggerEvent?.Invoke(trigger, cause);
    }
    

    private RegistryItem ConstructRegistryItem()
    {
        data = new GoalData(team, transform.position, GetComponentInChildren<MeshRenderer>().bounds.size);
        return new RegistryItem(id, data);
    }

    void Start()
    {
        WorldRegister.Instance.Register(ConstructRegistryItem());
    }
}