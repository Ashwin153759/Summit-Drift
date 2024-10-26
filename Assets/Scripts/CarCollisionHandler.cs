using UnityEngine;

public class CarCollisionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hitParticleEffectPrefab;
    [SerializeField] private GameObject groundParticleEffectPrefab;
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private LayerMask drivableLayer;
    [SerializeField] private AudioSource carCrashSound;

    [Header("Hit Effect Size Settings")]
    [SerializeField] private float hitMinStartSize = 0.005f;
    [SerializeField] private float hitMaxStartSize = 0.1f;

    [Header("Ground Effect Size Settings")]
    [SerializeField] private float groundMinStartSize = 0.005f;
    [SerializeField] private float groundMaxStartSize = 0.1f;

    [Header("Hit Effect Burst Settings")]
    [SerializeField] private int hitMinBurstCount = 5;
    [SerializeField] private int hitMaxBurstCount = 100;

    [Header("Ground Effect Burst Settings")]
    [SerializeField] private int groundMinBurstCount = 5;
    [SerializeField] private int groundMaxBurstCount = 100;

    private CarController carController;
    private CarCameraEffects carCameraEffects;

    private void Start()
    {
        carController = GetComponent<CarController>();
        carCameraEffects = GetComponent<CarCameraEffects>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Trigger Camera Shake
        //carCameraEffects.TriggerCameraShake(magnitude / 25f, 0.07f);

        float magnitude = collision.relativeVelocity.magnitude;

        if (magnitude < 7)
            return;

        // Car Hit Sound Effect
        carCrashSound.Play();

        // Check for collisions with obstacles
        if ((collisionLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            GameObject particleEffect = Instantiate(hitParticleEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            AdjustHitParticleEffect(particleEffect);
            carCameraEffects.TriggerCollisionFOV(magnitude);
        }

        // Check for ground collisions at a specific falling speed
        if ((drivableLayer.value & (1 << collision.gameObject.layer)) != 0 && carController.CurrentCarLocalVelocity.y < 0)
        {
            GameObject particleEffect = Instantiate(groundParticleEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            AdjustGroundParticleEffect(particleEffect);
        }
    }

    private void AdjustHitParticleEffect(GameObject particleEffect)
    {
        ParticleSystem particleSystem = particleEffect.GetComponent<ParticleSystem>();

        if (particleSystem != null)
        {
            float particleSizeMin = Mathf.Lerp(hitMinStartSize, hitMaxStartSize, carController.CarVelocityRatio);
            float particleSizeMax = particleSizeMin * 3.0f;
            int burstCount = Mathf.RoundToInt(Mathf.Lerp(hitMinBurstCount, hitMaxBurstCount, carController.CarVelocityRatio));

            var mainModule = particleSystem.main;
            mainModule.startSize = new ParticleSystem.MinMaxCurve(particleSizeMin, particleSizeMax);

            var emissionModule = particleSystem.emission;
            emissionModule.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0, burstCount)
            });
        }
    }

    private void AdjustGroundParticleEffect(GameObject particleEffect)
    {
        ParticleSystem particleSystem = particleEffect.GetComponent<ParticleSystem>();

        if (particleSystem != null)
        {
            // Clamp the Y velocity between 0 and 20 for lerping
            float yVelocityNormalized = Mathf.Clamp(Mathf.Abs(carController.CurrentCarLocalVelocity.y), 0, 20) / 20f;

            float particleSizeMin = Mathf.Lerp(groundMinStartSize, groundMaxStartSize, yVelocityNormalized);
            float particleSizeMax = particleSizeMin * 3.0f;
            int burstCount = Mathf.RoundToInt(Mathf.Lerp(groundMinBurstCount, groundMaxBurstCount, yVelocityNormalized));

            var mainModule = particleSystem.main;
            mainModule.startSize = new ParticleSystem.MinMaxCurve(particleSizeMin, particleSizeMax);

            var emissionModule = particleSystem.emission;
            emissionModule.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0, burstCount)
            });
        }
    }
}
