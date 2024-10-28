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
                sector1Text.text = "Sector 1: " + sector1Time.ToString("F3");
                sector1Text.enabled = true;
                currentSector = 2;
                break;

            case 2:
                sector2Time = currentLapTime - sector1Time;
                sector2Text.text = "Sector 2: " + sector2Time.ToString("F3");
                sector2Text.enabled = true;
                currentSector = 3;
                break;

            case 3:
                sector3Time = currentLapTime - sector1Time - sector2Time;
                sector3Text.text = "Sector 3: " + sector3Time.ToString("F3");
                sector3Text.enabled = true;

                // Stop the lap timer
                lapTimer.StopLap();

                // Calculate total lap time and update records
                float totalLapTime = lapTimer.lapTime;
                SaveAndDisplayBestTimes(totalLapTime);

                break;
        }
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
        
        //for (int i = 0; i < record.bestSectorTimes.Length; i++)
        //{
        //    Debug.Log($"Best Sector {i + 1} Time: {record.bestSectorTimes[i].ToString("F3")}");
        //}

        // sector1Text.text = "Best Sector 1: " + record.bestSectorTimes[0].ToString("F3");
        // sector2Text.text = "Best Sector 2: " + record.bestSectorTimes[1].ToString("F3");
        // sector3Text.text = "Best Sector 3: " + record.bestSectorTimes[2].ToString("F3");
    }
}
