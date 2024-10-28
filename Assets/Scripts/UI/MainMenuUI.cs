using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startRaceButton;
    [SerializeField] private Button garageButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

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
            // hard coded for now 
            string selectedScene = "Main";
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
