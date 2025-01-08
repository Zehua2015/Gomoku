using UnityEngine;
using UnityEngine.UI;

public class Time_Bar : MonoBehaviour
{
    public Slider timerSlider;
    // public float maxTime = 20f;

    void Start()
    {

    }
    public void InitializeTimerBar(float maxTime)
    {
        timerSlider.maxValue = maxTime;
        timerSlider.value = maxTime;
    }

    public void UpdateTimerBar(float currentTime)
    {
        timerSlider.value = currentTime;
    }
}
