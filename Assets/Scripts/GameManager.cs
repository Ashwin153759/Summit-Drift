using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    #region Fields and References

    [SerializeField] private GameObject[] carPrefabs;
    [SerializeField] private GameObject[] ghostPrefabs;

    private static GameManager instance;
    private CarInputActions inputActions;
    private LapTimer lapTimer;
    private CarController carController;
    private GhostRecorder ghostRecorder;
    private GhostPlayback ghostPlayback;

    private string ghostDataDirectory;
    private string selectedCarName;
    private string currentMap;
    private Transform startRaceLocation;
    private CountdownManager countdownManager;

    #endregion

    #region Initialization

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeInputActions();
        SetupGhostDataDirectory();
    }

    private void InitializeInputActions()
    {
        inputActions = new CarInputActions();
        inputActions.Car.Reset.performed += ctx => ResetRace();
    }

    private void SetupGhostDataDirectory()
    {
        ghostDataDirectory = Path.Combine(Application.persistentDataPath, "GhostData");
        if (!Directory.Exists(ghostDataDirectory))
        {
            Directory.CreateDirectory(ghostDataDirectory);
        }
    }

    #endregion

    #region Car Selection and Scene Loading

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

    #endregion

    #region Race Setup and Start

    private void SetupRace(string mapName, string carName)
    {
        currentMap = mapName;

        // Get the references of scene dependencies
        GameObject startRaceLocationObject = GameObject.Find("StartRaceLocation");

        lapTimer = FindObjectOfType<LapTimer>();
        countdownManager = FindObjectOfType<CountdownManager>();

        // Null Checks
        if (startRaceLocationObject == null) Debug.LogError("Cannot find start point of race");
        if (countdownManager == null) Debug.LogError("Cannot find Countdown Manager");
        if (lapTimer == null) Debug.LogError("Cannot find laptimer");

        // Spawn Player Car & Ghost
        InstantiateCarAndGhost(startRaceLocationObject, carName);

        // Handle Inputs
        inputActions.Enable(); // TODO: need to turn off reset btn when we are not in racemode
        carController.SetControlsActive(false);

        InitializeRace();
    }

    private void InstantiateCarAndGhost(GameObject startRaceLocationObject, string carName)
    {
        GameObject carPrefab = GetCarPrefabByName(carName, carPrefabs);
        if (carPrefab == null)
        {
            Debug.LogError("Car not found");
            return;
        }

        GameObject carInstance = Instantiate(carPrefab, startRaceLocationObject.transform.position, startRaceLocationObject.transform.rotation);
        carController = carInstance.GetComponent<CarController>();
        ghostRecorder = carInstance.GetComponent<GhostRecorder>();

        GhostData bestLapData = ghostRecorder.LoadLapData(GetGhostFilePath(currentMap, selectedCarName));
        if (bestLapData != null)
        {
            GameObject ghostPrefab = GetCarPrefabByName(carName + "_Ghost", ghostPrefabs);
            if (ghostPrefab == null)
            {
                Debug.LogError("Ghost Car not found");
                return;
            }

            GameObject ghostInstance = Instantiate(ghostPrefab, startRaceLocationObject.transform.position, startRaceLocationObject.transform.rotation);
            ghostPlayback = ghostInstance.GetComponent<GhostPlayback>();
        }
    }

    private void InitializeRace()
    {
        // Start Countdown 
        countdownManager.OnCountdownComplete += StartRace;
        countdownManager.StartCountdown(); 
    }

    public void StartRace()
    {
        countdownManager.OnCountdownComplete -= StartRace;

        // Start the Lap Timer, and Recording Ghost
        lapTimer.StartLap();
        ghostRecorder.StartRecording();

        // Enable Controls
        carController.SetControlsActive(true);

        // Start Ghost Playback
        if (ghostRecorder != null && ghostPlayback != null)
        {
            GhostData bestLapData = ghostRecorder.LoadLapData(GetGhostFilePath(currentMap, selectedCarName));
            StartCoroutine(DelayedStartGhostPlayback(bestLapData, ghostRecorder.recordInterval));
        }
    }

    private IEnumerator DelayedStartGhostPlayback(GhostData bestLapData, float interval)
    {
        yield return new WaitForSeconds(0.11f);
        ghostPlayback.StartPlayback(bestLapData, interval);
    }

    #endregion

    #region End Race and Reset

    public void EndRace(string mapName, string carName)
    {
        GhostData recordedLap = ghostRecorder.StopRecording();
        lapTimer.StopLap();
        
        carController.SetControlsActive(false);

        if (IsBestLap(recordedLap, mapName, carName))
        {
            ghostRecorder.SaveLapData(recordedLap, GetGhostFilePath(mapName, carName));
            Debug.Log("New Best Lap Saved for " + mapName + " - " + carName);
        }

        ghostPlayback?.StopPlayback();
    }

    private bool IsBestLap(GhostData newLapData, string mapName, string carName)
    {
        GhostData bestLapData = ghostRecorder.LoadLapData(GetGhostFilePath(mapName, carName));
        if (bestLapData == null) return true;

        return newLapData.positions.Count < bestLapData.positions.Count;
    }

    private string GetGhostFilePath(string mapName, string carName)
    {
        string fileName = $"{mapName}_{carName}_BestLap.json";
        return Path.Combine(ghostDataDirectory, fileName);
    }

    private GameObject GetCarPrefabByName(string carName, GameObject[] list)
    {
        foreach (var prefab in list)
        {
            if (prefab.name == carName)
            {
                return prefab;
            }
        }
        return null;
    }

    public void ResetRace()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion
}
