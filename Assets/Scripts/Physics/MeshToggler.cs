using UnityEngine;

public class MeshToggler : MonoBehaviour
{
    [SerializeField] private bool showChildMeshes;

    private void OnValidate()
    {
        MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer mesh in childRenderers)
        {
            if (mesh.gameObject != gameObject)
            {
                mesh.enabled = showChildMeshes;
            }
        }
    }
}
