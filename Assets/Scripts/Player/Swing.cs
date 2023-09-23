using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swing : MonoBehaviour
{
    #region PublicVariables
    public LineRenderer m_swingline;
    public Transform m_gunTip, m_cam, m_player;
    public LayerMask m_grappleLayer;
    #endregion

    #region PrivateVariables
    [Header("Swinging")]
    private float m_maxSwingDistance = 25f;
    private Vector3 m_swingPoint;
    private Vector3 m_currentGrapplePosition;
    private SpringJoint joint;

    [SerializeField] private KeyCode m_swingKey = KeyCode.Q;
    #endregion

    #region PublicMethod
    private void Update()
    {
        if (Input.GetKeyDown(m_swingKey))
        {
            StartSwing();
        }

        if (Input.GetKeyUp(m_swingKey))
        {
            StopSwing();
        }
    }
    #endregion

    #region PrivateMethod
    private void StartSwing()
    {
        RaycastHit hit;

        if(Physics.Raycast(m_cam.position, m_cam.forward, out hit, m_maxSwingDistance, m_grappleLayer))
        {
            m_swingPoint = hit.point;
            joint = m_player.gameObject.GetComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = m_swingPoint;

            float distanceFromPoint = Vector3.Distance(m_player.position, m_swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            m_swingline.positionCount = 2;
            m_currentGrapplePosition = m_gunTip.position;
        }
    }

    private void StopSwing()
    {
        m_swingline.positionCount = 0;
        Destroy(joint);
    }

    private void DrawRope()
    {
        if(joint == false)
        {
            return;
        }

        m_currentGrapplePosition = Vector3.Lerp(m_currentGrapplePosition, m_swingPoint, Time.deltaTime * 8f);

        m_swingline.SetPosition(0, m_gunTip.position);
        m_swingline.SetPosition(1, m_swingPoint);
    }
    #endregion
}
