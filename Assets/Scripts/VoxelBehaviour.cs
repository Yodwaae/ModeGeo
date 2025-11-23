using UnityEngine;

public class VoxelBehaviour : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    private void OnTriggerEnter(Collider other) { meshRenderer.enabled = false; }

    private void OnTriggerExit(Collider other) { meshRenderer.enabled = true; }
}
