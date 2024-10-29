using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlideAnimations : MonoBehaviour
{
    [SerializeField] private Animator mainMenuAnimator;
    [SerializeField] private Animator settingsAnimator;


    //method to slide out main menu when settings button is clicked
    public void ShowMainMenuUI() {
        mainMenuAnimator.SetTrigger("SlideIn");
    }

    public void HideMainMenuUI() {
        mainMenuAnimator.SetTrigger("SlideOut");
    }

    public void ShowSettingUI() {
        settingsAnimator.SetTrigger("SlideIn");

    }

    public void HideSettingUI() {
        settingsAnimator.SetTrigger("SlideOut");
    }


}
