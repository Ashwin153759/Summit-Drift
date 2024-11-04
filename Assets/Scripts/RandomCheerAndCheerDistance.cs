using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCheerAndCheerDistance : MonoBehaviour
{

    private GameObject car;

    private float cheerDistance = 50f;
    private Animator animator;

    void Start() {
        animator = GetComponent<Animator>(); // Get the Animator component

        if (car == null) {
            car = GameObject.FindWithTag("Player");
        }

        animator.SetBool("IsIdle", true);
    }

    void Update() {

        //current distance between car and spectator
        float currentDistance = Vector3.Distance(transform.position, car.transform.position);

        //if the spectator is within cheerDistance of the car, then a random cheer is assigned to them
        if (currentDistance <= cheerDistance) {
            if (animator != null) {
                animator.SetBool("IsIdle", false);
                AssignRandomCheeringAnimation();
            }
        } 
        else {
            animator.SetBool("IsIdle", true);
        }


    }

    private void AssignRandomCheeringAnimation() {
        if (animator == null) {
            Debug.LogWarning($"Animator not found on {gameObject.name}");
            return;
        }

        // Generate a random integer based on the number of cheering animations (0, 1, 2, 3)
        int randomCheer = Random.Range(0, 4);

        // Set the integer parameter in the Animator to trigger the appropriate animation
        animator.SetInteger("CheerIndex", randomCheer);

        // Debug.Log($"{gameObject.name} is playing animation with CheerIndex: {randomCheer}");
    }
}
