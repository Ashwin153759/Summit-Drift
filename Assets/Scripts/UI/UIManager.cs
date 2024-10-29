using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject audioSettingsUI;
    [SerializeField] private GameObject controlsUI;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu() {
        mainMenuUI.SetActive(true);
        settingsUI.SetActive(false);
        audioSettingsUI.SetActive(false);

        //make sure no button stays highlighted when swithing from menu to menu
        ResetAllButtonStates();
    }

    public void ShowSettings() {
        mainMenuUI.SetActive(false);
        settingsUI.SetActive(true);
        audioSettingsUI.SetActive(false);
        controlsUI.SetActive(false);

        ResetAllButtonStates();
    }

    public void ShowAudioSettings() {
        mainMenuUI.SetActive(false);
        settingsUI.SetActive(false);
        audioSettingsUI.SetActive(true);
        controlsUI.SetActive(false);

        ResetAllButtonStates();
    }

    public void ShowControls() {
        mainMenuUI.SetActive(false);
        settingsUI.SetActive(false);
        audioSettingsUI.SetActive(false);
        controlsUI.SetActive(true);

        ResetAllButtonStates();
    }

    //function to prevent buttons from staying highlighted when you switch between menus
    private void ResetAllButtonStates() {
        // Find all buttons with the ButtonHoverEffects component and reset them
        foreach (var buttonEffect in FindObjectsOfType<ButtonHoverEffects>()) {
            buttonEffect.ResetState();
        }

        EventSystem.current.SetSelectedGameObject(null);
    }



}
