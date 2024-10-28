using UnityEngine;
using TMPro;

public class LapTimer : MonoBehaviour
{
    public TextMeshProUGUI lapTimeText;
    private bool lapActive = false;
    public float lapTime = 0f;

    public void StartLap()
    {
        lapTime = 0f;
        lapActive = true;
    }

    private void Update()
    {
        if (lapActive)
        {
            lapTime += Time.deltaTime;
            DisplayLapTime();
        }
    }

    private void DisplayLapTime()
    {
        lapTimeText.text = "Lap Time: " + lapTime.ToString("F3");
    }

    public void StopLap()
    {
        lapActive = false;
    }
}
