using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FabricArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PaperSurface paper = other.GetComponent<PaperSurface>() 
                             ?? other.GetComponentInParent<PaperSurface>();

        if (paper == null)
            return;

        paper.SetOnFabric(true);
    }

    private void OnTriggerExit(Collider other)
    {
        PaperSurface paper = other.GetComponent<PaperSurface>() 
                             ?? other.GetComponentInParent<PaperSurface>();

        if (paper == null)
            return;

        paper.SetOnFabric(false);
    }
}