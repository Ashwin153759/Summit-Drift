using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;

    public float bestSessionLapTime = float.MaxValue;
    public float[] bestSessionSectorTimes;

    private int numSectors;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(int sectors)
    {
        numSectors = sectors;
        bestSessionSectorTimes = new float[numSectors];
        for (int i = 0; i < numSectors; i++)
        {
            bestSessionSectorTimes[i] = float.MaxValue;
        }
    }

    public void UpdateBestSessionTimes(float lapTime, float[] sectorTimes)
    {
        if (lapTime < bestSessionLapTime)
        {
            bestSessionLapTime = lapTime;
        }

        for (int i = 0; i < numSectors; i++)
        {
            if (sectorTimes[i] < bestSessionSectorTimes[i])
            {
                bestSessionSectorTimes[i] = sectorTimes[i];
            }
        }
    }
}

