using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public UnityEvent m_Event;
    public UnityEvent m_Event2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
            m_Event.Invoke();
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Cube"))
        {
            m_Event2.Invoke();
        }
    }
}
