using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Controller : MonoBehaviour, ITeleport, IRestartGame
{
    private CharacterController m_CharacterController;
    private PortalWeaponController m_PortalWeaponController; 
    public Transform m_PitchController;
    private float m_Yaw;
    private float m_Pitch;
    private float m_FootstepTimer;
    private float m_JumpDelay = 0.1f;
    private float m_JumpDelayTimer = 0f;
    public bool m_CanMove { get; set; } = true;
    private Quaternion m_StartRotation;
    private Vector3 m_StartPosition;
    public Camera m_Camera;

    [SerializeField] private GameObject m_CenterPlayer; 
    [SerializeField] private float m_YawSpeed;
    [SerializeField] private float m_pitchSpeed;
    [SerializeField] private float m_minPitch;
    [SerializeField] private float m_maxPitch;
    [SerializeField] public float m_Speed;
    [SerializeField] private float m_speedMultiplier;
    [SerializeField] private float m_verticalSpeed;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_footstepInterval;
    [SerializeField] private float m_GravityForce = 2;

    [Header("Keys")]
    private KeyCode m_LeftKeyCode = KeyCode.A;
    private KeyCode m_RightKeyCode = KeyCode.D;
    private KeyCode m_UpKeyCode = KeyCode.W;
    private KeyCode m_DownKeyCode = KeyCode.S;
    private KeyCode m_JumpKeyCode = KeyCode.Space;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_EnterPortalSound;
    [SerializeField] private AudioClip m_ExitPortalSound;
    private string m_CurrentSurfaceTag;

    [Header("Surfaces")]
    [SerializeField] private float m_MinBounceForce;
    private float m_InitialBounceSpeed;
    private bool m_HasBounced = false;
    public static Action OnPlayerLaunched;

    [Header("Portal")]
    public Vector3 m_MovementDirection;
    public float m_TeleportOffset;
    private bool m_EnterPortal;
    private Portal m_Portal;

    [Header("ZeroGravity")]
    private ZeroGravity m_ZeroGravity;
    private bool m_GravityZone;
    private float m_StartSpeed;
    private Vector3 m_PreviousOffsetFromPortal;

    [Header("Sounds")]
    [SerializeField] private ParticleSystem m_CheckpointParticles;

    public static Action<bool> OnCheckpointEntered;
    private bool m_AddPortalPhysics;
    private float m_TimeStopCharacterLerp = 0;
    private Vector3 m_ForwardLaunch;
    private Vector3 m_exitVelocity;
    private Vector3 m_CurrentVelocity;
    private float m_SpeedLaunch = 2;
    private Vector3 m_PreviousPosition;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_PortalWeaponController = GetComponent<PortalWeaponController>();
        GameManager.instance.SetPlayer(this);
    }

    void Start()
    {
        //m_Animator = GetComponent<Animator>();
        GameManager.instance.SetPlayer(this);
        GameManager.instance.AddRestartGame(this);
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;

        m_Yaw = transform.eulerAngles.y;
        m_Pitch = m_PitchController.localRotation.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
        m_FootstepTimer = 0f;
        m_GravityZone = false;
        m_StartSpeed = m_Speed;
        m_AddPortalPhysics = false;
    }

    void Update()
    {
        // Camera Movement
        float l_Horizontal = Input.GetAxis("Mouse X");
        float l_vertical = -Input.GetAxis("Mouse Y");

        m_Yaw = m_Yaw + l_Horizontal * m_YawSpeed * Time.deltaTime;
        m_Pitch = m_Pitch + l_vertical * m_pitchSpeed * Time.deltaTime;
        m_Pitch = Mathf.Clamp(m_Pitch, m_minPitch, m_maxPitch);

        transform.rotation = Quaternion.Euler(0, m_Yaw, 0);
        m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0, 0);

        float l_forwardAngle = m_Yaw * Mathf.Deg2Rad;
        float l_rightAngle = (m_Yaw + 90.0f) * Mathf.Deg2Rad;

        Vector3 l_forward = new Vector3(Mathf.Sin(l_forwardAngle), 0, Mathf.Cos(l_forwardAngle));
        Vector3 l_right = new Vector3(Mathf.Sin(l_rightAngle), 0, Mathf.Cos(l_rightAngle));

        if (!m_CanMove) return;

        //
        m_MovementDirection = Vector3.zero;

        if (Input.GetKey(m_RightKeyCode))
            m_MovementDirection = l_right;

        else if (Input.GetKey(m_LeftKeyCode))
            m_MovementDirection = -l_right;

        if (Input.GetKey(m_UpKeyCode))
            m_MovementDirection += l_forward;

        else if (Input.GetKey(m_DownKeyCode))
            m_MovementDirection -= l_forward;

        if (m_MovementDirection == Vector3.zero)
        {
            m_TimeStopCharacterLerp = Mathf.Clamp01(m_TimeStopCharacterLerp + Time.deltaTime * 10);
            m_MovementDirection = Vector3.Lerp(m_MovementDirection, Vector3.zero, m_TimeStopCharacterLerp);
        }
        else
        {
            m_TimeStopCharacterLerp = 0;
        }

        m_MovementDirection.Normalize();

        if (m_JumpDelayTimer > 0)
        {
            m_JumpDelayTimer -= Time.deltaTime;
        }

        if (m_HasBounced)
        {
            m_verticalSpeed = m_InitialBounceSpeed;
            m_HasBounced = false;
        }

        DetectSurface();

        if (m_CharacterController.isGrounded && Input.GetKey(m_JumpKeyCode) && m_JumpDelayTimer <= 0f)
        {
            m_verticalSpeed = m_JumpSpeed;
            m_JumpDelayTimer = m_JumpDelay;
        }

        Vector3 l_MovementDirection = m_MovementDirection * m_Speed * Time.deltaTime;

        if (m_GravityZone)
        {
            m_verticalSpeed = 0;
            l_MovementDirection.y = 0;
            m_Speed = m_StartSpeed / 4;
            l_MovementDirection += m_ZeroGravity.m_Direction * m_ZeroGravity.m_Speed * Time.deltaTime;
        }
        else if (m_AddPortalPhysics)
        {
            m_CurrentVelocity.y += Physics.gravity.y * Time.deltaTime * m_GravityForce;
            m_CharacterController.Move(m_CurrentVelocity * Time.deltaTime);
            m_verticalSpeed = 0.0f; 
        }
        else
        {
            m_Speed = m_StartSpeed;
            m_verticalSpeed += Physics.gravity.y * Time.deltaTime * m_GravityForce;
            l_MovementDirection.y = m_verticalSpeed * Time.deltaTime;
        }

        Debug.Log(m_CharacterController.velocity.magnitude);

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_MovementDirection);  

        if ((l_CollisionFlags & CollisionFlags.Below) != 0 && !m_GravityZone)
        {
            m_verticalSpeed = 0;
        }

        if ((l_CollisionFlags & CollisionFlags.Above) != 0 && m_verticalSpeed > 0.0f && !m_GravityZone)
            m_verticalSpeed = 0;

        if (m_CharacterController.velocity.magnitude > 0.001f && m_CharacterController.isGrounded)
        {
            HandleFootstepSound();
        }

        float l_DotMovementForward = Vector3.Dot(transform.forward, l_MovementDirection.normalized);

        if (m_EnterPortal)
        {
            Vector3 l_Offset = m_Portal.transform.position - m_CenterPlayer.transform.position;

            if (Vector3.Dot(m_Portal.transform.forward, l_Offset.normalized) > 0.0f)
            {
                Teleport(m_Portal);
                m_EnterPortal = false;
            }
        }

        if (Input.GetKey(KeyCode.T))
            Time.timeScale = 0.2f;
        else
            Time.timeScale = 1.0f; 
    }

    private void DetectSurface()
    {
        Vector3 l_FeetPosition = m_CharacterController.bounds.center - new Vector3(0, m_CharacterController.bounds.extents.y, 0);

        Ray ray = new Ray(l_FeetPosition, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 0.5f))
            m_CurrentSurfaceTag = hit.collider.tag;
        else
            m_CurrentSurfaceTag = "Untagged";
    }

    private void HandleFootstepSound()
    {
        m_FootstepTimer -= Time.deltaTime;

        if (m_FootstepTimer <= 0f)
        {
            switch (m_CurrentSurfaceTag)
            {
                case "Metal":
                    SoundsManager.instance.PlayFootstepSound(transform, 0.4f, SoundsManager.SurfaceType.Metal);
                    break;
                case "Rock":
                    SoundsManager.instance.PlayFootstepSound(transform, 0.4f, SoundsManager.SurfaceType.Rock);
                    break;
                case "Glass":
                    SoundsManager.instance.PlayFootstepSound(transform, 0.4f, SoundsManager.SurfaceType.Glass);
                    break;
                default:
                    SoundsManager.instance.PlayFootstepSound(transform, 0.4f, SoundsManager.SurfaceType.Default);
                    break;
            }

            m_FootstepTimer = m_footstepInterval;
        }
    }

    private void SetStartPosition(Vector3 l_position)
    {
        m_StartPosition = l_position;
    }

    public void SetSpeed()
    {
        m_Speed = m_StartSpeed;
    }

    public float GetSpeed() { return m_Speed; }

    public void RestartGame()
    {
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_AddPortalPhysics = false; 
        m_MovementDirection = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            m_EnterPortal = true;
            m_Portal = other.GetComponent<Portal>();
            Physics.IgnoreCollision(m_CharacterController, m_Portal.m_WallPortaled, true);
            m_PreviousOffsetFromPortal = m_Portal.transform.position - transform.position;
        }

        if (other.CompareTag("GravityZero"))
        {
            m_GravityZone = true;
            m_ZeroGravity = other.GetComponent<ZeroGravity>();
        }

        if (other.CompareTag("Checkpoint"))
        {
            CheckpointController l_CheckpointController;
            l_CheckpointController = other.GetComponent<CheckpointController>();

            if (l_CheckpointController.IsChecked() == false)
            {
                OnCheckpointEntered?.Invoke(true);
                SetStartPosition(l_CheckpointController.CheckPointPosition());
                l_CheckpointController.SetChecked(true);
                m_CheckpointParticles.Play();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            m_EnterPortal = false;
            m_Portal = other.GetComponent<Portal>();
            Physics.IgnoreCollision(m_CharacterController, m_Portal.m_WallPortaled, false);
        }

        if (other.CompareTag("GravityZero"))
        {
            m_ZeroGravity = other.GetComponent<ZeroGravity>();
            m_GravityZone = false;
        }
    }

    public void Teleport(Portal l_portal)
    {
        float l_Velocity = m_CharacterController.velocity.magnitude;

        float l_DotPortalToVectorUp = Mathf.Abs(Vector3.Dot(Vector3.up, l_portal.transform.forward));
        float l_DotMirrorPortalToVectorUp = Mathf.Abs(Vector3.Dot(Vector3.up, l_portal.m_MirrorPortal.transform.forward));
        float l_DotPortals = Vector3.Dot(l_portal.transform.forward, l_portal.m_MirrorPortal.m_MirrorPortal.transform.forward);
        m_AddPortalPhysics = true;

        if (l_DotPortalToVectorUp <= 0.1f)
        {
            //Entro y Salgo por portales en pared
            if (l_DotMirrorPortalToVectorUp <= 0.1f)
                m_AddPortalPhysics = false;

            //Entro por portal en pared y salgo por el suelo
            if (l_DotMirrorPortalToVectorUp >= 0.9)
            {
                if (l_Velocity <= 5.0f)
                {
                    l_Velocity = 8.0f;
                }
            }
        }

        if (l_DotPortalToVectorUp >= 0.9f)
        {
            //Entro y salgo por portales en el suelo
            if (l_DotMirrorPortalToVectorUp >= 0.9f)
            {
                if (l_Velocity <= 2.0f)
                {
                    l_Velocity = 8.0f;
                }
            }
        }

        m_ForwardLaunch = l_portal.m_MirrorPortal.transform.forward;
        m_CurrentVelocity = m_ForwardLaunch * l_Velocity;

        Vector3 l_Position = transform.position; //Obtener Posicion en mundo
        Vector3 l_LocalPosition = l_portal.m_OtherPortalTransform.InverseTransformPoint(l_Position); //Pasar de Posicion mundo a local
        Vector3 l_WorldPosition = l_portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition); //Convertir la local al otro portal. 

        Vector3 l_Forward = transform.forward;
        Vector3 l_LocalForward = l_portal.m_OtherPortalTransform.InverseTransformDirection(l_Forward);
        Vector3 l_WorldForward = l_portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        SoundsManager.instance.PlaySoundClip(m_EnterPortalSound, transform, 0.1f); 
        m_CharacterController.enabled = false;
        transform.position = l_WorldPosition;
        transform.forward = l_WorldForward;

        if(l_DotMirrorPortalToVectorUp <= 0.1f || l_DotPortalToVectorUp <=0.1f)
            m_Yaw = transform.eulerAngles.y;

        m_CharacterController.enabled = true;
        SoundsManager.instance.PlaySoundClip(m_ExitPortalSound, transform, 0.1f);
        Physics.IgnoreCollision(m_CharacterController, m_Portal.m_WallPortaled, false);

        if (m_PortalWeaponController.m_TrapedObject)
        {
            m_PortalWeaponController.m_ObjectAttract.transform.localScale =
                m_PortalWeaponController.m_ObjectAttract.GetComponent<TeleportableObjects>().m_StartSize * m_Portal.m_PortalSize;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        if (collision.collider.CompareTag("LaunchingSurface"))
        {
            OnPlayerLaunched?.Invoke();
        }
        else if (collision.collider.CompareTag("BouncingSurface"))
        {
            if (!m_HasBounced)
            {
                m_InitialBounceSpeed = Mathf.Max(Mathf.Abs(m_verticalSpeed), m_MinBounceForce);
                m_HasBounced = true;
            }
        }
        else
        {
            m_HasBounced = false;
        }

        if ((collision.collider.CompareTag("WhiteWall") || (collision.collider.CompareTag("Wall") 
            && m_AddPortalPhysics)))
        {
            m_AddPortalPhysics = false;
        }
    }
}

