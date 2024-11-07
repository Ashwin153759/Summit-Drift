using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownManager : MonoBehaviour
{
    // Event to notify when countdown finishes
    public event Action OnCountdownComplete; 

    [SerializeField] private TextMeshProUGUI countdownText; 
    [SerializeField] private int countdownTime = 3;

    private void Start()
    {
        countdownText.text = "";
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        int timeRemaining = countdownTime;
        yield return new WaitForSeconds(1f);

        while (timeRemaining > 0)
        {
            // Play beep sound
            AudioManager.instance.Play("Countdown");

            countdownText.text = timeRemaining.ToString();
            Debug.Log(timeRemaining);
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        AudioManager.instance.Play("Countdown_finale");
        countdownText.text = "GO!";

        // Trigger event to start the race
        OnCountdownComplete?.Invoke(); 

        yield return new WaitForSeconds(0.5f);
        countdownText.text = "";
    }
}