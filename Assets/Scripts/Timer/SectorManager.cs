using UnityEngine;
using TMPro;

public class SectorManager : MonoBehaviour
{
    [SerializeField] private string currentMap = "MapName";

    public LapTimer lapTimer;
    public TextMeshProUGUI sector1Text, sector2Text, sector3Text;

    private float sector1Time, sector2Time, sector3Time;
    private int currentSector = 1;

    private DataManager dataManager;

    private void Start()
    {
        dataManager = FindObjectOfType<DataManager>();

        // Initially hide all sector times
        sector1Text.enabled = false;
        sector2Text.enabled = false;
        sector3Text.enabled = false;

        // Fetch and display best times at the start
        MapRecord bestRecord = dataManager.GetMapRecord(currentMap, 3);
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

                // Stop the lap timer
                lapTimer.StopLap();

                // Calculate total lap time and update records
                float totalLapTime = lapTimer.lapTime;
                UpdateLapTimeText(totalLapTime);
                SaveAndDisplayBestTimes(totalLapTime);

                currentSector = 1;

                break;
        }
    }

    private void UpdateSectorText(TextMeshProUGUI sectorText, float newTime, int sectorIndex)
    {
        MapRecord record = dataManager.GetMapRecord(currentMap, 3);
        bool improved = newTime < record.bestSectorTimes[sectorIndex];

        // Update text color based on whether it improved
        sectorText.color = improved ? Color.green : Color.red;
        sectorText.text = $"Sector {sectorIndex + 1}: " + newTime.ToString("F3");
        sectorText.enabled = true;
    }

    private void UpdateLapTimeText(float newLapTime)
    {
        MapRecord record = dataManager.GetMapRecord(currentMap, 3);
        bool isNewBestLap = newLapTime < record.bestLapTime;

        TextMeshProUGUI lapTimeText = lapTimer.lapTimeText;

        // Set lap time color based on improvement
        lapTimeText.color = isNewBestLap ? Color.green : Color.red;
        lapTimeText.text = "Lap Time: " + newLapTime.ToString("F3");
        lapTimeText.enabled = true;
    }

    private void SaveAndDisplayBestTimes(float totalLapTime)
    {
        float[] sectorTimes = { sector1Time, sector2Time, sector3Time };

        // Check if this lap is better and save if necessary
        bool isNewBestLap = dataManager.SaveRecord(currentMap, totalLapTime, sectorTimes);

        // Fetch the updated best times and display them
        MapRecord updatedRecord = dataManager.GetMapRecord(currentMap, 3);
        DisplayBestTimes(updatedRecord);

        if (isNewBestLap)
        {
            Debug.Log("New best lap time and/or sector times recorded!");
        }
    }

    private void DisplayBestTimes(MapRecord record)
    {
        Debug.Log("Best Lap Time: " + record.bestLapTime.ToString("F3"));

        // Display best times for each sector
        Debug.Log($"Best Sector 1: {record.bestSectorTimes[0]:F3}");
        Debug.Log($"Best Sector 2: {record.bestSectorTimes[1]:F3}");
        Debug.Log($"Best Sector 3: {record.bestSectorTimes[2]:F3}");
    }
}
