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
        lapTimer = FindObjectOfType<LapTimer>();
        ghostPlayback = FindObjectOfType<GhostPlayback>();

        currentMap = mapName;
        GameObject startRaceLocationObject = GameObject.Find("StartRaceLocation");

        if (startRaceLocationObject == null)
        {
            Debug.LogError("Cannot find start point of race");
            return;
        }

        InstantiateCarAndGhost(startRaceLocationObject, carName);
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
        if (lapTimer != null && carController != null)
        {
            inputActions.Disable();
            carController.SetControlsActive(false);

            // Start the countdown before starting the race
            StartCoroutine(StartRaceCountdown());
        }
    }

    private IEnumerator StartRaceCountdown()
    {
        int countdownTime = 3;

        while (countdownTime > 0)
        {
            // Display the countdown time (can replace with UI text display)
            Debug.Log(countdownTime);

            // Play beep sound
            //AudioManager.instance.Play("BeepSound");

            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        Debug.Log("Go!");

        // Start the actual race
        StartRace();
    }

    public void StartRace()
    {
        lapTimer.StartLap();
        ghostRecorder.StartRecording();

        inputActions.Enable();
        carController.SetControlsActive(true);

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
        inputActions.Disable();
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
