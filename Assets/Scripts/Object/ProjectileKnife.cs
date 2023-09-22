using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ProjectileKnife : MonoBehaviour
{
    #region PublicVariables
    #endregion

    #region PrivateVariables
    private Rigidbody m_rb;

    private bool m_targetHit;
    #endregion

    #region PublicMethod
    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
    }
    #endregion

    #region PrivateMethod
    private void OnCollisionEnter(Collision collision)
    {
        if (m_targetHit)
        {
            return;
        }
        
        m_targetHit = true;

        m_rb.isKinematic = true;

        //transform.SetParent(collision.transform);
        transform.lossyScale.Set(1, 1, 1);
    }
    #endregion
}
