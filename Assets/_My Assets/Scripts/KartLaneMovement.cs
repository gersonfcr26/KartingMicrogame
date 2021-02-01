using UnityEngine;
using System.Collections.Generic;
using KartGame.KartSystems;
using FluffyUnderware.Curvy;
using UnityEngine.Events;

[System.Serializable]
public class TrackModel
{
    public float trackWidth = 9.2f;
    public List<LaneModel> lanes;

    public LaneModel GetClosestLane(float offset)
    {
        var lane = lanes[0];
        for (int i = 0; i < lanes.Count; i++)
        {
            if((lanes[i].offset - offset) < (lane.offset - offset))
            {
                lane = lanes[i];
            }
        }

        return lane;
    }

    public bool GetNextLane(LaneModel previousLane, out LaneModel nextLane, bool moveRight)
    {
        nextLane = previousLane;
        if(moveRight && previousLane.index == lanes[lanes.Count - 1].index)
        {
            return false;
        }
        else if (!moveRight && previousLane.index == lanes[0].index)
        {
            return false;
        }

        for (int i = 0; i < lanes.Count; i++)
        {
            if(lanes[i].index == previousLane.index)
            {
                continue;
            }

            if (moveRight && (lanes[i].index > nextLane.index))
            {
                    nextLane = lanes[i];
            }
            else if (!moveRight && (lanes[i].index < nextLane.index))
            {
                nextLane = lanes[i];
            }

        }

        return previousLane == nextLane;
    }
}

[System.Serializable]
public class LaneModel
{
    public int index;
    public float offset;
}

public interface IDriftable
{
    void DriftInput(bool enable);

    void DriftOutput();
}

[RequireComponent(typeof(IInput))]
[RequireComponent(typeof(Rigidbody))]
public class KartLaneMovement : MonoBehaviour, IControllable, IDriftable
{
    [Header("Track")]
    [SerializeField]
    private CurvySpline m_trackSpline;
    [SerializeField]
    private TrackModel m_trackModel;
    [SerializeField]
    private float m_referenceDistance = 1;

    [Header("Lanes")]
    [SerializeField]
    private float m_timeToChangeLane = 0.5f;

    [Header("Kart settings")]
    [SerializeField]
    private float m_acceleration = 1;
    [SerializeField]
    private float m_topSpeed = 20;
    [SerializeField]
    private float m_kartHeight = 0.75f;
    [SerializeField]
    private float m_drag = 1;

    [Header("Drift")]
    [SerializeField]
    private float m_driftAngle = 30;
    [SerializeField]
    private float m_timeToTurn = 0.5f;
    [SerializeField]
    private AnimationCurve m_driftCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    public UnityEvent onDriftStarted;

    public UnityEvent onDriftFinished;

    // Internal (Spline data)
    private Vector3 m_closestPosition;
    private Quaternion m_currentOrientation;
    private Vector3 m_targetPointAhead;
    private Quaternion m_referenceOrientation;

    // Internal (Lanes control)  
    private bool m_changingLane;
    private LaneModel m_currentLane;
    private LaneModel m_targetLane;

    private float m_kartlaneOffset;
    private float m_startOffsetOnChanging;
    private float m_changingLaneTimer;

    // Internal (Movement)
    private float m_currentDistance;
    private float m_currentSpeed;
    private float m_curveSpeed;

    // Internal
    private bool m_isControlable;

    // Drift
    private bool m_detectingDrift;
    private bool m_driftEnabled;
    private bool m_driftTurning;
    private bool m_driftRight;
    private float m_driftTimer;

    // -------------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    private void Start()
    {
        // Assume kart is not aligned
        m_changingLane = true;
        // Get spline info
        CalculateKartSplineOffset();
        // Get closest lane first time
        m_targetLane = m_trackModel.GetClosestLane(m_kartlaneOffset);
        // First alignment
        if (m_currentLane == null)
        {
            m_startOffsetOnChanging = m_kartlaneOffset;
            // Find target lane
            m_targetLane = m_trackModel.GetClosestLane(m_kartlaneOffset);
        }

        // Set initial speed
        m_currentSpeed = 5;
    }

    // Update is called once per frame
    private void Update()
    {
        if(!m_isControlable)
        {
            return;
        }

        // Get spline info
        CalculateKartSplineOffset();

        float targetLaneOffset = m_targetLane.offset;
        var targetOrientation = m_currentOrientation;
        // Currently changing lane
        if (m_changingLane)
        {
            // Update changing lane timer
            m_changingLaneTimer += Time.deltaTime;
            float changeValue = Mathf.Clamp01(m_changingLaneTimer / m_timeToChangeLane);
            // Set new target offset based on interpolation
            if (m_currentLane == null)
            {
                targetLaneOffset = Mathf.Lerp(m_startOffsetOnChanging, m_targetLane.offset, changeValue);
            }
            else
            {
                targetLaneOffset = Mathf.Lerp(m_currentLane.offset, m_targetLane.offset, changeValue);
            }

            // Check change complete
            if(m_changingLaneTimer >= m_timeToChangeLane)
            {
                m_changingLane = false;
                m_changingLaneTimer = 0;
                m_currentLane = m_targetLane;
            }            
        }

        // Get input
        float inputSteer = 0;
        if (!m_changingLane)
        {
            inputSteer = Input.GetAxis("Horizontal");
        }

        // Update steering
        if (inputSteer != 0)
        {
            var curveAngle = Vector3.Angle(m_currentOrientation * Vector3.forward, m_referenceOrientation * Vector3.forward);
            if (m_detectingDrift && (inputSteer < 0 && curveAngle > 0 || inputSteer > 0 && curveAngle < 0))
            {
                m_driftEnabled = true;
                m_driftRight = inputSteer > 0;
                m_driftTurning = true;
                onDriftStarted?.Invoke();
            }
            else
            {
                m_changingLane = true;
                m_trackModel.GetNextLane(m_currentLane, out m_targetLane, inputSteer > 0);
            }
        }

        // Affect speed by drag
        m_currentSpeed = Mathf.Clamp(m_currentSpeed - m_drag * Time.deltaTime, 0, m_topSpeed);

        // Update acceleration
        m_currentSpeed += m_acceleration * Time.deltaTime;
        m_currentSpeed = Mathf.Clamp(m_currentSpeed, -m_topSpeed, m_topSpeed);

        // Update kart position
        if (m_currentSpeed > 0)
        {
            // Apply assisted speed in curves
            m_curveSpeed = Vector3.Angle(m_currentOrientation * Vector3.forward, m_referenceOrientation * Vector3.forward);

            float distanceAhead = (m_currentSpeed + m_curveSpeed * (m_currentSpeed / m_topSpeed)) * Time.deltaTime;
            float targetDistance = m_currentDistance + distanceAhead;

            // Check connection point
            bool reconnected = targetDistance > m_trackSpline.Length;
            if (reconnected)
            {
                targetDistance -= m_trackSpline.Length;
            }

            // Get point over spline ahead
            var pointOverSplineAhead = m_trackSpline.InterpolateByDistance(targetDistance);
            // Get point over lane ahead
            float tfAhead = m_trackSpline.DistanceToTF(targetDistance);
            var orientationAhead = m_trackSpline.GetOrientationFast(tfAhead);
            m_targetPointAhead = pointOverSplineAhead + Vector3.ProjectOnPlane((orientationAhead * Vector3.right) * targetLaneOffset, m_currentOrientation * Vector3.up) + Vector3.up * m_kartHeight;

            // Check drift
            if (m_driftTurning)
            {
                // Control rotation transition
                if (m_driftEnabled)
                {
                    m_driftTimer += Time.deltaTime;
                    if (m_driftTimer >= m_timeToTurn)
                    {
                        m_driftTimer = m_timeToTurn;
                        m_driftTurning = false;
                    }
                }
                else
                {
                    m_driftTimer -= Time.deltaTime;
                    if (m_driftTimer <= m_timeToTurn)
                    {
                        m_driftTimer = 0;
                        m_driftTurning = false;
                    }
                }
            }

            // Apply drift rotation
            if (m_driftEnabled || m_driftTurning)
            {
                float angle = Mathf.Lerp(0, (m_driftRight ? 1 : -1) * m_driftAngle, m_driftCurve.Evaluate(m_driftTimer / m_timeToTurn));
                // Set drift angle
                targetOrientation *= Quaternion.AngleAxis(angle, Vector3.up);
            }

            // Set kart position and rotation
            transform.position = m_targetPointAhead;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetOrientation, 0.8f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!m_isControlable)
        {
            return;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(m_closestPosition, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawLine(m_closestPosition, transform.position);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_targetPointAhead, 0.2f);
    }

    // -------------------------------------------------------------------------------------------------------

    public void EnableControl()
    {
        m_isControlable = true;
    }

    public void DisableControl()
    {
        m_isControlable = false;
    }

    public bool IsControlled()
    {
        return m_isControlable;
    }

    public void DriftInput(bool enable)
    {
        m_detectingDrift = enable;
    }

    public void DriftOutput()
    {
        if(m_driftEnabled)
        {
            m_driftEnabled = false;
            m_driftTurning = true;
            onDriftFinished?.Invoke();
        }
    }

    // -------------------------------------------------------------------------------------------------------

    private void CalculateKartSplineOffset()
    {
        // Get nearest point in spline
        float nearestTF = m_trackSpline.GetNearestPointTF(transform.position, out m_closestPosition);
        // Get orientation at that total fragment (TF)
        m_currentOrientation = m_trackSpline.GetOrientationFast(nearestTF);
        // Project offset vector on road plane to get lane offset
        Vector3 carProjectionToRoadCenter = Vector3.ProjectOnPlane((transform.position - m_closestPosition), m_currentOrientation * Vector3.up);
        // Get current distance over spline
        m_currentDistance = m_trackSpline.TFToDistance(nearestTF);
        // Get lane position
        m_kartlaneOffset = Vector3.Dot(carProjectionToRoadCenter, m_currentOrientation * Vector3.right);

        // Get additional info
        float refDistance = m_currentDistance + m_referenceDistance;
        float refTF = m_trackSpline.DistanceToTF(refDistance > m_trackSpline.Length ? refDistance - m_trackSpline.Length : refDistance);
        m_referenceOrientation = m_trackSpline.GetOrientationFast(refTF);
    }

    // -------------------------------------------------------------------------------------------------------

}
