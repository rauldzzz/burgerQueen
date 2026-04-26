using UnityEngine;

public class Countertop_Item : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject itemPrefab;   // Drag your ingredient prefab here
    public float giveDelay = 1.5f;

    private float timer = 0f;
    private PlayerInteract playerInside = null;

    void Update()
    {
        if (playerInside == null) return;

        timer += Time.deltaTime;

        if (timer >= giveDelay)
        {
            playerInside.heldIngredients.Add(itemPrefab.name);
            Debug.Log("Countertop gave: " + itemPrefab.name);
            timer = 0f;
            playerInside = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteract player = other.GetComponent<PlayerInteract>();
        if (player != null)
        {
            playerInside = player;
            timer = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteract player = other.GetComponent<PlayerInteract>();
        if (player != null)
        {
            playerInside = null;
            timer = 0f;
        }
    }
}