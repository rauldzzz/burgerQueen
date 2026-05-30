using UnityEngine;

public class RestartButton : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameManager.Instance.RestartGame();
    }
}