using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coins = 0;

    void Awake()
    {
        Instance = this;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log("Coins: " + coins);
    }
}
