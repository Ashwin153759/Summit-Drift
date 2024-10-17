using UnityEngine;

public class CarCollisionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject particleEffectPrefab;
    [SerializeField] private LayerMask collisionLayer;

    [Header("Particle Size Settings")]
    [SerializeField] private float minStartSize = 0.005f;
    [SerializeField] private float maxStartSize = 0.1f;

    [Header("Particle Burst Settings")]
    [SerializeField] private int minBurstCount = 5;
    [SerializeField] private int maxBurstCount = 100;

    private CarController carController;


    private void Start()
    {
        carController = GetComponent<CarController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (particleEffectPrefab != null)
        {
            // Check if the layer of the collided object is part of the collisionLayer mask
            if ((collisionLayer.value & (1 << collision.gameObject.layer)) != 0)
            {
                // Get the first contact point of the collision
                Vector3 collisionPoint = collision.contacts[0].point;

                // Spawn the particle effect at the collision point
                GameObject particleEffect = Instantiate(particleEffectPrefab, collisionPoint, Quaternion.identity);

                // Adjust the particle effect based on car velocity
                AdjustParticleEffect(particleEffect);
            }
        }
    }

    /// <summary>
    /// Adjusts the particle effect settings depending on collision speed
    /// </summary>
    /// <param name="particleEffect"></param>
    private void AdjustParticleEffect(GameObject particleEffect)
    {
        ParticleSystem particleSystem = particleEffect.GetComponent<ParticleSystem>();

        if (particleSystem != null)
        {
            // Calculate size range and burst count based on the car's velocity ratio
            float particleSizeMin = Mathf.Lerp(minStartSize, maxStartSize, carController.CarVelocityRatio);
            float particleSizeMax = particleSizeMin * 3.0f;

            int burstCount = Mathf.RoundToInt(Mathf.Lerp(minBurstCount, maxBurstCount, carController.CarVelocityRatio));

            // Adjust the Main module for start size range
            var mainModule = particleSystem.main;
            mainModule.startSize = new ParticleSystem.MinMaxCurve(particleSizeMin, particleSizeMax);

            // Adjust the Emission module for burst count
            var emissionModule = particleSystem.emission;
            emissionModule.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0, burstCount)
            });
        }
    }
}