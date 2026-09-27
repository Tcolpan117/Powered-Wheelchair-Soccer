using System.Collections.Generic;
using UnityEngine;

public static class RegistryIndex
{
    
}
public interface RegistryItem
{
    string name {get;}
    object item {get;}
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
        _registry[item.name] = item;
    }

    public void UnRegister(RegistryItem item)
    {
        _registry.Remove(item.name);
    }

    public T GetItem<T>(string name) where T : class
    {
        if (_registry.TryGetValue(name, out RegistryItem registryItem)){
            return registryItem.item as T;
        }
        return null;
    }
}
