using UnityEngine;

public class CompanionCube : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    public float m_PortalDistance = 1.5f;
    public float m_MaxAngleToTeleport = 75f;
    bool m_AttachedObject = false;

    [Header("Cube Size")]
    public Vector3 m_MaxCubeScale = new Vector3(2f, 2f, 2f);
    public Vector3 m_MinCubeScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 m_DefaultCubeScale = new Vector3(1f, 1f, 1f);
    int m_CubeLevelSize = 0;
    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        Portal l_Portal = other.GetComponent<Portal>();
        if (other.CompareTag("Portal"))
        {
            if (CanTeleport(l_Portal))
                Teleport(l_Portal);
        }
    }
    bool CanTeleport(Portal _Portal)
    {
        if (gameObject.CompareTag("Cube"))
        {
            float l_InternalScale = _Portal.transform.localScale.x;
            if (l_InternalScale == 1f && m_CubeLevelSize == 1)
            {
                return false;
            }
            if (l_InternalScale == 0.5f && m_CubeLevelSize >= 0)
            {
                return false;
            }
            float l_DotValue = Vector3.Dot(_Portal.transform.forward, - m_Rigidbody.linearVelocity.normalized);
            return !m_AttachedObject && l_DotValue > Mathf.Cos(m_MaxAngleToTeleport * Mathf.Deg2Rad);
        }
        else
        {
            return false;
        }
    }
    void Teleport(Portal _Portal)
    {
        Vector3 l_Direction = m_Rigidbody.linearVelocity.normalized;
        Vector3 l_WorldPosition = transform.position + l_Direction * m_PortalDistance;
        Vector3 l_LocalPosition =_Portal.m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);
        transform.position = _Portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_WorldDirection = transform.forward;
        Vector3 l_LocalDirection = _Portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldDirection);
        transform.forward = _Portal.m_MirrorPortal.transform.TransformPoint(l_LocalDirection);

        Vector3 l_LocalVelocity = _Portal.m_OtherPortalTransform.InverseTransformDirection(m_Rigidbody.linearVelocity);
        m_Rigidbody.linearVelocity=_Portal.m_MirrorPortal.transform.TransformDirection(l_LocalVelocity);

        float l_Scale = _Portal.m_MirrorPortal.transform.localScale.x / _Portal.transform.localScale.x;
        float l_InternalScale = _Portal.transform.localScale.x;

        if (l_Scale == 2f || l_Scale == 4f)
        {
            m_CubeLevelSize++;
            if (m_CubeLevelSize > 1) {m_CubeLevelSize = 1; }

            if (m_CubeLevelSize == 1)
            {
                transform.localScale = m_MaxCubeScale;
            }
            if (m_CubeLevelSize == 0)
            {
                transform.localScale = m_DefaultCubeScale;
            }
        }
        if (l_Scale == 0.5f || l_Scale == 0.25f)
        {
            if (m_CubeLevelSize < -1) { m_CubeLevelSize = -1; }

            m_CubeLevelSize--;
            if (m_CubeLevelSize == 0)
            {
                transform.localScale = m_DefaultCubeScale;
            }
            if (m_CubeLevelSize == -1)
            {
                transform.localScale = m_MinCubeScale;
            }
        }
    }

    public void SetAttachedObject(bool AttachedObject)
    {
        m_AttachedObject = AttachedObject;
    }
}
