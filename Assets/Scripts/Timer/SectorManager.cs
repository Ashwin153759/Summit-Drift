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
    private string carName;

    private void Start()
    {
        dataManager = FindObjectOfType<DataManager>();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<CarController>() != null)
            {
                carName = player.name;
                break;
            }
        }

        // Initially hide all sector times
        sector1Text.enabled = false;
        sector2Text.enabled = false;
        sector3Text.enabled = false;

        // Fetch and display best times at the start
        MapRecord bestRecord = dataManager.GetMapRecord(currentMap, carName);
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
                break;
        }
    }

    private void UpdateSectorText(TextMeshProUGUI sectorText, float newTime, int sectorIndex)
    {
        MapRecord record = dataManager.GetMapRecord(currentMap, carName);

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
        MapRecord record = dataManager.GetMapRecord(currentMap, carName);

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
        bool isNewBestLap = dataManager.SaveRecord(currentMap, carName, totalLapTime, sectorTimes);

        // Fetch the updated best times and display them
        MapRecord updatedRecord = dataManager.GetMapRecord(currentMap, carName);
        DisplayBestTimes(updatedRecord);

        if (isNewBestLap)
        {
            Debug.Log("New best lap time and/or sector times recorded!");
        }
    }

    private void DisplayBestTimes(MapRecord record)
    {
        Debug.Log("Best Lap Time: " + record.bestLapTime.ToString("F3"));
        //Debug.Log($"Best Sector 1: {record.bestSectorTimes[0]:F3}");
        //Debug.Log($"Best Sector 2: {record.bestSectorTimes[1]:F3}");
        //Debug.Log($"Best Sector 3: {record.bestSectorTimes[2]:F3}");
    }
}
