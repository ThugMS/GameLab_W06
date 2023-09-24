using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swing : MonoBehaviour
{
    #region PublicVariables
    public LineRenderer m_swingline;
    public Transform m_gunTip, m_cam, m_player;
    public LayerMask m_grappleLayer;
    public PlayerMovement m_pm;
    #endregion

    #region PrivateVariables
    [Header("Swinging")]
    private float m_maxSwingDistance = 25f;
    private Vector3 m_swingPoint;
    private Vector3 m_currentGrapplePosition;
    private SpringJoint joint;

    [Header("OdmGear")]
    public Transform m_orientation;
    public Rigidbody m_rb;
    public float m_horizontalThrustForce;
    public float m_forwardThrustForce;
    public float m_extendCableSpeed;

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

        if(joint != null)
        {
            OdmGearMovement();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }
    #endregion

    #region PrivateMethod
    private void StartSwing()
    {
        m_pm.m_isSwinging = true;
        m_pm.m_isSwingPressed = true;

        RaycastHit hit;

        if(Physics.Raycast(m_cam.position, m_cam.forward, out hit, m_maxSwingDistance, m_grappleLayer))
        {
            m_swingPoint = hit.point;
            joint = m_player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = m_swingPoint;

            float distanceFromPoint = Vector3.Distance(m_player.position, m_swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;
            //joint.breakForce = 1f;

            m_swingline.positionCount = 2;
            m_currentGrapplePosition = m_gunTip.position;
        }
    }

    private void StopSwing()
    {
        m_pm.m_isSwingPressed = false;

        m_swingline.positionCount = 0;
        Destroy(joint);
    }

    private void OdmGearMovement()
    {
        // аб©Л, ╬у
        if (Input.GetKey(KeyCode.D))
        {
            m_rb.AddForce(m_orientation.right * m_horizontalThrustForce * Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.A)) 
        {
            m_rb.AddForce(-m_orientation.right * m_horizontalThrustForce * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            //Vector3 v = m_swingPoint - m_player.position;
            
            Vector3 v = new Vector3(m_orientation.forward.x, m_rb.velocity.normalized.y, m_orientation.forward.z);
            m_rb.AddForce(v * m_forwardThrustForce * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = m_swingPoint - transform.position;
            m_rb.AddForce(directionToPoint.normalized * m_forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, m_swingPoint);

            if(joint.minDistance > 5f)
            {
                joint.maxDistance = distanceFromPoint * 0.8f;
                joint.minDistance = distanceFromPoint * 0.25f;
            }
        }

        if(Input.GetKey(KeyCode.S))
        {
            //float extendedDistanceFromPoint = Vector3.Distance(transform.position, m_swingPoint) + m_extendCableSpeed;

            //joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            //joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    private void DrawRope()
    {
        if(joint == false)
        {
            return;
        }

        m_currentGrapplePosition = Vector3.Lerp(m_currentGrapplePosition, m_swingPoint, Time.deltaTime * 8f);

        m_swingline.enabled = true;
        m_swingline.SetPosition(0, m_gunTip.position);
        m_swingline.SetPosition(1, m_swingPoint);
    }
    #endregion
}
