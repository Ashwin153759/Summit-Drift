using Cinemachine;
using System.Collections;
using UnityEngine;

public class CarCameraEffects : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin perlinNoise;

    [Header("Boost Settings")]
    [SerializeField] private float defaultFOV = 70f;
    [SerializeField] private float boostFOV = 75;
    [SerializeField] private float fovTransitionSpeed = 10f;

    private CarController carController;
    private Coroutine shakeResetCoroutine;

    private void Awake()
    {
        carController = GetComponent<CarController>();
        perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        ResetIntensity();
    }

    private void Update()
    {
        HandleFOV();
    }

    private void HandleFOV()
    {
        // Adjust the FOV based on boost status
        float targetFOV = carController.IsBoosting ? boostFOV : defaultFOV;
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }

    public void TriggerCameraShake(float intensity, float shakeTime)
    {
        Debug.Log(intensity);
        perlinNoise.m_AmplitudeGain = intensity;

        // Start the shake reset process
        if (shakeResetCoroutine != null)
        {
            StopCoroutine(shakeResetCoroutine);
        }
        shakeResetCoroutine = StartCoroutine(ResetShakeOverTime(shakeTime));
    }

    IEnumerator ResetShakeOverTime(float shakeTime)
    {
        // Wait for the shake time before starting to reset
        yield return new WaitForSeconds(shakeTime);

        // Gradually reduce the shake intensity back to 0
        float initialIntensity = perlinNoise.m_AmplitudeGain;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            perlinNoise.m_AmplitudeGain = Mathf.Lerp(initialIntensity, 0, elapsedTime);
            yield return null;
        }

        ResetIntensity();
    }

    void ResetIntensity()
    {
        perlinNoise.m_AmplitudeGain = 0;
    }
}
