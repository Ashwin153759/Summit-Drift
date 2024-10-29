using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private GameObject[] tires = new GameObject[4];
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2];
    [SerializeField] private TrailRenderer[] skidMarks = new TrailRenderer[2];
    [SerializeField] private ParticleSystem[] skidSmokes = new ParticleSystem[2];
    [SerializeField] private List<GameObject> boostParticleObjects;
    [SerializeField] private AudioSource engineSound, skidSound, boostSound, boostEndSound;
    [SerializeField] private Transform centerOfMass;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;

    [Header("Input")]
    [SerializeField] private float steerSensitivity = 2f;
    [SerializeField] private float steerDamping = 3f;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1f;
    [SerializeField] private float driftDeceleration = 100f;
    [SerializeField] private float DriftDragCoefficient = 0.5f;
    [SerializeField] private float sidewaysDragTransitionSpeed = 2f;
    [SerializeField] private float boostMultiplier = 1f;

    [Header("Visuals")]
    [SerializeField] private float tireRotSpeed = 3000f;
    [SerializeField] private float maxSteeringAngle = 30f;
    [SerializeField] private float minSideSkidVelocity = 10f;
    [SerializeField] private float skidSmokeMinSize = 0.1f;
    [SerializeField] private float skidSmokeMaxSize = 1f;

    [Header("Audio")]
    [SerializeField]
    [Range(0, 1)] private float minPitch = 1f;
    [SerializeField]
    [Range(1, 5)] private float maxPitch = 1f;

    private Rigidbody carRB;
    private CarCameraEffects carCameraEffects;

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    // Drifting
    private bool isDrifting; // if we are trying to drift
    private bool isDriftingBtn; // If the drift button is pressed
    private bool isCarSlipping; // if the car is slipping

    private float currentSidewaysDrag;
    private float driftDuration = 0f;
    private float driftIntensity = 0f;

    private bool isBoosting = false;
    private float boostEndTime = 0f;
    private float boostStrength = 0f;
    private bool isBoostSoundPlaying = false;

    // Input
    private float accumulatedSteerInput = 0f;

    private CarInputActions inputActions;
    private float moveInput;
    private float steerInput;

    // Ground Detected
    private int[] wheelsIsGrounded = new int[4];
    private bool isGrounded = false;
    private bool carOfftrack;
    private int offtrackLayer;

    #region Unity Functions

    private void Awake()
    {
        carRB = GetComponent<Rigidbody>();

        // Initialize input actions
        inputActions = new CarInputActions();

        // Assign input action events for Move
        inputActions.Car.Move.performed += ctx => moveInput = ctx.ReadValue<float>();
        inputActions.Car.Move.canceled += ctx => moveInput = 0f;

        // Drift button handling (true when pressed, false when released)
        inputActions.Car.Drift.performed += ctx => StartDrifting();
        inputActions.Car.Drift.canceled += ctx => EndDrifting();

        // Get the offtrack layer index
        offtrackLayer = LayerMask.NameToLayer("Mountain");
    }

    private void Start()
    {
        carCameraEffects = GetComponent<CarCameraEffects>();

        Vector3 localCenterOfMass = transform.InverseTransformPoint(centerOfMass.position);
        carRB.centerOfMass = localCenterOfMass;

        currentSidewaysDrag = dragCoefficient;
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
        Visuals();
        EngineSound();
    }

    private void Update()
    {
        GetInputs();
    }

    private void OnDestroy()
    {
        inputActions.Disable();
    }

    #endregion

    #region Get Functions

    public Vector3 CurrentCarLocalVelocity
    {
        get { return currentCarLocalVelocity; }
    }

    public float CarVelocityRatio
    {
        get { return carVelocityRatio; }
    }

    #endregion

    #region User Inputs

    private void GetInputs()
    {
        Steering();
    }

    private void Steering()
    {
        float rawSteerInput = inputActions.Car.Steer.ReadValue<float>();

        // Accumulate steer input with damping for smoother transitions
        accumulatedSteerInput += (rawSteerInput * steerSensitivity - accumulatedSteerInput) * Time.deltaTime * steerDamping;

        // Clamp the accumulated steer input to [-1, 1] range (to prevent overflow)
        accumulatedSteerInput = Mathf.Clamp(accumulatedSteerInput, -1f, 1f);

        // Assign the accumulated value to steerInput, which will be used in the steering code
        steerInput = accumulatedSteerInput;
    }

    public void SetControlsActive(bool isActive)
    {
        if (isActive)
        {
            inputActions.Enable();
        }
        else
        {
            inputActions.Disable();
        }
    }

    #endregion

    #region Movement

    private void Movement()
    {
        if (isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
            Drifting();
        }
    }

    private void Acceleration()
    {
        float effectiveAcceleration = acceleration;
        float effectiveMaxSpeed = maxSpeed;

        // If moving backward, limit speed and acceleration
        if (moveInput < 0 && carVelocityRatio < 0)
        {
            effectiveAcceleration *= 0.25f; 
            effectiveMaxSpeed *= 0.25f;
        }

        if (Mathf.Abs(currentCarLocalVelocity.z) < effectiveMaxSpeed)
        {
            carRB.AddForceAtPosition(effectiveAcceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
        }
    }

    private void Deceleration()
    {
        // Slows down car whether it is going forward or backwards
        float forceDirection = currentCarLocalVelocity.z > 0 ? 1f : -1f;

        float decelerationForce = (isDrifting ? driftDeceleration : deceleration) * Mathf.Abs(carVelocityRatio);

        // If the car is offtrack make them slower
        decelerationForce = (carOfftrack ? decelerationForce * 5 : decelerationForce);

        carRB.AddForceAtPosition(-decelerationForce * transform.forward * forceDirection, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        carRB.AddRelativeTorque((isDrifting ? steerStrength * 1.5f : steerStrength) * steerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    private void SidewaysDrag()
    {
        if (isDrifting)
        {
            currentSidewaysDrag = DriftDragCoefficient;
        }
        else
        {
            // Smoothly and linearly transition back to normal deceleration using Mathf.MoveTowards
            currentSidewaysDrag = Mathf.MoveTowards(currentSidewaysDrag, dragCoefficient, sidewaysDragTransitionSpeed * Time.fixedDeltaTime);
        }

        float currentSidewaysSpeed = currentCarLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * currentSidewaysDrag;

        Vector3 dragForce = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void Drifting()
    {
        // Calculate if the car is slipping
        isCarSlipping = isGrounded && Mathf.Abs(currentCarLocalVelocity.x) > minSideSkidVelocity && carVelocityRatio > 0;

        // Initiate Drift if we are moving forward and pressing button
        isDrifting = isDriftingBtn && moveInput > 0;

        if (isDrifting)
        {
            // Only track drift stats when actually slipping
            if (isCarSlipping)
            {
                driftDuration += Time.deltaTime;
                driftIntensity = Mathf.Max(driftIntensity, Mathf.Abs(currentCarLocalVelocity.x));  // Track max sideways velocity
            }
            else
            {
                // Reset drift if we are no longer slipping
                ResetDriftStats();
            }
        }
    }

    private void ApplyDriftBoost()
    {
        // Calculate boost strength based on drift duration and intensity
        boostStrength = driftDuration * driftIntensity * 0.5f * boostMultiplier;

        // Apply boost for a duration based on drift
        float boostDuration = Mathf.Clamp(driftDuration, 0.5f, 3f);
        boostEndTime = Time.time + boostDuration;

        if (!isBoosting)
        {
            StartCoroutine(ApplyBoostForDuration(boostDuration));
        }
    }

    private IEnumerator ApplyBoostForDuration(float boostDuration)
    {
        isBoosting = true;

        while (Time.time < boostEndTime)
        {
            // Apply the continuous boost force
            Vector3 boostVelocity = transform.forward * boostStrength * Time.deltaTime;
            
            carRB.AddForce(boostVelocity, ForceMode.Acceleration);

            yield return null;
        }

        isBoosting = false;
    }

    private void ResetDriftStats()
    {
        driftDuration = 0f;
        driftIntensity = 0f;
    }

    private void StartDrifting()
    {
        isDriftingBtn = true;
    }

    private void EndDrifting()
    {
        isDriftingBtn = false;

        if (isCarSlipping && driftDuration >= 0.3f)
        {
            ApplyDriftBoost();
        }

        ResetDriftStats();
    }

    #endregion

    #region Visuals

    private void Visuals()
    {
        TireVisuals();
        Vfx();
        BoostVisual();
    }

    private void TireVisuals()
    {
        float steeringAngle = maxSteeringAngle * steerInput;

        for (int i = 0; i < tires.Length; i++)
        {
            if (i < 2)
            {
                tires[i].transform.Rotate(Vector3.right, tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);

                frontTireParents[i].transform.localEulerAngles = new Vector3(frontTireParents[i].transform.localEulerAngles.x, steeringAngle, frontTireParents[i].transform.localEulerAngles.z);
            }
            else
            {
                // If we press gas, rotate more, else rotate with tirespeed
                if (Mathf.Abs(moveInput) > Mathf.Abs(carVelocityRatio))
                    tires[i].transform.Rotate(Vector3.right, tireRotSpeed * moveInput * Time.deltaTime, Space.Self);
                else
                    tires[i].transform.Rotate(Vector3.right, tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);
            }

        }
    }

    private void BoostVisual()
    {

        // Handle Boost Sound Effects
        if (isBoosting && !isBoostSoundPlaying)
        {
            StartSound(boostSound);
            isBoostSoundPlaying = true;

            carCameraEffects.StartBoostFOV();
        }
        else if (!isBoosting && isBoostSoundPlaying)
        {
            StopSound(boostSound);
            StartSound(boostEndSound);
            isBoostSoundPlaying = false;

            carCameraEffects.StopBoostFOV();
        }

        // Turn on / off Boost in Exhaust
        foreach (GameObject particleObject in boostParticleObjects)
        {
            ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                if (isBoosting)
                {
                    if (!particleSystem.isPlaying)
                    {
                        particleSystem.Play();
                    }
                }
                else
                {
                    if (particleSystem.isPlaying)
                    {
                        particleSystem.Stop();
                    }
                }
            }
        }
    }

    private void Vfx()
    {
        if (isCarSlipping && isGrounded)
        {
            ToggleSkidMarks(true);
            ToggleSkidSmokes(true);
            ToggleSound(true, skidSound);
        }
        else
        {
            ToggleSkidMarks(false);
            ToggleSkidSmokes(false);
            ToggleSound(false, skidSound);
        }
    }

    private void ToggleSkidMarks(bool toggle)
    {
        foreach (var skidMark in skidMarks)
        {
            skidMark.emitting = toggle;
        }
    }

    private void ToggleSkidSmokes(bool toggle)
    {
        foreach (var smoke in skidSmokes)
        {
            if (toggle)
            {
                smoke.Play();
                // Adjust the size of the skid smoke depending on how fast we are drifting sideways
                AdjustSkidSmokeParticleEffect(smoke);
            }
            else
            {
                smoke.Stop();
            }
        }
    }

    private void AdjustSkidSmokeParticleEffect(ParticleSystem particleEffect)
    {
        var mainModule = particleEffect.main;

        // Normalize the velocity value (clamp it between 0 and 1)
        float normalizedVelocity = Mathf.Clamp01(Mathf.Abs(CurrentCarLocalVelocity.x) / 25f);

        // Calculate the new particle size based on the normalized skid velocity
        float newParticleSize = Mathf.Lerp(skidSmokeMinSize, skidSmokeMaxSize, normalizedVelocity);

        mainModule.startSize = new ParticleSystem.MinMaxCurve(newParticleSize);
    }

    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }

    #endregion

    #region Audio

    private void EngineSound()
    {
        engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(carVelocityRatio));
    }

    private void ToggleSound(bool toggle, AudioSource audio)
    {
        audio.mute = !toggle;
    }

    private void StartSound(AudioSource audio)
    {
        audio.Play();
    }

    private void StopSound(AudioSource audio)
    {
        audio.Stop();
    }

    #endregion

    #region Car Status Check

    private void GroundCheck()
    {
        int tempGroundedWheels = 0;
        bool wheelsOnMountain = true;

        for (int i = 0; i < wheelsIsGrounded.Length; i++)
        {
            RaycastHit hit;
            float maxDistance = restLength;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxDistance + wheelRadius, drivable))
            {
                wheelsIsGrounded[i] = 1;
                tempGroundedWheels++;

                // Check if the wheel is on the mountain layer
                if (hit.collider.gameObject.layer != offtrackLayer)
                {
                    wheelsOnMountain = false;
                }
            }
            else
            {
                wheelsIsGrounded[i] = 0;
                wheelsOnMountain = false;
            }
        }

        isGrounded = tempGroundedWheels > 1;
        carOfftrack = wheelsOnMountain;
    }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.velocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    #endregion

    #region Suspension Functions
    private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxDistance = restLength;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxDistance + wheelRadius, drivable))
            {
                wheelsIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                // Adds an upward force to the car suspension point
                carRB.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                // Visuals

                SetTirePosition(tires[i], hit.point + rayPoints[i].up * wheelRadius);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                wheelsIsGrounded[i] = 0;

                // Visuals

                SetTirePosition(tires[i], rayPoints[i].position - rayPoints[i].up * maxDistance);

                Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxDistance) * -rayPoints[i].up, Color.green);
            }

        }
    }

    #endregion
}

