using System;
using UnityEngine;

public class Goal : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item {get; private set;}

    [SerializeField] private Team team;
    [SerializeField] private Zone zone;

    public static event Action<string, string> TriggerEvent;

    private string ConstructId(){return team.ToString() + zone.ToString();}  
    private void ConstructRegistryItem() => item = new RegistryItem(ConstructId(), gameObject);

    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);

    private void OnTriggerEnter(Collider other)
    {  
        var cause = other.GetComponentInParent<IRegistryProvider>();
        if (cause == null) return;
        TriggerEvent?.Invoke(ConstructId(), cause.item.id);
    }


    public Vector3 GetPosition() {return transform.position;}
    public Vector3 GetDimentions() {return GetComponentInChildren<MeshRenderer>().bounds.size;}

    public static void Raise(string trigger, string cause)
    {
        TriggerEvent?.Invoke(trigger, cause);
    }
}