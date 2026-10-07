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

    public T Get<T>() where T : Component => gameObject.GetComponent<T>();
}


public static class WorldRegister
{
    private static readonly Dictionary<string, RegistryItem> _registry = new();
    public static void Register(RegistryItem item) => _registry[item.id] = item;
    public static void UnRegister(RegistryItem item) => _registry.Remove(item.id);
    public static RegistryItem GetItem(string id) => _registry[id];

}
