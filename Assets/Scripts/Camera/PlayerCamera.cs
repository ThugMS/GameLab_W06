using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    #region PublicVariables
    public float m_sensX;
    public float m_sensY;

    public Transform m_orientation;
    #endregion

    #region PrivateVariables
    private float m_rotationX;
    private float m_rotationY;
    #endregion

    #region PublicMethod
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * m_sensX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * m_sensY;

        m_rotationY += mouseX;
        
        m_rotationX -= mouseY;
        m_rotationX = Mathf.Clamp(m_rotationX, -90f, 90f);

        transform.rotation = Quaternion.Euler(m_rotationX, m_rotationY, 0);
        m_orientation.rotation = Quaternion.Euler(0, m_rotationY, 0);
    }
    #endregion

    #region PrivateMethod
    #endregion
}
