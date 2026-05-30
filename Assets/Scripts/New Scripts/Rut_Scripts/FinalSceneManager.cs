using TMPro;
using UnityEngine;

public class FinalSceneManager : MonoBehaviour
{
    [Header("Burger Quantities")]
    public TextMeshProUGUI singleBurgerQuantity;
    public TextMeshProUGUI cheeseBurgerQuantity;
    public TextMeshProUGUI doubleCheeseBurgerQuantity;
    public TextMeshProUGUI completeBurgerQuantity;
    public TextMeshProUGUI tomatonatorQuantity;
    public TextMeshProUGUI vegetalBurgerQuantity;

    [Header("Money")]
    public TextMeshProUGUI generatedMoneyQuantity;
    public TextMeshProUGUI spentMoneyQuantity;
    public TextMeshProUGUI finalMoneyQuantity;

    private void Start()
    {
        if (SessionStatistics.Instance == null)
        {
            Debug.LogError("SessionStatistics not found.");
            return;
        }

        SessionStatistics stats = SessionStatistics.Instance;

        singleBurgerQuantity.text =
            "x" + stats.GetBurgerCount("SingleBurger").ToString();

        cheeseBurgerQuantity.text =
            "x" + stats.GetBurgerCount("CheeseBurger").ToString();

        doubleCheeseBurgerQuantity.text =
            "x" + stats.GetBurgerCount("DoubleCheeseBurger").ToString();

        completeBurgerQuantity.text =
            "x" + stats.GetBurgerCount("CompleteBurger").ToString();

        tomatonatorQuantity.text =
            "x" + stats.GetBurgerCount("Tomatonator").ToString();

        vegetalBurgerQuantity.text =
            "x" + stats.GetBurgerCount("VegetalBurger").ToString();

        generatedMoneyQuantity.text =
            stats.totalMoneyEarned.ToString() + "$";

        spentMoneyQuantity.text =
            "-" + stats.totalMoneySpent.ToString() + "$";

        finalMoneyQuantity.text =
            stats.GetRemainingMoney().ToString() + "$";
    }
}
