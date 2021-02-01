using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is a direct stats modifier over the kart for the required four stats. If the goal is to create stats 
/// for different karts or opponents is better use the AddativeKartModifierAsset. It's important recall that 
/// this assets have many more stats than the configurable here.
/// </summary>
public class KartSpecsModifier : MonoBehaviour, IKartModifier
{
    [SerializeField]
    private KartMovement m_kartMovement;

    [Header("Kart Stats")]
    [SerializeField, Range(0f, MaxValue)]
    private float m_maxSpeed;
    [SerializeField, Range(0f, MaxValue)]
    private float m_acceleration;
    [SerializeField, Range(0f, MaxValue)]
    private float m_braking;
    [SerializeField, Range(0f, MaxValue)]
    private float m_steering;
    [SerializeField]
    private bool m_applySpecs = true;

    public const int MaxValue = 10;

    // Start is called before the first frame update
    private void Start()
    {
        // Set stats directly on kart
        if(m_applySpecs)
        {
            m_kartMovement.AddKartModifier(this);
        }
    }

    /// <summary>
    /// Set external stat value.
    /// </summary>
    public void SetMaxSpeed(float value)
    {
        m_maxSpeed = value * MaxValue;
    }

    /// <summary>
    /// Set external stat value.
    /// </summary>
    public void SetAccelaration(float value)
    {
        m_acceleration = value * MaxValue;
    }

    /// <summary>
    /// Set external stat value.
    /// </summary>
    public void SetBraking(float value)
    {
        m_braking = value * MaxValue;
    }

    /// <summary>
    /// Set external stat value.
    /// </summary>
    public void SetSteering(float value)
    {
        m_steering = value * MaxValue;
    }

    // Interface implementation -----------------------------------------------------------------------

    public float ModifyAcceleration(float acceleration)
    {
        return acceleration + m_acceleration;
    }

    public float ModifyBraking(float braking)
    {
        return braking + m_braking;
    }

    public float ModifyCoastingDrag(float coastingDrag)
    {
        return coastingDrag;
    }

    public float ModifyGravity(float gravity)
    {
        return gravity;
    }

    public float ModifyGrip(float grip)
    {
        return grip + m_steering / 0.5f;
    }

    public float ModifyHopHeight(float hopHeight)
    {
        return hopHeight;
    }

    public float ModifyReverseAcceleration(float reverseAcceleration)
    {
        return reverseAcceleration + m_acceleration / 0.5f;
    }

    public float ModifyReverseSpeed(float reverseSpeed)
    {
        return reverseSpeed;
    }

    public float ModifyTopSpeed(float topSpeed)
    {
        return topSpeed + m_maxSpeed;
    }

    public float ModifyTurnSpeed(float turnSpeed)
    {
        return turnSpeed + m_steering;
    }

    public float ModifyWeight(float weight)
    {
        return weight;
    }
}
