using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private CarInputActions inputActions;
    private LapTimer lapTimer;
    private CarController carController;
    private GhostRecorder ghostRecorder;
    private GhostPlayback ghostPlayback;

    private string ghostDataDirectory;
    private string selectedCarName;
    private string currentMap;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize input actions
        inputActions = new CarInputActions();
        inputActions.Car.Reset.performed += ctx => ResetRace();

        // Set up directory for ghost data
        ghostDataDirectory = Path.Combine(Application.persistentDataPath, "GhostData");
        if (!Directory.Exists(ghostDataDirectory))
        {
            Directory.CreateDirectory(ghostDataDirectory);
        }
    }

    public void SetSelectedCarName(string carName)
    {
        selectedCarName = carName;
    }

    public string GetSelectedCarName()
    {
        return selectedCarName;
    }

    public void LoadRaceScene(string sceneName)
    {
        // Pass the selectedCarName to OnSceneLoaded
        SceneManager.sceneLoaded += (scene, mode) => OnSceneLoaded(scene, selectedCarName);
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, string carName)
    {
        if (scene.name == SceneManager.GetActiveScene().name)
        {
            SetupRace(scene.name, carName);
            SceneManager.sceneLoaded -= (scene, mode) => OnSceneLoaded(scene, carName);
        }
    }

    private void SetupRace(string mapName, string carName)
    {
        lapTimer = FindObjectOfType<LapTimer>();
        carController = FindObjectOfType<CarController>();

        ghostRecorder = FindObjectOfType<GhostRecorder>();
        ghostPlayback = FindObjectOfType<GhostPlayback>();

        currentMap = mapName;

        // SPAWN CAR && GHOST

        if (lapTimer != null && carController != null)
        {
            carController.SetControlsActive(false);
            StartRace();
        }
    }

    public void StartRace()
    {
        Debug.Log("Race Starts!");

        // Start recording and enable controls
        lapTimer.StartLap();
        ghostRecorder.StartRecording();

        inputActions.Enable();
        carController.SetControlsActive(true);

        // Start the Ghost Playback
        if (ghostRecorder != null && ghostPlayback != null)
        {
            Debug.Log(GetGhostFilePath(currentMap, selectedCarName));
            GhostData bestLapData = ghostRecorder.LoadLapData(GetGhostFilePath(currentMap, selectedCarName));
            if (bestLapData != null)
            {
                StartCoroutine(DelayedStartGhostPlayback(bestLapData, ghostRecorder.recordInterval));
            }
        }
    }

    private IEnumerator DelayedStartGhostPlayback(GhostData bestLapData, float interval)
    {
        // Adjust delay if necessary
        yield return new WaitForSeconds(0.1f);
        ghostPlayback.StartPlayback(bestLapData, interval);
    }

    public void EndRace(string mapName, string carName)
    {
        Debug.Log("Race Ends!");

        // Stop recording and disable controls
        GhostData recordedLap = ghostRecorder.StopRecording();
        lapTimer.StopLap();
        inputActions.Disable();
        carController.SetControlsActive(false);

        // Check if this lap is the best lap and save it if true
        if (IsBestLap(recordedLap, mapName, carName))
        {
            ghostRecorder.SaveLapData(recordedLap, GetGhostFilePath(mapName, carName));
            Debug.Log("New Best Lap Saved for " + mapName + " - " + carName);
        }

        // Disable ghost playback when the race ends
        ghostPlayback.StopPlayback();
    }

    private bool IsBestLap(GhostData newLapData, string mapName, string carName)
    {
        GhostData bestLapData = ghostRecorder.LoadLapData(GetGhostFilePath(mapName, carName));

        // No saved lap, so this is the best by default
        if (bestLapData == null) return true;

        // Compare lap time or number of frames
        return newLapData.positions.Count < bestLapData.positions.Count;
    }

    private string GetGhostFilePath(string mapName, string carName)
    {
        string fileName = $"{mapName}_{carName}_BestLap.json";
        return Path.Combine(ghostDataDirectory, fileName);
    }

    public void ResetRace()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
