using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class Portal : MonoBehaviour
{
    public Camera m_Camera;
    public Transform m_OtherPortalTransform;
    public Portal m_MirrorPortal;
    public float m_NearCameraOffset = 0.5f;
    public List<Transform> m_ValidPositions;

    [Header("Validation")]
    public float m_ValidDistanceOffset = 0.15f;
    public LayerMask m_ValidPortalLayerMask;
    public float m_MaxAnglePermited = 0.5f;


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

    public bool IsValidPosition(Vector3 Position, Vector3 Normal)
    {
        gameObject.SetActive(true);
        transform.position = Position;
        transform.rotation = Quaternion.LookRotation(Normal);
        bool l_Valid = true;

        Vector3 l_CameraPosition = Camera.main.transform.position;
        for (int i = 0; i < m_ValidPositions.Count; i++)
        {
            Vector3 l_ValidPosition = m_ValidPositions[i].position;
            Vector3 l_Direction = l_ValidPosition - l_CameraPosition;
            float l_Distance = Vector3.Distance(l_ValidPosition, l_CameraPosition);
            l_Direction /= l_Distance;
            Ray l_Ray = new Ray(l_CameraPosition, l_Direction);
            if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, l_Distance + m_ValidDistanceOffset, m_ValidPortalLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                if (l_RaycastHit.collider.CompareTag("DrawableWall"))
                {
                    if (Vector3.Distance(l_RaycastHit.point, l_ValidPosition) < m_ValidDistanceOffset)
                    {
                        float l_DotAngle = Vector3.Dot(l_RaycastHit.normal, m_ValidPositions[i].forward);
                        if (l_DotAngle < Mathf.Cos(m_MaxAnglePermited * Mathf.Deg2Rad))
                            l_Valid = false;
                    }
                    else
                        l_Valid = false;
                }
                else
                    l_Valid = false;
            }
            l_Valid = false;
        }
        return l_Valid;
    }

    public bool InitialPosition(Vector3 _Postion, Vector3 _Normal)
    {
        transform.position = _postion;

        for (int i = 0; i < m_ValidPositions.Count; i++)
        {
            Vector3 l_Validposition = m_ValidPositions[i].position;
            Vector3 l_Direction = m_Validpoints - l_cameraposition;
            float l_Distance = vector3.distance(m_validpoints, l_cameraposition);
            l_Direction.normalize();
            l_Direction /= l_Distance;
            Ray l_ray = new Ray(l_CameraPosition, l_Direction);

            if (physics.raycast(l_ray, out raycasthit l_raycasthit, l_Distance + m_validdistanceoffset, m_validportallayermask.value, ))
            {
                if (l_raycasthit.collider.comparetag("drawablewall"))
                {
                    if (vector3.distance(raycasthit.point, l_validposition))
                    {
                        float l_dotvalue = Vector3.dot(l_raycasthit.normal, m_validpoints[i].forward);
                        if (l_dotangle.mathf.cos(m_maxanglepermitted * mathf.deg2rad))
                        {
                            return true;
                        }
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }
    }

}
