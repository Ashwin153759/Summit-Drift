using UnityEngine;
using TMPro;

public class SectorManager : MonoBehaviour
{
    public LapTimer lapTimer;
    public TextMeshProUGUI sector1Text, sector2Text, sector3Text;

    private float sector1Time, sector2Time, sector3Time;
    private int currentSector = 1;

    private void Start()
    {
        // Initially hide all sector times
        sector1Text.enabled = false;
        sector2Text.enabled = false;
        sector3Text.enabled = false;
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
                sector1Text.enabled = true;  // Show Sector 1 time
                currentSector = 2;
                break;

            case 2:
                sector2Time = currentLapTime - sector1Time;
                sector2Text.text = "Sector 2: " + sector2Time.ToString("F3");
                sector2Text.enabled = true;  // Show Sector 2 time
                currentSector = 3;
                break;

            case 3:
                sector3Time = currentLapTime - sector1Time - sector2Time;
                sector3Text.text = "Sector 3: " + sector3Time.ToString("F3");
                sector3Text.enabled = true;  // Show Sector 3 time

                // Stop the lap timer and reset for the next lap
                lapTimer.StopLap();
                break;
        }
    }
}
