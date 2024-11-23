using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassEnabler : MonoBehaviour
{
    GrassComputeScript grass;

    void Awake()
    {
        grass = GetComponent<GrassComputeScript>();
        grass.enabled = true;
    }

}
