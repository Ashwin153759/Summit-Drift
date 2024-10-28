using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private CarInputActions inputActions;

    private void Awake()
    {
        // Implement singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize input actions in Awake
        inputActions = new CarInputActions();
        inputActions.Car.Reset.performed += ctx => ResetRace();
    }

    public void StartRace()
    {
        inputActions.Enable();
    }

    public void EndRace()
    {
        inputActions.Disable();
    }

    public void ResetRace()
    {
        // Reload the current scene to reset the race
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // place car in starting position again
    }
}
