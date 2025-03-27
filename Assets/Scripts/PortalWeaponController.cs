using System.Collections.Generic;
using UnityEngine;

public class PortalWeaponController : MonoBehaviour, IRestartGame 
{
    [Header("Portal")]
    [SerializeField] private Camera m_Camera;
    [SerializeField] private GameObject m_BluePreviewPortal;
    [SerializeField] private GameObject m_OrangePreviewPortal;
    [SerializeField] private GameObject m_BluePortal;
    [SerializeField] private GameObject m_OrangePortal;
    [SerializeField] private float m_AngleValidPortal;
    [SerializeField] private float m_ScrollWheelIncrement;
    public List<Transform> m_ValidPoints = new List<Transform>();
    private CharacterController m_CharacterController;
    private RaycastHit m_HitCollisoned;
    private Vector3 m_StartScale;
    private float m_Angle;
    private float m_CurrentPortalSize;
    private float m_PreviewAnimation;

    [Header("Weapon")]
    [SerializeField] private PortalBullet m_BulletPortalBlue;
    [SerializeField] private PortalBullet m_BulletPortalOrange;
    [SerializeField] private Transform m_ShootPoint;
    [SerializeField] private float m_DistanceRay;
    [SerializeField] private float m_ThresholdPortal;
    [SerializeField] private GameObject m_CrossHairBlue;
    [SerializeField] private GameObject m_CrossHairOrange;
    [SerializeField] private GameObject m_CrossHairNoValidPosition;
    [SerializeField] private float m_ForceLaunch;
    [SerializeField] private LayerMask m_LayerMask;
    [SerializeField] private ParticleSystem m_AttractParticleSystem;
    [SerializeField] private ParticleSystem m_ReppeleParticleSystem;
    [SerializeField] private ParticleSystem m_FallParticleSystem;

    public GameObject m_ObjectAttract;
    private Transform m_AttachedPreviousParent;
    public Transform m_AttractPoint;
    private Rigidbody m_RbObjectAttract;
    private BoxCollider m_ObjectCollider;
    public float m_AttractSpeed;
    private float m_AttractingPorgress;
    public bool m_TrapedObject;
    private bool m_AttractingObjects;
    private bool m_CanShootBlue;
    private bool m_CanShootOrange;
    private int m_ReSize;

    [Header("Animatons&Particles")]
    [SerializeField] private Animator m_PortalWeaponAnimator;
    [SerializeField] private ParticleSystem m_BlueParticles;
    [SerializeField] private ParticleSystem m_OrangeParticles;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_BluePortalSound;
    [SerializeField] private AudioClip m_OrangePortalSound;

    private void Start()
    {
        GameManager.instance.AddRestartGame(this);

        m_AttractingObjects = false;
        m_TrapedObject = false;

        m_ReSize = 0;
        m_StartScale = m_BluePreviewPortal.transform.localScale;
        m_CurrentPortalSize = m_ReSize;
        m_PreviewAnimation = 0.0f;

        m_CrossHairBlue.SetActive(false);
        m_CrossHairOrange.SetActive(false); 
        m_CrossHairNoValidPosition.SetActive(false);

        m_Angle = Mathf.Cos(m_AngleValidPortal * Mathf.Deg2Rad);
        m_CharacterController = GetComponent<CharacterController>();

        //BULLET PORTAL
        m_BulletPortalBlue.gameObject.SetActive(false);
        m_BulletPortalBlue.transform.position = m_ShootPoint.position;

        m_BulletPortalOrange.gameObject.SetActive(false);
        m_BulletPortalOrange.transform.position = m_ShootPoint.position;

        m_CanShootBlue = true;
        m_CanShootOrange = true;
    }

    private void Update()
    {
        RaycastHit l_hit;
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        float l_ScrollWheel = Input.GetAxis("Mouse ScrollWheel");

        if (l_ScrollWheel > 0)
        {
            m_ReSize += 1;
            m_PreviewAnimation = 0;
        }
        else if (l_ScrollWheel < 0)
        {
            m_ReSize -= 1;
            m_PreviewAnimation = 0;
        }

        PreviewPortalAnimation();
        m_ReSize = Mathf.Clamp(m_ReSize, -1, 1);

        if (Physics.Raycast(l_Ray.origin, l_Ray.direction, out l_hit, m_DistanceRay, m_LayerMask.value))
        {
            if (!m_AttractingObjects && !m_TrapedObject)
            {
                //PORTAL
                m_BluePreviewPortal.transform.rotation = Quaternion.LookRotation(l_hit.normal);
                m_BluePreviewPortal.transform.position = l_hit.point;
                m_OrangePreviewPortal.transform.rotation = Quaternion.LookRotation(l_hit.normal);
                m_OrangePreviewPortal.transform.position = l_hit.point;

                if (IsValidPosition())
                {
                    //PREVIEW
                    if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                    {
                        if(!IsValidPosition())
                            m_CrossHairNoValidPosition.SetActive(true);

                        if (Input.GetMouseButton(0))
                        {
                            m_BluePreviewPortal.SetActive(true);
                            m_CanShootBlue = true;
                            m_CanShootOrange = false;
                        }
                        else if (Input.GetMouseButton(1))
                        {
                            m_OrangePreviewPortal.SetActive(true);
                            m_CanShootOrange = true;
                            m_CanShootBlue = false;
                        }
                    }
                    else
                    {
                        m_BluePreviewPortal.SetActive(false);
                        m_OrangePreviewPortal.SetActive(false);
                    }

                    if (Input.GetMouseButtonUp(0) && m_CanShootBlue)
                    {
                        m_HitCollisoned = l_hit;
                        m_BulletPortalBlue.gameObject.SetActive(true);
                        m_BulletPortalBlue.Shoot(m_ShootPoint.position, l_Ray.direction);
                        m_PortalWeaponAnimator.SetTrigger("Shoot");
                        m_BlueParticles.Play();
                        SoundsManager.instance.PlaySoundClip(m_BluePortalSound, transform, 0.05f);
                        m_CanShootBlue = false;
                    }

                    if (Input.GetMouseButtonUp(1) && m_CanShootOrange)
                    {
                        m_HitCollisoned = l_hit;
                        m_BulletPortalOrange.gameObject.SetActive(true);
                        m_BulletPortalOrange.Shoot(m_ShootPoint.position, l_Ray.direction);
                        m_PortalWeaponAnimator.SetTrigger("Shoot");
                        m_OrangeParticles.Play();
                        SoundsManager.instance.PlaySoundClip(m_OrangePortalSound, transform, 0.05f);
                        m_CanShootOrange = false;
                    }

                    m_CrossHairNoValidPosition.SetActive(false);

                }
                else
                {
                    m_BluePreviewPortal.SetActive(false);
                    m_OrangePreviewPortal.SetActive(false);
                }
            }

            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                if (!IsValidPosition())
                    m_CrossHairNoValidPosition.SetActive(true);
            }

            if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                    m_CrossHairNoValidPosition.SetActive(false);


            //ATTRACT OBJECTS
            if (l_hit.collider.CompareTag("CompanionCube") || l_hit.collider.CompareTag("Turret") || l_hit.collider.CompareTag("RefractionCube"))
            {
                if (Input.GetMouseButtonDown(0) && !m_TrapedObject && !m_AttractingObjects)
                {
                    AttractObject(l_hit);
                    m_AttractParticleSystem.Play();
                }
            }
        }

        if (m_BulletPortalBlue.m_Colisioned)
        {
            ActivePortalBlue(m_HitCollisoned);
            m_CanShootBlue = true;
            m_BulletPortalBlue.m_Colisioned = false;
        }

        if (m_BulletPortalOrange.m_Colisioned)
        {
            ActivePortalOrange(m_HitCollisoned);
            m_CanShootOrange = true;
            m_BulletPortalOrange.m_Colisioned = false;
        }

        if (m_ObjectAttract == null)
        {
            m_TrapedObject = false;
            m_AttractingObjects = false;
        }

        if (Input.GetMouseButtonDown(0) && m_TrapedObject)
        {
            m_ReppeleParticleSystem.Play();
            m_RbObjectAttract.isKinematic = false;
            m_ObjectAttract.transform.SetParent(m_AttachedPreviousParent);
            TeleportableObjects l_TeleportableObjects = m_ObjectAttract.GetComponent<TeleportableObjects>();
            l_TeleportableObjects.m_Catched = false;
            m_TrapedObject = false;
            m_RbObjectAttract.useGravity = true;
            m_ObjectCollider.enabled = true;
            m_RbObjectAttract.AddForce(m_Camera.transform.forward * m_ForceLaunch);
        }
        else if (Input.GetMouseButtonDown(1) && m_TrapedObject)
        {
            m_FallParticleSystem.Play();    
            m_RbObjectAttract.isKinematic = false;
            m_ObjectAttract.transform.SetParent(m_AttachedPreviousParent);
            TeleportableObjects l_TeleportableObjects = m_ObjectAttract.GetComponent<TeleportableObjects>();
            l_TeleportableObjects.m_Catched = false;
            m_TrapedObject = false;
            m_RbObjectAttract.useGravity = true;
            m_ObjectCollider.enabled = true;
        }

        if (m_CharacterController.velocity.magnitude > 1.0f)
        {
            m_PortalWeaponAnimator.SetFloat("Speed", m_CharacterController.velocity.magnitude);
        }
        else
            m_PortalWeaponAnimator.SetFloat("Speed", m_CharacterController.velocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (m_AttractingObjects)
        {
            m_AttractingPorgress += m_AttractSpeed * Time.deltaTime;

            m_ObjectAttract.transform.position = Vector3.Lerp(m_ObjectAttract.transform.position, m_AttractPoint.transform.position, m_AttractingPorgress);
            m_ObjectAttract.transform.forward = Vector3.Lerp(m_ObjectAttract.transform.forward, transform.forward, m_AttractingPorgress);

            if (m_AttractingPorgress >= 1)
            {
                m_ObjectAttract.transform.SetParent(m_AttractPoint.transform);
                TeleportableObjects l_TeleportableObjects = m_ObjectAttract.GetComponent<TeleportableObjects>();
                l_TeleportableObjects.m_Catched = true;
                m_ObjectAttract.transform.localPosition = Vector3.zero;
                m_AttractingObjects = false;
                m_TrapedObject = true;
                m_AttractingPorgress = 0;
            }
        }

        if (m_TrapedObject && m_RbObjectAttract != null)
        {
            m_RbObjectAttract.isKinematic = true;
        }
    }

    private void AttractObject(RaycastHit l_hit)
    {
        m_ObjectAttract = l_hit.collider.gameObject;
        m_RbObjectAttract = m_ObjectAttract.GetComponent<Rigidbody>();
        m_ObjectCollider = m_RbObjectAttract.GetComponent<BoxCollider>();
        m_AttachedPreviousParent = m_RbObjectAttract.transform.parent;

        m_AttractingObjects = true;
        m_RbObjectAttract.useGravity = false;
        m_RbObjectAttract.velocity = Vector3.zero;
        m_RbObjectAttract.angularVelocity = Vector3.zero;
    }

    private void ActivePortalBlue(RaycastHit l_hit)
    {
        m_BluePortal.SetActive(true);
        Portal l_portal = m_BluePortal.GetComponent<Portal>();
        l_portal.m_WallPortaled = l_hit.collider.GetComponent<Collider>();
        l_portal.transform.localScale = l_portal.m_StartSizeAnimation;
        l_portal.m_PortalSize = m_CurrentPortalSize;
        l_portal.m_PortalAnimation = true;
        m_CrossHairBlue.SetActive(true);
        m_BluePortal.transform.rotation = Quaternion.LookRotation(l_hit.normal);
        m_BluePortal.transform.position = l_hit.point;
    }

    private void ActivePortalOrange(RaycastHit l_hit)
    {
        m_OrangePortal.SetActive(true);
        Portal l_portal = m_OrangePortal.GetComponent<Portal>();
        l_portal.m_WallPortaled = l_hit.collider.GetComponent<Collider>();
        l_portal.transform.localScale = l_portal.m_StartSizeAnimation;
        l_portal.m_PortalSize = m_CurrentPortalSize;
        l_portal.m_PortalAnimation = true;
        m_CrossHairOrange.SetActive(true);
        m_OrangePortal.transform.rotation = Quaternion.LookRotation(l_hit.normal);
        m_OrangePortal.transform.position = l_hit.point;
    }

    public bool IsValidPosition()
    {
        bool isValid = true;
        RaycastHit l_hit;
        Vector3 l_CameraPosition = m_Camera.transform.position;

        for (int i = 0; i < m_ValidPoints.Count; i++)
        {
            Vector3 l_Diretion = m_ValidPoints[i].transform.position - m_Camera.transform.position;

            if (Physics.Raycast(l_CameraPosition, l_Diretion, out l_hit, m_DistanceRay, m_LayerMask.value))
            {
                float l_Dotangle = Vector3.Dot(l_hit.normal, m_ValidPoints[i].forward);

                if (Vector3.Distance(m_ValidPoints[i].transform.position, l_hit.point) >= m_ThresholdPortal ||
                    !l_hit.collider.CompareTag("WhiteWall") ||
                    l_Dotangle >= m_Angle || l_hit.collider.CompareTag("Portal"))
                {
                    return false;
                }
            }
            else
                return false;
        }
        return isValid;
    }

    public void PreviewPortalAnimation()
    {
        float l_speedAnimation = 1;
        m_PreviewAnimation += Time.deltaTime * l_speedAnimation;

        if (m_ReSize == 1)
        {
            m_BluePreviewPortal.transform.localScale = Vector3.Lerp(m_BluePreviewPortal.transform.localScale, m_StartScale * 2, m_PreviewAnimation);
            m_OrangePreviewPortal.transform.localScale = Vector3.Lerp(m_OrangePreviewPortal.transform.localScale, m_StartScale * 2, m_PreviewAnimation);
            m_CurrentPortalSize = 2;
        }
        else if (m_ReSize == 0)
        {
            m_BluePreviewPortal.transform.localScale = Vector3.Lerp(m_BluePreviewPortal.transform.localScale, m_StartScale, m_PreviewAnimation);
            m_OrangePreviewPortal.transform.localScale = Vector3.Lerp(m_OrangePreviewPortal.transform.localScale, m_StartScale, m_PreviewAnimation);
            m_CurrentPortalSize = 1;
        }
        else if (m_ReSize == -1)
        {
            m_BluePreviewPortal.transform.localScale = Vector3.Lerp(m_BluePreviewPortal.transform.localScale, m_StartScale / 2, m_PreviewAnimation);
            m_OrangePreviewPortal.transform.localScale = Vector3.Lerp(m_OrangePreviewPortal.transform.localScale, m_StartScale / 2, m_PreviewAnimation);
            m_CurrentPortalSize = 0.5f;
        }

        if (m_PreviewAnimation >= 1)
            m_PreviewAnimation = 0;
    }

    public void NewSector()
    {
        m_BluePortal.SetActive(false);
        m_OrangePortal.SetActive(false);
        m_CrossHairBlue.SetActive(false);
        m_CrossHairOrange.SetActive(false);
    }

    public void RestartGame()
    {
        m_BluePortal.SetActive(false);
        m_OrangePortal.SetActive(false);
        m_CrossHairBlue.SetActive(false);
        m_CrossHairOrange.SetActive(false);
    }
}
