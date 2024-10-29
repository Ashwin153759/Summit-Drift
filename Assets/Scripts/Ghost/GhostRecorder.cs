using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GhostData
{
    public List<Vector3> positions = new List<Vector3>();
    public List<Quaternion> rotations = new List<Quaternion>();
}

public class GhostRecorder : MonoBehaviour
{
    public float recordInterval = 0.1f;

    private float recordTimer;
    private GhostData currentLapData;
    private bool isRecording;

    public void StartRecording()
    {
        Debug.Log("Started Recording");
        currentLapData = new GhostData();
        isRecording = true;
        recordTimer = 0;
    }

    public GhostData StopRecording()
    {
        Debug.Log("Stopped Recording");
        isRecording = false;
        return currentLapData;
    }

    private void Update()
    {
        if (!isRecording) return;

        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0;
            currentLapData.positions.Add(transform.position);
            currentLapData.rotations.Add(transform.rotation);
        }
    }

    public void SaveLapData(GhostData data, string filePath)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    public GhostData LoadLapData(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<GhostData>(json);
    }
}
