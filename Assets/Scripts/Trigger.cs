using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public static event Action<GameObject, GameObject> TriggerEvent;

    private void OnTriggerEnter(Collider other)
    {
        TriggerEvent?.Invoke(this.gameObject, other.gameObject);
    }

    public static void Raise(GameObject trigger, GameObject cause)
    {
        TriggerEvent?.Invoke(trigger, cause);
    }
}