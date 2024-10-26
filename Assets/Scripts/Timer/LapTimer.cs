using UnityEngine;
using TMPro;

public class LapTimer : MonoBehaviour
{
    public TextMeshProUGUI lapTimeText;
    public bool lapActive = true;
    public float lapTime = 0f;

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
        // Continuously updates the lap time display in 3 decimal format
        lapTimeText.text = "Lap Time: " + lapTime.ToString("F3");
    }

    // Called by SectorManager at lap end
    public void ResetLap()
    {
        lapTime = 0f;
        lapActive = true;
    }

    public void StopLap()
    {
        lapActive = false;
    }
}
