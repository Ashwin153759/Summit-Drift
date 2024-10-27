using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {


    [SerializeField] private Button startRaceButton;
    [SerializeField] private Button garageButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject settingsMenuUI;


    private void Awake() {
        startRaceButton.onClick.AddListener(() => {
            SceneManager.LoadScene(1);
        });

        garageButton.onClick.AddListener(() => {
            
        });

        settingsButton.onClick.AddListener(() => {
        });

        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }

}
