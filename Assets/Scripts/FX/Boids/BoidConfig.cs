using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoidConfig
{
    public bool isDebugOn = false;
    public float maxSpeed = 3f;
    public float maxForce = 0.1f;
    public float targetAmount = 1f;
    public float separationAmount = 1.5f;
    public float cohesionAmount = 1f;
    public float alignmentAmount = 0.75f;
    public Vector3 baseRotation;
}