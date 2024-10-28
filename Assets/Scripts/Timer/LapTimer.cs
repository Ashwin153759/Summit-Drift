using UnityEngine;
using TMPro;

public class LapTimer : MonoBehaviour
{
    public TextMeshProUGUI lapTimeText;
    public bool lapActive = true;
    public float lapTime = 0f;

    private GameManager gameManager;

    private void Start()
    {
        // Find the GameManager instance
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
            gameManager.StartRace();
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
        // Continuously updates the lap time display in 3 decimal format
        lapTimeText.text = "Lap Time: " + lapTime.ToString("F3");
    }

    public void ResetLap()
    {
        lapTime = 0f;
        lapActive = true;
        
        if (gameManager != null)
            gameManager.StartRace();
    }

    public void StopLap()
    {
        lapActive = false;
        
        if (gameManager != null)
            gameManager.EndRace();
    }
}
