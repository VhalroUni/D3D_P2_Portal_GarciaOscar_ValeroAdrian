using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Portal : MonoBehaviour
{
    public Camera m_Camera;
    public Transform m_OtherPortalTransform;
    public Portal m_MirrorPortal;
    public float m_NearCameraOffset = 0.5f;
    public List<Transform> m_ValidPositions;


    public void LateUpdate()
    {
        Vector3 l_WorldPosition = Camera.main.transform.position;
        Vector3 l_LocalPosition = m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);
        m_MirrorPortal.m_Camera.transform.position = m_MirrorPortal.transform.InverseTransformPoint(l_LocalPosition);

        Vector3 l_WorldForward = Camera.main.transform.forward;
        Vector3 l_LocalForward = m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);
        m_MirrorPortal.m_Camera.transform.forward = m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        float l_DisatnceToPortal = Vector3.Distance(m_MirrorPortal.transform.position, m_MirrorPortal.m_Camera.transform.position);
        m_MirrorPortal.m_Camera.nearClipPlane = l_DisatnceToPortal + m_NearCameraOffset;
    }
}
