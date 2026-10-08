using UnityEngine;

public class Pen : MonoBehaviour
{
    public RegistryItem item {get; private set;}


    [SerializeField] private string id;
    [SerializeField] private Team team;


    private void ConstructRegistryItem() => item = new RegistryItem(id, gameObject);
    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);


    public Vector3 GetPosition(){ return transform.position;}
    public Bounds GetBounds(){return GetComponent<Renderer>().bounds;}
}