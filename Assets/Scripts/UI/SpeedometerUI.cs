using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedometerUI : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 200;

    [SerializeField] private float minSpeedArrowAngle;
    [SerializeField] private float maxSpeedArrowAngle;

    private CarController carController;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private RectTransform arrow;

    private void Start()
    {
        carController = FindAnyObjectByType<CarController>();
    }

    void Update()
    {
        float speed = Mathf.Abs(carController.CurrentCarLocalVelocity.z * 2.5f);

        if (speedText != null)
            speedText.text = ((int)speed) + " km/h";
        if (arrow != null)
            arrow.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, speed / maxSpeed));
    }
}
