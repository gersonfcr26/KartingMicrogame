using FluffyUnderware.Curvy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartLaneMovement : MonoBehaviour
{
    [SerializeField]
    private CurvySpline m_trackSpline;

    private Vector3 m_closestPosition;
    private Quaternion m_currentOrientation;

    // Start is called before the first frame update
    void Start()
    {
        // Get nearest point in spline
        float nearestTF = m_trackSpline.GetNearestPointTF(transform.position, out m_closestPosition);
        // Get orientation at that total fragment (TF)
        m_currentOrientation = m_trackSpline.GetOrientationFast(nearestTF);
        // Project offset vector on road plane to get lane offset
        Vector3 carProjectionToRoadCenter = Vector3.ProjectOnPlane((transform.position - m_closestPosition), m_currentOrientation * Vector3.up);
        // Get lane position
        float lanePosition = Vector3.Dot(carProjectionToRoadCenter, m_currentOrientation * Vector3.right);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
