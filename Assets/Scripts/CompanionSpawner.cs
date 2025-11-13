using Unity.VisualScripting;
using UnityEngine;

public class CompanionSpawner : MonoBehaviour
{
    public GameObject m_CompanionCubePrefab;
    public Transform m_SpawnerTransform;

    public void Spawn()
    {
        Debug.Log("Spawning Companion Cube");
        GameObject l_GameObject = GameObject.Instantiate(m_CompanionCubePrefab);
        l_GameObject.transform.position = m_SpawnerTransform.position;
        l_GameObject.transform.rotation = m_SpawnerTransform.rotation;
    }
}