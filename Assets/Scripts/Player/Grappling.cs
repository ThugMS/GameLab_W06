using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    #region PublicVariables
    public bool m_isGrappling = false;
    public LineRenderer m_line;
    #endregion

    #region PrivateVariables
    [Header("References")]
    [SerializeField] private PlayerMovement m_pm;
    [SerializeField] private Transform m_camera;
    [SerializeField] private Transform m_gunTip;
    [SerializeField] private LayerMask m_grappleLayer;

    [Header("Garppling")]
    [SerializeField] private float m_maxGrapplerDistance;
    [SerializeField] private float m_grappleDelayTime;
    [SerializeField] private Vector3 m_grapplePoint;
    [SerializeField] private float m_overShootYAxis;

    [Header("Cooldown")]
    [SerializeField] private float m_grapplingCooldown;
    [SerializeField] private float m_grapplingTimer;

    [Header("Input")]
    [SerializeField] private KeyCode m_grappleKey = KeyCode.Q;
    #endregion

    #region PublicMethod
    private void Start()
    {
        m_pm = GetComponent<PlayerMovement>();

    }

    private void Update()
    {
        if (Input.GetKeyDown(m_grappleKey))
        {
            StartGrapple();
        }

        if (m_grapplingTimer > 0)
            m_grapplingTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (m_isGrappling == true)
        {
            DrawRope();
        }
    }
    #endregion

    #region PrivateMethod
    private void StartGrapple()
    {
        if (m_grapplingTimer > 0)
        {
            return;
        }

        m_isGrappling = true;

        m_pm.m_isFreeze = true;

        RaycastHit hit;

        if (Physics.Raycast(m_camera.position, m_camera.forward, out hit, m_maxGrapplerDistance, m_grappleLayer))
        {
            m_grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), m_grappleDelayTime);
        }
        else
        {
            m_grapplePoint = m_camera.position + m_camera.forward * m_maxGrapplerDistance;

            Invoke(nameof(StopGrapple), m_grappleDelayTime);
        }
}

private void ExecuteGrapple()
    {
        m_pm.m_isFreeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = m_grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + m_overShootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = m_overShootYAxis;

        m_pm.JumpToPosition(m_grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        m_isGrappling = false;
        m_pm.m_isFreeze = false;

        m_grapplingTimer = m_grapplingCooldown;
        m_line.positionCount = 0;
    }

    private void DrawRope()
    {
        m_line.positionCount = 2;
        m_line.SetPosition(0, m_gunTip.position);
        m_line.SetPosition(1, m_grapplePoint);
    }
    #endregion
}
