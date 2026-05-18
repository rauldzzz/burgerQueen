using UnityEngine;

public class PlayerSpot : MonoBehaviour
{
    public bool occupied = false;

    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            occupied = true;
            rend.material.color = Color.green;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            occupied = false;
            rend.material.color = originalColor;
        }
    }
}