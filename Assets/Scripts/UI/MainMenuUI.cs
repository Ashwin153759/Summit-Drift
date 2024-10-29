using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startRaceButton;
    [SerializeField] private Button garageButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [SerializeField]

    public enum CarType
    {
        Marauder,
        Cyclone,
        Solaris
    }

    [SerializeField] private CarType selectedCar;

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.Log("GameManager NOT FOUND");
            return;
        }

        startRaceButton.onClick.AddListener(() =>
        {
            string selectedScene = "Main";
            string selectedCarName = selectedCar.ToString();

            gameManager.SetSelectedCarName(selectedCarName); // garage does this in future
            gameManager.LoadRaceScene(selectedScene); 
        });

        garageButton.onClick.AddListener(() => { /* Garage logic */ });
        settingsButton.onClick.AddListener(() => { /* Settings logic */ });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
