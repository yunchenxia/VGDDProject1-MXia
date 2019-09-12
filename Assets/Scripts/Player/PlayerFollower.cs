using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    private Transform m_PlayerTransform;

    [SerializeField]
    private Vector3 m_Offset;

    [SerializeField]
    private float m_Rotation = 10;
    #endregion

    #region Main UPdates
    private void LateUpdate()
    {
        Vector3 newPos = m_PlayerTransform.position + m_Offset;

        transform.position = Vector3.Slerp(transform.position, newPos,1);

        float rotationAmount = m_Rotation * Input.GetAxis("Mouse X");
        transform.RotateAround(m_PlayerTransform.position, Vector3.up, rotationAmount);

        m_Offset = transform.position - m_PlayerTransform.position;
    }
    #endregion


}
