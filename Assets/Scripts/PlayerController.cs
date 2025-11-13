using System;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    float m_Yaw;
    float m_Pitch;
    public float m_YawSpeed;
    public float m_PitchSpeed;
    public float m_MinPitch;
    public float m_MaxPitch;
    public Transform m_PitchController;
    public bool m_UseInvertedYaw;
    public bool m_UseInvertedPitch;
    public CharacterController m_CharacterController;
    float m_VerticalSpeed = 0.0f;

    [Header("Spawn")]
    public Transform m_SpawnPoint;
    Vector3 m_StartPosition;
    Vector3 l_Position;
    Quaternion m_StartRotation;

    [Header("Camera")]
    public Camera m_Camera;
    bool m_AngleLocked = false;
    public float m_Speed;
    public float m_JumpSpeed;
    public float m_SpeedMultiplier;

    [Header("Input")]
    public KeyCode m_LeftKeycode = KeyCode.A;
    public KeyCode m_RightKeycode = KeyCode.D;
    public KeyCode m_UpKeycode = KeyCode.W;
    public KeyCode m_DownKeycode = KeyCode.S;
    public KeyCode m_JumpKeycode = KeyCode.Space;
    public KeyCode m_RunKeycode = KeyCode.LeftShift;
    public KeyCode m_GrabKeyCode = KeyCode.E;
    public KeyCode m_GetDamage = KeyCode.K;
    public KeyCode m_Interact = KeyCode.F;
    public int m_BlueShootMouseButton = 0;
    public int m_OrangeShootMouseButton = 1;

    [Header("Debug Input")]
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;

    [Header("Player Shooting")]
    public LayerMask m_hitLayer;
    public float m_ShootMaxDist = 50.0f;

    [Header("Animations")]
    public Animation m_Animation;
    public AnimationClip m_ShootAnimationClip;
    public AnimationClip m_IdleAnimationClip;

    [Header("Attach Object")]
    public ForceMode m_ForceMode;
    public float m_ThrowForce = 10.0f;
    Rigidbody m_AttachedObjectRigidbody;
    public bool m_AttachingObject;
    public Transform m_GripTransform;
    Vector3 m_StartAttachingObjectPosition;
    float m_AttachingCurrentTime;
    float m_AttachingTime = 1.5f;
    public float m_AttachingObjectRotationDistanceLerp = 2.0f;
    public bool m_AttachedObject;
    public LayerMask m_ValidAttachObjectsLayerMask;

    [Header("Portal")]
    public float m_PortalDistance = 3f;
    public float m_MaxAngleToTeleport = 75f;
    Vector3 m_MovementDirection;
    public Portal m_BluePortal;
    public Portal m_OrangePortal;

    [Header("Portal Size")]
    public Vector3 m_MaxPortalScale = new Vector3(2f, 2f, 2f);
    public Vector3 m_MinPortalScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 m_DefaultPortalScale = new Vector3(1f, 1f, 1f);
    int m_scrollLevel = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_StartPosition = m_SpawnPoint.position;
        m_StartRotation = m_SpawnPoint.rotation;

        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
    }
    void Update()
    {

        float l_MouseX = Input.GetAxis("Mouse X");
        float l_MouseY = Input.GetAxis("Mouse Y");

        if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked = !m_AngleLocked;

        if (!m_AngleLocked)
        {
            m_Yaw = m_Yaw + l_MouseX * m_YawSpeed * Time.deltaTime * (m_UseInvertedYaw ? -1.0f : 1.0f);
            m_Pitch = m_Pitch + l_MouseY * m_PitchSpeed * Time.deltaTime * (m_UseInvertedPitch ? -1.0f : 1.0f);
            m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
            m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);
        }

        Vector3 l_Movement = Vector3.zero;
        float l_YawPiRadians = m_Yaw * Mathf.Deg2Rad;
        float l_Yaw90PiRadians = (m_Yaw + 90.0f) * Mathf.Deg2Rad;
        Vector3 l_ForwardDirection = new Vector3(Mathf.Sin(l_YawPiRadians), 0.0f, Mathf.Cos(l_YawPiRadians));
        Vector3 l_RightDirection = new Vector3(Mathf.Sin(l_Yaw90PiRadians), 0.0f, Mathf.Cos(l_Yaw90PiRadians));

        if (Input.GetKey(m_RightKeycode))
            l_Movement = l_RightDirection;
        else if (Input.GetKey(m_LeftKeycode))
            l_Movement = -l_RightDirection;

        if (Input.GetKey(m_UpKeycode))
            l_Movement += l_ForwardDirection;
        else if (Input.GetKey(m_DownKeycode))
            l_Movement -= l_ForwardDirection;

        float l_SpeedMultiplier = 1.0f;

        if (Input.GetKey(m_RunKeycode))
            l_SpeedMultiplier = m_SpeedMultiplier;

        l_Movement.Normalize();
        m_MovementDirection = l_Movement;
        l_Movement *= m_Speed * l_SpeedMultiplier * Time.deltaTime;

        m_VerticalSpeed = m_VerticalSpeed + Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);
        if (m_VerticalSpeed < 0.0f && (l_CollisionFlags & CollisionFlags.Below) != 0)
        {
            m_VerticalSpeed = 0.0f;
            if (Input.GetKeyDown(m_JumpKeycode))
                m_VerticalSpeed = m_JumpSpeed;
        }
        else if (m_VerticalSpeed > 0.0f && (l_CollisionFlags & CollisionFlags.Above) != 0)
            m_VerticalSpeed = 0.0f;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if ((CanShoot()))
        {
            if (Input.GetMouseButton(m_BlueShootMouseButton))
            {
                if (scroll > 0f)
                {
                    m_scrollLevel++;
                    if (m_scrollLevel > 1) { m_scrollLevel = 1; }

                    if (m_scrollLevel == 1)
                    {
                        m_BluePortal.transform.localScale= m_MaxPortalScale;
                    }
                    if (m_scrollLevel == 0)
                    {
                        m_BluePortal.transform.localScale = m_DefaultPortalScale;
                    }
                }
                else if (scroll < 0f)
                {
                    if (m_scrollLevel < -1) { m_scrollLevel = -1; }

                    m_scrollLevel--;
                    if (m_scrollLevel == 0)
                    {
                        m_BluePortal.transform.localScale = m_DefaultPortalScale;
                    }
                    if (m_scrollLevel == -1)
                    {
                        m_BluePortal.transform.localScale = m_MinPortalScale;
                    }
                }
                Previsualization(m_BluePortal);
            }
            if (Input.GetMouseButton(m_OrangeShootMouseButton))
            {
                if (scroll > 0f)
                {
                    m_scrollLevel++;
                    if (m_scrollLevel > 1) { m_scrollLevel = 1; }

                    if (m_scrollLevel == 1)
                    {
                        m_OrangePortal.transform.localScale = m_MaxPortalScale;
                    }
                    if (m_scrollLevel == 0)
                    {
                        m_OrangePortal.transform.localScale = m_DefaultPortalScale;
                    }
                }
                else if (scroll < 0f)
                {
                    if (m_scrollLevel < -1) { m_scrollLevel = -1; }

                    m_scrollLevel--;
                    if (m_scrollLevel == 0)
                    {
                        m_OrangePortal.transform.localScale = m_DefaultPortalScale;
                    }
                    if (m_scrollLevel == -1)
                    {
                        m_OrangePortal.transform.localScale = m_MinPortalScale;
                    }
                }
                Previsualization(m_OrangePortal);
            }
            if (Input.GetMouseButtonUp(m_BlueShootMouseButton))
            {
                Shoot(m_BluePortal);
            }
            if (Input.GetMouseButtonUp(m_OrangeShootMouseButton))
            {
                Shoot(m_OrangePortal);
            }
        }

        if (CanAttachObject())
            AttachObject();

        if (m_AttachedObjectRigidbody != null)
        {
            UpdateAttachedObject();
        }

        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDist, m_ValidAttachObjectsLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            Debug.Log(l_RaycastHit.collider.name);
            float l_Distance = Vector3.Distance(l_Position, transform.position);
            if (l_RaycastHit.collider.CompareTag("PortalButton") && Input.GetKeyDown(m_Interact) && l_Distance < 15)
            {
                Debug.Log("Interacted with Portal Button");
                l_RaycastHit.collider.GetComponent<PortalButton>().m_Event.Invoke();
            }
        }
    }
    bool CanAttachObject()
    {
        return true;
    }
    bool CanShoot()
    {
        return m_AttachedObjectRigidbody == null && !Input.GetKey(KeyCode.E);
    }
    private void Shoot(Portal _Portal)
    {

        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDist, _Portal.m_ValidPortalLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            if (l_RaycastHit.collider.CompareTag("DrawableWall"))
            {
                if (_Portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
                {
                    _Portal.gameObject.SetActive(true);
                }
                else
                {
                    _Portal.gameObject.SetActive(false);
                }
            }
        }
    }

    private void Previsualization(Portal _Portal)
    {

        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDist, _Portal.m_ValidPortalLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            if (l_RaycastHit.collider.CompareTag("DrawableWall"))
            {

                if (_Portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
                {
                    _Portal.transform.position = l_RaycastHit.point;
                    _Portal.transform.rotation = Quaternion.LookRotation(l_RaycastHit.normal);

                    _Portal.gameObject.SetActive(true);
                }
                else
                {
                    _Portal.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            Portal l_Portal = other.GetComponent<Portal>();
            if (CanTeleport(l_Portal))
            {
                Teleport(l_Portal);
            }
        }
    }

    bool CanTeleport(Portal _Portal)
    {
        float l_DotValue = Vector3.Dot(_Portal.transform.forward, -m_MovementDirection);
        return l_DotValue > Mathf.Cos(m_MaxAngleToTeleport * Mathf.Deg2Rad);
    }

    void Teleport(Portal _Portal)
    {
        Vector3 l_NextPosition = transform.position + m_MovementDirection * m_PortalDistance;
        Vector3 l_LocalPosition = _Portal.m_OtherPortalTransform.InverseTransformPoint(l_NextPosition);
        Vector3 l_WorldPosition = _Portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_WorldForward = transform.forward;
        Vector3 l_LocalForward = _Portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldPosition);
        l_WorldForward = _Portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        m_CharacterController.enabled = false;
        transform.position = l_WorldPosition;
        transform.rotation = Quaternion.LookRotation(l_WorldForward);
        m_Yaw = transform.rotation.eulerAngles.y;
        m_CharacterController.enabled = true;

    }
    void AttachObject()
    {
        if (Input.GetKeyDown(m_GrabKeyCode))
        {
            Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDist, m_ValidAttachObjectsLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                if (l_RaycastHit.collider.CompareTag("Cube") || l_RaycastHit.collider.CompareTag("Turret") || l_RaycastHit.collider.CompareTag("RefractionCube"))
                {
                    AttachObject(l_RaycastHit.rigidbody);
                }
            }
        }
    }
    void AttachObject(Rigidbody _Rigidbody)
    {
        m_AttachingObject = true;
        m_AttachedObjectRigidbody = _Rigidbody;
        m_AttachedObjectRigidbody.GetComponent<CompanionCube>().SetAttachedObject(true);
        m_StartAttachingObjectPosition = _Rigidbody.transform.position;
        m_AttachingCurrentTime = 0.0f;
        m_AttachedObject = false;
    }
    void UpdateAttachedObject()
    {
        if (m_AttachingObject)
        {
            m_AttachingCurrentTime += Time.deltaTime;
            float l_Pct = m_AttachingCurrentTime / m_AttachingTime;
            Vector3 l_Position = Vector3.Lerp(m_StartAttachingObjectPosition, m_GripTransform.position, l_Pct);
            float l_Distance = Vector3.Distance(l_Position, m_GripTransform.position);
            float l_RotationPct = 1.0f - Mathf.Min(1.0f,(l_Distance / m_AttachingObjectRotationDistanceLerp));
            Quaternion l_Rotation = Quaternion.Lerp(transform.rotation, m_GripTransform.rotation, l_RotationPct);
            m_AttachedObjectRigidbody.MovePosition(l_Position);
            m_AttachedObjectRigidbody.MoveRotation(l_Rotation);
            if (l_Pct == 1.0f) 
            {
                m_AttachingObject = false;
                m_AttachedObject = true;
                m_AttachedObjectRigidbody.transform.SetParent(m_GripTransform);
                m_AttachedObjectRigidbody.transform.localPosition = Vector3.zero;
                m_AttachedObjectRigidbody.transform.localRotation = Quaternion.identity;
                m_AttachedObjectRigidbody.isKinematic = true;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            ThrowObject(m_ThrowForce);
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetKeyUp(m_GrabKeyCode))
        {
            ThrowObject(0.0f);
        }
    }

    void ThrowObject(float Force)
    {
        m_AttachedObjectRigidbody.isKinematic = false;
        m_AttachedObjectRigidbody.AddForce(m_PitchController.forward * Force, m_ForceMode);
        m_AttachingObject = false;
        m_AttachedObject = false;
        m_AttachedObjectRigidbody.GetComponent<CompanionCube>().SetAttachedObject(false);
        m_AttachedObjectRigidbody = null;

    }
    public void Restart()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
}
