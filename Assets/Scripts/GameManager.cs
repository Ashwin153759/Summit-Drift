using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private CarInputActions inputActions;
    private LapTimer lapTimer;
    private CarController carController;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new CarInputActions();
        inputActions.Car.Reset.performed += ctx => ResetRace();
    }

    public void LoadRaceScene(string sceneName)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneManager.GetActiveScene().name)
        {
            SetupRace();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void SetupRace()
    {
        lapTimer = FindObjectOfType<LapTimer>();
        carController = FindObjectOfType<CarController>();

        if (lapTimer != null && carController != null)
        {
            carController.SetControlsActive(false);
            StartRace();
        }
    }

    public void StartRace()
    {
        lapTimer.StartLap();

        // Enable Inputs
        inputActions.Enable();
        carController.SetControlsActive(true);
    }

    public void EndRace()
    {
        // Disable Inputs
        inputActions.Disable();
        carController?.SetControlsActive(false);
    }

    public void ResetRace()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
