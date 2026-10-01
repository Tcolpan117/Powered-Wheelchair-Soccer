using System.Collections.Generic;
using UnityEngine;

public static class RegistryIndex
{
    
}
public class RegistryItem
{
    public string identifier {get;}
    public object data {get;}

    public RegistryItem(string identifier, object data)
    {
        this.identifier = identifier;
        this.data = data;
    }
}


public class WorldRegister : MonoBehaviour
{
    public static WorldRegister Instance {get; private set;}

    private Dictionary<string, RegistryItem> _registry = new Dictionary<string, RegistryItem>();

     void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(RegistryItem item)
    {
        _registry[item.identifier] = item;
    }

    public void UnRegister(RegistryItem item)
    {
        _registry.Remove(item.identifier);
    }

    public T GetItem<T>(string identifier) where T : class
    {
        if (_registry.TryGetValue(identifier, out RegistryItem registryItem)){
            return registryItem.data as T;
        }
        return null;
    }
}
