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

    //public bool InitialPosition(Vector3 _Postion, Vector3 _Normal)
    //{
    //    transform.position = _Postion;

    //    for (int i = 0; i < m_ValidPoints.Count; i++)
    //    {
    //        Vector3 l_ValidPosition = m_ValidPoints[i].position;
    //        Vector3 l_Direction = m_ValidPoints - l_CameraPosition;
    //        float l_Distance = Vector3.Distance(m_ValidPoints, l_CameraPosition);
    //        l_Direction.Normalize();
    //        l_Direction /= l_Distance;
    //        Ray l_Ray = new Ray(l_CameraPosition, l_Direction);

    //        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, l_Distance + m_ValidDistanceOffset, m_ValidPortalLayerMask.value, ))
    //        {
    //            if (l_RaycastHit.collider.CompareTag("DrawableWall"))
    //            {
    //                if (Vector3.Distance(RayCastHit.point, l_ValidPosition))
    //                {
    //                    float l_DotValue = Vector3.Dot(l_RaycastHit.normal, m_ValidPoints[i].forward);
    //                    if (l_DotAngle.Mathf.Cos(m_MaxAnglePermitted * Mathf.Deg2Rad))
    //                    {
    //                        return true;
    //                    }
    //                }
    //                else
    //                    return false;
    //            }
    //            else
    //                return false;
    //        }
    //        else
    //            return false;
    //    }
    //}

}
