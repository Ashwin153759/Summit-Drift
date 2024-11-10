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
    private float fadeDuration = 0.3f;

    private void Start()
    {
        countdownText.text = "";
        countdownText.alpha = 0;
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownSequence());
    }

    private IEnumerator CountdownSequence()
    {
        yield return new WaitForSeconds(1);

        countdownText.text = countdownTime.ToString();
        yield return FadeTextAlpha(0, 1);

        // Start countdown numbers
        int timeRemaining = countdownTime;
        while (timeRemaining > 0)
        {
            // Play beep sound
            AudioManager.instance.Play("Countdown");

            // Update countdown text
            countdownText.text = timeRemaining.ToString();
            Debug.Log(timeRemaining);

            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        // Play final countdown sound and display "GO!"
        AudioManager.instance.Play("Countdown_finale");
        countdownText.text = "GO!";

        // Trigger the event to start the race
        OnCountdownComplete?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // Fade out the text after "GO!"
        yield return FadeTextAlpha(1, 0); // Fade out to transparent
        countdownText.text = "";
    }

    private IEnumerator FadeTextAlpha(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            countdownText.alpha = alpha;
            Debug.Log(alpha);
            yield return null;
        }
        countdownText.alpha = endAlpha;
    }
}
