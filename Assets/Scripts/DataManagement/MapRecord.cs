[System.Serializable]
public class MapRecord
{
    public string mapName;              
    public float bestLapTime = float.MaxValue;
    public float[] bestSectorTimes;      

    // Initializes with larger values for all sectors
    public MapRecord(string name, int numSectors)
    {
        mapName = name;
        bestSectorTimes = new float[numSectors];
        for (int i = 0; i < numSectors; i++)
        {
            bestSectorTimes[i] = float.MaxValue;
        }
    }
}
