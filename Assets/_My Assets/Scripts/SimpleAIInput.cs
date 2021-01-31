using UnityEngine;
using KartGame.KartSystems;
using FluffyUnderware.Curvy;

public class SimpleAIInput : MonoBehaviour, IInput
{
    [SerializeField]
    private AIDifficulty m_difficulty;
    [SerializeField]
    private AISetup m_AISetup;
    [SerializeField]
    private CurvySpline m_AISpline;

    [SerializeField, Range(0f, 1f)]
    private float m_maxAcceleration = 1;
    [SerializeField, Range(0f, 1f)]
    private float m_maxSteering = 1;
    [SerializeField]
    private float m_forwardDistance = 10;
    [SerializeField]
    private float m_refAngle = 20;
    [SerializeField]
    private float m_angleWeight = 1;
    [SerializeField]
    private float m_laneWeight = 1;
    [SerializeField]
    private float m_minLaneOffset = 1;
    [SerializeField]
    private float m_minSpeedToTurn = 10;
    [SerializeField]
    private float m_maxSpeedToTurn = 20;

    // Internal
    private float m_speedToTurn;

    private float m_laneOffset;
    private float m_angleOffset;

    private float m_Acceleration;
    private float m_Steering;
    private bool m_BoostPressed;
    private bool m_FirePressed;
    private bool m_HopPressed;
    private bool m_HopHeld;

    private Vector3 m_closestPosition;
    private Quaternion m_currentOrientation;
    private Vector3 m_currentDirection;
    private Vector3 m_targetPosition;
    private Quaternion m_targetOrientation;
    private Vector3 m_targetDirection;

    private KartMovement m_kartMovement;

    // Accessors -------------------------------------------------------------------------------------

    public float Acceleration => m_Acceleration;

    public float Steering => m_Steering;

    public bool BoostPressed => m_BoostPressed;

    public bool FirePressed => m_FirePressed;

    public bool HopPressed => m_HopPressed;

    public bool HopHeld => m_HopHeld;

    // Messages -------------------------------------------------------------------------------------

    private void Awake()
    {
        SetAIDifficulty((int)m_difficulty);
        m_kartMovement = GetComponent<KartMovement>();
        m_AISpline = GameObject.Find("AI Spline").GetComponent<CurvySpline>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_AISpline == null)
        {
            return;
        }

        // Get spline current and ahead target info to set AI
        GetSplineData();

        // Get offsets to guide kart
        CalculateOffsets();

        // Set acceleration
        var accelByAngle = (Mathf.Abs(m_angleOffset) > m_refAngle ? -1 : (Mathf.Lerp(1, 0, Mathf.Abs(m_angleOffset) / m_refAngle)));
        m_Acceleration = m_maxAcceleration * accelByAngle;
        if(m_kartMovement.LocalSpeed < m_speedToTurn)
        {
            m_Acceleration = m_maxAcceleration;
        }

        // Set steering
        var steeringByAngle = m_angleWeight * (Mathf.Sign(m_angleOffset) * Mathf.Clamp(Mathf.Abs(m_angleOffset), 0, m_refAngle) / m_refAngle);
        var steeringByLane = steeringByAngle;

        if (Mathf.Abs(m_laneOffset) > m_minLaneOffset)
        {
            steeringByLane = m_laneWeight * (-Mathf.Clamp(m_laneOffset, -1, 1));
        }
        m_Steering = m_maxSteering * ((steeringByAngle + steeringByLane) / 2);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(m_closestPosition, 1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(m_targetPosition, 1f);
        Gizmos.DrawLine(m_closestPosition, transform.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(m_targetPosition, m_targetDirection * 5);
    }

    // Methods -------------------------------------------------------------------------------------

    private void GetSplineData()
    {
        // Get nearest point in spline
        float nearestTF = m_AISpline.GetNearestPointTF(transform.position, out m_closestPosition);
        // Get orientation at that total fragment (TF)
        m_currentOrientation = m_AISpline.GetOrientationFast(nearestTF);
        // Project direction on Y plane
        m_currentDirection = Vector3.ProjectOnPlane(m_currentOrientation * Vector3.forward, Vector3.up);

        // Get distance from TF
        float currDistance = m_AISpline.TFToDistance(nearestTF);
        float targetDistance = currDistance + m_forwardDistance;
        // Check connection point. Substract if get over spline length
        if (targetDistance >= m_AISpline.Length)
        {
            targetDistance -= m_AISpline.Length;
        }

        // Get point from target distance
        m_targetPosition = m_AISpline.InterpolateByDistanceFast(targetDistance);
        // Get TF from distance
        float targetTF = m_AISpline.DistanceToTF(targetDistance);
        // Get orientation at distance
        m_targetOrientation = m_AISpline.GetOrientationFast(targetTF);
        // Project direction on Y plane
        m_targetDirection = Vector3.ProjectOnPlane(m_targetOrientation * Vector3.forward, Vector3.up);
    }

    private void CalculateOffsets()
    {
        // Project offset vector on road plane to get lane offset
        Vector3 currentOffset = Vector3.ProjectOnPlane((transform.position - m_closestPosition), m_currentOrientation * Vector3.up);
        // Get lane position
        m_laneOffset = Vector3.Dot(currentOffset, m_currentOrientation * Vector3.right);
        // Get angle between current and target direction
        //m_angleOffset = Vector3.SignedAngle(m_currentDirection, m_targetDirection, Vector3.up);
        m_angleOffset = Vector3.SignedAngle(transform.forward, m_targetDirection, Vector3.up);
    }

    public void SetAIDifficulty(int difficulty)
    {
        m_difficulty = (AIDifficulty)difficulty;
        m_speedToTurn = Mathf.Lerp(m_minSpeedToTurn, m_maxSpeedToTurn, m_AISetup.GetAI(m_difficulty).value);
    }
}
