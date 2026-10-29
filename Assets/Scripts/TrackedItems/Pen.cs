using UnityEngine;

public class Pen : MonoBehaviour
{
    public RegistryItem item {get; private set;}

    [SerializeField] private Team team;
    [SerializeField] private Zone zone;

    private string ConstructId(){return team.ToString() + zone.ToString();}  
    private void ConstructRegistryItem() => item = new RegistryItem(ConstructId(), gameObject);
    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);


    public Vector3 GetPosition(){ return transform.position;}
    public Bounds GetBounds(){return GetComponent<Renderer>().bounds;}
}