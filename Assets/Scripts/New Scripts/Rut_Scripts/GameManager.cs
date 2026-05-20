using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Money")]
    public int totalMoney = 0;

    //[Header("Upgrades")]
    //public float grillSpeedMultiplier = 1f;

    //POSAR RECEPTES NOSTRES!!
    //public bool unlockHotDog = false;
    //public bool unlockPizza = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
