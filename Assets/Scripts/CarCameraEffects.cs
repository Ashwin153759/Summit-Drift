using Cinemachine;
using System.Collections;
using UnityEngine;

public class CarCameraEffects : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin perlinNoise;

    [Header("General")]
    [SerializeField] private float defaultFOV = 70f;

    [Header("Boost FOV Settings")]
    [SerializeField] private float boostFOV = 75;
    [SerializeField] private float boostFovTransitionSpeed = 10f;
    [SerializeField] private float boostReturnFovTransitionSpeed = 2;

    [Header("Collision FOV Settings")]
    [SerializeField] private float maxCollisionFOV = 60f;
    [SerializeField] private float collisionFovTransitionSpeed = 10f;
    [SerializeField] private float collisionReturnFovTransitionSpeed = 2;
    [SerializeField] private float maxCollisionIntensity = 40f;

    private CarController carController;
    private Coroutine shakeResetCoroutine;
    private Coroutine boostFOVCoroutine;
    private Coroutine collisionFOVCoroutine;


    private void Awake()
    {
        carController = GetComponent<CarController>();
        perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        ResetIntensity();

        virtualCamera.m_Lens.FieldOfView = defaultFOV;
    }

    public void StartBoostFOV()
    {
        // Stop any ongoing FOV transitions to prevent conflict
        if (boostFOVCoroutine != null)
        {
            StopCoroutine(boostFOVCoroutine);
        }
        boostFOVCoroutine = StartCoroutine(AdjustFOV(boostFOV, boostFovTransitionSpeed));
    }

    public void StopBoostFOV()
    {
        // Stop any ongoing FOV transitions
        if (boostFOVCoroutine != null)
        {
            StopCoroutine(boostFOVCoroutine);
        }
        boostFOVCoroutine = StartCoroutine(AdjustFOV(defaultFOV, boostReturnFovTransitionSpeed));
    }

    private IEnumerator AdjustFOV(float targetFOV, float transitionSpeed)
    {
        // Smoothly interpolate to the target FOV with specified speed
        while (Mathf.Abs(virtualCamera.m_Lens.FieldOfView - targetFOV) > 0.1f)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
            yield return null;
        }

        // Set exact target FOV at the end
        virtualCamera.m_Lens.FieldOfView = targetFOV;
    }

    public void TriggerCollisionFOV(float collisionIntensity)
    {
        float normalizedIntensity = Mathf.Clamp01(collisionIntensity / maxCollisionIntensity);
        float targetFOV = Mathf.Lerp(defaultFOV, maxCollisionFOV, normalizedIntensity);

        if (collisionFOVCoroutine != null)
        {
            StopCoroutine(collisionFOVCoroutine);
        }
        collisionFOVCoroutine = StartCoroutine(CollisionFOVRoutine(targetFOV));
    }

    private IEnumerator CollisionFOVRoutine(float targetFOV)
    {
        // Transition to target FOV
        while (Mathf.Abs(virtualCamera.m_Lens.FieldOfView - targetFOV) > 0.1f)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.MoveTowards(virtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * collisionFovTransitionSpeed);
            yield return null;
        }

        // Ensure exact target FOV at the end
        virtualCamera.m_Lens.FieldOfView = targetFOV;

        // Transition back to default FOV
        while (Mathf.Abs(virtualCamera.m_Lens.FieldOfView - defaultFOV) > 0.1f)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.MoveTowards(virtualCamera.m_Lens.FieldOfView, defaultFOV, Time.deltaTime * collisionReturnFovTransitionSpeed);
            yield return null;
        }

        // Ensure final FOV reset
        virtualCamera.m_Lens.FieldOfView = defaultFOV;
    }


    public void TriggerCameraShake(float intensity, float shakeTime)
    {
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
