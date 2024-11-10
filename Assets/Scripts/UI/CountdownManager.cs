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

    // Arrays for light objects and materials (0 = Red, 1 = Yellow, 2 = Green)
    [SerializeField] private GameObject[] trafficLights = new GameObject[3];
    [SerializeField] private Material[] offMaterials = new Material[3];
    [SerializeField] private Material[] onMaterials = new Material[3];

    private void Start()
    {
        countdownText.text = "";
        countdownText.alpha = 0;

        // Set all lights to ON initially
        SetAllTrafficLights();
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

            // Change light color based on countdown number
            UpdateTrafficLight(timeRemaining);

            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        // Play final countdown sound and display "GO!"
        AudioManager.instance.Play("Countdown_finale");
        countdownText.text = "GO!";

        // Green Light
        ResetTrafficLights();
        SetLightMaterial(trafficLights[2], onMaterials[2]);

        // Start Race Flag
        OnCountdownComplete?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // Fade out the text
        yield return FadeTextAlpha(1, 0);
        countdownText.text = "";
    }

    private void UpdateTrafficLight(int countdownNumber)
    {
        // Turn on the appropriate light and turn off others
        switch (countdownNumber)
        {
            case 3:
                ResetTrafficLights();
                break;

            case 2:
                ResetTrafficLights();
                SetLightMaterial(trafficLights[0], onMaterials[0]); // Red light on
                break;

            case 1:
                ResetTrafficLights();
                SetLightMaterial(trafficLights[1], onMaterials[1]); // Yellow light on
                break;
        }
    }

    private void ResetTrafficLights()
    {
        SetLightMaterial(trafficLights[0], offMaterials[0]);
        SetLightMaterial(trafficLights[1], offMaterials[1]);
        SetLightMaterial(trafficLights[2], offMaterials[2]);
    }

    private void SetAllTrafficLights()
    {
        SetLightMaterial(trafficLights[0], onMaterials[0]);
        SetLightMaterial(trafficLights[1], onMaterials[1]);
        SetLightMaterial(trafficLights[2], onMaterials[2]);
    }

    private void SetLightMaterial(GameObject lightObject, Material material)
    {
        Renderer renderer = lightObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }

    private IEnumerator FadeTextAlpha(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            countdownText.alpha = alpha;
            yield return null;
        }
        countdownText.alpha = endAlpha;
    }
}
