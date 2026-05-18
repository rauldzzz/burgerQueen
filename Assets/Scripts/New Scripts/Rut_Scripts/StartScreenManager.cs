using UnityEngine;

public class StartScreenManager : MonoBehaviour
{
    public PlayerSpot spot1;
    public PlayerSpot spot2;

    private bool gameStarted = false;

    void Update()
    {
        if (!gameStarted &&
            spot1.occupied &&
            spot2.occupied)
        {
            gameStarted = true;
            StartGame();
        }
    }

    void StartGame()
    {
        Debug.Log("GAME START");

        // aquí podeu:
        // amagar el canvas
        // començar temporitzador
        // activar controls
        // etc.
    }
}
