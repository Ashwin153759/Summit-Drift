using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SectorManager : MonoBehaviour
{
    public LapTimer lapTimer;
    public TextMeshProUGUI sector1Text, sector2Text, sector3Text;
    private float sector1Time, sector2Time, sector3Time;
    private int currentSector = 1;

    private DataManager dataManager;
    private GameManager gameManager;
    private string carName;

    // Get the Map name
    private string CurrentMap => SceneManager.GetActiveScene().name;

    private void Start()
    {
        dataManager = FindObjectOfType<DataManager>();
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found");
            return;
        }

        // Fetch car name from GameManager
        carName = gameManager.GetSelectedCarName();

        // Initially hide all sector times
        sector1Text.enabled = false;
        sector2Text.enabled = false;
        sector3Text.enabled = false;

        // Fetch and display best times at the start
        MapRecord bestRecord = dataManager.GetMapRecord(CurrentMap, carName);
        DisplayBestTimes(bestRecord);
    }

    public void RecordSectorTime(int sectorNumber)
    {
        if (sectorNumber != currentSector)
        {
            Debug.Log($"Skipped sector {sectorNumber}. Expected sector: {currentSector}");
            return;
        }

        float currentLapTime = lapTimer.lapTime;

        switch (sectorNumber)
        {
            case 1:
                sector1Time = currentLapTime;
                UpdateSectorText(sector1Text, sector1Time, 0);
                currentSector = 2;
                break;

            case 2:
                sector2Time = currentLapTime - sector1Time;
                UpdateSectorText(sector2Text, sector2Time, 1);
                currentSector = 3;
                break;

            case 3:
                sector3Time = currentLapTime - sector1Time - sector2Time;
                UpdateSectorText(sector3Text, sector3Time, 2);
                lapTimer.StopLap();

                float totalLapTime = lapTimer.lapTime;
                UpdateLapTimeText(totalLapTime);

                SaveAndDisplayBestTimes(totalLapTime);
                currentSector = 1;

                // End the race with the current map and car name
                gameManager.EndRace(CurrentMap, carName);

                break;
        }
    }

    private void UpdateSectorText(TextMeshProUGUI sectorText, float newTime, int sectorIndex)
    {
        MapRecord record = dataManager.GetMapRecord(CurrentMap, carName);

        // Check if there’s an existing best time to compare
        if (record.bestSectorTimes[sectorIndex] < float.MaxValue)
        {
            bool improved = newTime < record.bestSectorTimes[sectorIndex];
            float delta = newTime - record.bestSectorTimes[sectorIndex];

            // Update text color based on improvement and show delta
            sectorText.color = improved ? Color.green : Color.red;
            sectorText.text = $"Sector {sectorIndex + 1}: {newTime:F3} ({(improved ? "-" : "+")}{Mathf.Abs(delta):F3})";
        }
        else
        {
            // Display the time without delta if no previous record
            sectorText.color = Color.white;
            sectorText.text = $"Sector {sectorIndex + 1}: {newTime:F3}";
        }

        sectorText.enabled = true;
    }

    private void UpdateLapTimeText(float newLapTime)
    {
        MapRecord record = dataManager.GetMapRecord(CurrentMap, carName);

        // Only show delta if there’s an existing best lap time
        if (record.bestLapTime < float.MaxValue)
        {
            bool isNewBestLap = newLapTime < record.bestLapTime;
            float delta = newLapTime - record.bestLapTime;

            // Update lap time color based on improvement and show delta
            TextMeshProUGUI lapTimeText = lapTimer.lapTimeText;
            lapTimeText.color = isNewBestLap ? Color.green : Color.red;
            lapTimeText.text = $"Lap Time: {newLapTime:F3} ({(isNewBestLap ? "-" : "+")}{Mathf.Abs(delta):F3})";
        }
        else
        {
            // Display the lap time without delta if no previous record
            lapTimer.lapTimeText.color = Color.white;
            lapTimer.lapTimeText.text = $"Lap Time: {newLapTime:F3}";
        }
    }

    private void SaveAndDisplayBestTimes(float totalLapTime)
    {
        float[] sectorTimes = { sector1Time, sector2Time, sector3Time };
        bool isNewBestLap = dataManager.SaveRecord(CurrentMap, carName, totalLapTime, sectorTimes);

        // Fetch the updated best times and display them
        MapRecord updatedRecord = dataManager.GetMapRecord(CurrentMap, carName);
        DisplayBestTimes(updatedRecord);

        if (isNewBestLap)
        {
            Debug.Log("New best lap time and/or sector times recorded!");
        }
    }

    private void DisplayBestTimes(MapRecord record)
    {
        Debug.Log("Best Lap Time: " + record.bestLapTime.ToString("F3"));
    }
}
