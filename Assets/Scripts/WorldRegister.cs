using System.Collections.Generic;
using UnityEngine;

public interface IRegistryProvider{RegistryItem item {get;}}

public class RegistryItem
{
    public string id {get;}
    public object data {get;}
    public GameObject gameObject {get;}
    public RegistryItem(string id, object data, GameObject gameObject)
    {
        this.id = id;
        this.data = data;
        this.gameObject = gameObject;
    }
}


public class WorldRegister : MonoBehaviour
{
    public static WorldRegister Instance {get; private set;}
    private Dictionary<string, RegistryItem> _registry = new Dictionary<string, RegistryItem>();

    public void Register(RegistryItem item)
    {
        _registry[item.id] = item;
    }

    public void UnRegister(RegistryItem item)
    {
        _registry.Remove(item.id);
    }

    public static RegistryItem GetItem(string id)
    {
        if (Instance._registry.TryGetValue(id, out RegistryItem registryItem)){
            return registryItem;
        }
        return null;
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
