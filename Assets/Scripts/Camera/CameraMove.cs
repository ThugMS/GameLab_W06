using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    #region PublicVariables
    public Transform m_cameraPosition;
    #endregion

    #region PrivateVariables
    #endregion

    #region PublicMethod
    private void Update()
    {
        transform.position = m_cameraPosition.position;
    }
    #endregion

    #region PrivateMethod
    #endregion
}
