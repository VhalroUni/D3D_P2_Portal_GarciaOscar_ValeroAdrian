using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Turret : MonoBehaviour
{
    public LineRenderer m_LineRenderer;
    public float m_MaxDistance = 50.0f;
    public float m_MaxAlifeAngle = 15.0f;
    public LayerMask m_LayerMask;

    public void Update()
    {
        float l_DotAngle = Vector3.Dot(transform.up, Vector3.up);
        if (l_DotAngle < Mathf.Cos(m_MaxAlifeAngle * Mathf.Deg2Rad))
        {
            Die();
        }
        else
        {
            if (m_LineRenderer != null)
            {
                float l_Distance = m_MaxDistance;
                Ray l_Ray = new Ray(m_LineRenderer.transform.position, m_LineRenderer.transform.forward);
                if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_LayerMask.value, QueryTriggerInteraction.Ignore))
                {
                    l_Distance = l_RaycastHit.distance;
                    if (l_RaycastHit.collider.CompareTag("RefractionCube"))
                    {
                        l_RaycastHit.collider.GetComponent<RefractionCube>().Reflect();
                    }
                    if (l_RaycastHit.collider.CompareTag("Player"))
                    {
                        l_RaycastHit.collider.GetComponent<PlayerController>().Restart();
                    }
                    if (l_RaycastHit.collider.CompareTag("Turret"))
                    {
                        l_RaycastHit.collider.GetComponent<Turret>().Die();
                    }
                }
                Vector3 l_Position = new Vector3(0.0f, 0.0f, l_Distance);
                m_LineRenderer.SetPosition(1, l_Position);
            }
        }
    }

    public void Die()
    {
        GameObject.Destroy(m_LineRenderer);
    }
}
