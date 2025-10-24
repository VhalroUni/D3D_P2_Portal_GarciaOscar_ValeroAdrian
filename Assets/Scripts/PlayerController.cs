using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Assertions.Must;
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
    Vector3 m_StartPosition;
    Vector3 l_Position;
    Quaternion m_StartRotation;
    public Camera m_Camera;
    public


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
    public KeyCode m_GetDamage = KeyCode.K;
    public MouseButton m_BluePortal = MouseButton.Left;
    public MouseButton m_OrangePortal = MouseButton.Right;

    [Header("Debug Input")]
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;

    [Header("Player Shooting")]
    public LayerMask m_hitLayer;

    [Header("Animations")]
    public Animation m_Animation;
    public AnimationClip m_ShootAnimationClip;
    public AnimationClip m_IdleAnimationClip;

    [Header("Portal")]
    public float m_PortalDistance = 3f;
    public float m_MaxAngleToTeleport = 75f;
    Vector3 m_MovementDirection;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
        Vector3 l_WorldPosition = _Portal.m_MirrorPortal.transform.InverseTransformPoint(l_LocalPosition);

        Vector3 l_WorldForward = transform.forward;
        Vector3 l_LocalForward = _Portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldPosition);
        l_WorldForward = _Portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        m_CharacterController.enabled = false;
        transform.position = l_WorldPosition;
        transform.rotation = Quaternion.LookRotation(l_WorldForward);
        m_Yaw = transform.rotation.eulerAngles.y;
        m_CharacterController.enabled = true;

    }
}
