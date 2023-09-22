using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwing : MonoBehaviour
{
    #region PublicVariables
    [Header("References")]
    public Transform m_camera;
    public Transform m_attackPoint;
    public GameObject m_objectToThrow;

    [Header("Settings")]
    public int m_totalThrows;
    public float m_throwCoolDown;

    [Header("Throwing")]
    public KeyCode m_throwKey = KeyCode.Mouse0;
    public float m_throwForce;
    public float m_throwUpwardForce;

    public bool m_readyToThrow;
    #endregion

    #region PrivateVariables
    #endregion

    #region PublicMethod
    private void Start()
    {
        m_readyToThrow = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(m_throwKey) && m_readyToThrow && m_totalThrows > 0)
        {
            Throw();
        }
    }
    #endregion

    #region PrivateMethod
    private void Throw()
    {
        m_readyToThrow = false;

        // instantiate object
        GameObject projectile = Instantiate(m_objectToThrow, m_attackPoint.position, m_camera.rotation);

        // get rigidbody
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // calculate diretion
        Vector3 forceDirection = m_camera.transform.forward;

        RaycastHit hit;

        if(Physics.Raycast(m_camera.position, m_camera.forward, out hit, 500f))
        {
            forceDirection = (hit.point - m_attackPoint.position).normalized;
        }

        // add force
        Vector3 forceToAdd = forceDirection * m_throwForce + transform.up * m_throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        m_totalThrows--;

        Invoke(nameof(ResetThrow), m_throwCoolDown);
    }

    private void ResetThrow()
    {
        m_readyToThrow = true;
    }
    #endregion
}
