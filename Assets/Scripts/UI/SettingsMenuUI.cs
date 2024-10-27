using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour {


    [SerializeField] private Button audioButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button graphicsButton;
    [SerializeField] private Button returnToMainMenuButton;

    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject audioSettingsUI;


    private void Start() {
    }

    private void Awake() {

        controlsButton.onClick.AddListener(() => {
        });

        graphicsButton.onClick.AddListener(() => {
        });

    }

}
