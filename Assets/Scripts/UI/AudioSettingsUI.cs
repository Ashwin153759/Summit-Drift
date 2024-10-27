using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour {


    [SerializeField] private Button returnToSettingsButton;

    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject audioSettingsUI;

    [SerializeField] private Slider musicVolumeSlider;


    private void Awake() {

    }

}
