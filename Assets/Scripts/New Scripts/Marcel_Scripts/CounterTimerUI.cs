using UnityEngine;
using UnityEngine.UI;

public class CounterTimerUI : MonoBehaviour
{
    public Image fillImage;
    public GameObject timerObject;

    void Start()
    {
        Hide();
    }

    public void Show()
    {
        timerObject.SetActive(true);
    }

    public void Hide()
    {
        timerObject.SetActive(false);
        if (fillImage != null)
            fillImage.fillAmount = 0f;
    }

    public void UpdateFill(float current, float max)
    {
        if (max <= 0f)
        {
            fillImage.fillAmount = 1f;
            return;
        }

        fillImage.fillAmount = current / max;
    }
}