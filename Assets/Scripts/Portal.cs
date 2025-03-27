using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private float m_OffsetCamera; 
    [SerializeField] private float m_SpeedAnimation;

    public Transform m_OtherPortalTransform;
    public Portal m_MirrorPortal;
    public Camera m_Camera;
    public float m_PortalSize;
    public bool m_PortalAnimation;
    private float m_AnimationProgress;
    public Vector3 m_StartSizeAnimation;
    private Vector3 m_StartSizePortal;

    public Collider m_WallPortaled;

    public LineRenderer m_LaserRenderer; 
    public bool m_LaserEnabled;
    public LayerMask m_LayerMask;   
    private RaycastHit m_RaycastHitLaser;
    public float m_LaserOffset;

    [Header("CloneObjects")]
    public GameObject m_Gun;
    public GameObject m_Cube;
    public GameObject m_Turret; 
    private WindowPortalController m_WindowPortalController;

    public static Action OnLaserReceived;

    private void Start()
    {
        m_StartSizePortal = transform.localScale; 
        m_StartSizeAnimation = m_StartSizePortal / 100;
        m_PortalAnimation = false;
        m_AnimationProgress = 0f;
        m_LaserRenderer.gameObject.SetActive(false);
        m_WindowPortalController = GetComponentInChildren<WindowPortalController>();
    }

    private void Update()
    {
        Camera l_CameraPlayerController =  GameManager.instance.GetPlayer().m_Camera.GetComponent<Camera>();
        Vector3 l_Position = l_CameraPlayerController.transform.position;
        Vector3 l_LocalPosition = m_OtherPortalTransform.InverseTransformPoint(l_Position);
        Vector3 l_WorldPosition = m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_Forward = l_CameraPlayerController.transform.forward;
        Vector3 l_LocalForward = m_OtherPortalTransform.InverseTransformDirection(l_Forward);
        Vector3 l_WorldForward = m_MirrorPortal.transform.TransformDirection(l_LocalForward);
        m_MirrorPortal.m_Camera.transform.position = l_WorldPosition;
        m_MirrorPortal.m_Camera.transform.forward = l_WorldForward;

        float l_DistanceToPortal = Vector3.Distance(l_WorldPosition, m_MirrorPortal.transform.position);
        float l_DistanceNearClipPlane = m_OffsetCamera + l_DistanceToPortal;
        m_MirrorPortal.m_Camera.nearClipPlane = l_DistanceNearClipPlane;
     
        if (m_PortalAnimation)
            PortalAnimation();

        m_LaserRenderer.gameObject.SetActive(m_LaserEnabled);
        m_LaserEnabled = false; 
    }

    public void RayReflection(Ray ray, RaycastHit hit)
    {
        if (m_LaserEnabled)
            return; 

        Vector3 l_LaserPosition = hit.point;
        Vector3 l_LaserLocalPosition = m_OtherPortalTransform.InverseTransformPoint(l_LaserPosition);
        Vector3 l_WorldPosition = m_MirrorPortal.transform.TransformPoint(l_LaserLocalPosition);

        Vector3 l_Forward = ray.direction.normalized;
        Vector3 l_LocalForward = m_OtherPortalTransform.InverseTransformDirection(l_Forward);
        Vector3 l_WorldForward = m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        m_LaserRenderer.transform.forward = l_WorldForward;
        m_LaserRenderer.transform.position = l_WorldPosition;
        m_LaserRenderer.transform.localScale = Vector3.one; 
        m_LaserEnabled = true;

        Ray l_Ray = new Ray(m_LaserRenderer.transform.position + m_LaserRenderer.transform.forward* m_LaserOffset, m_LaserRenderer.transform.forward);
        float m_MaxDistance = 200;

        if (Physics.Raycast(l_Ray, out RaycastHit l_HitInfo, m_MaxDistance, m_LayerMask.value))
        {
            float l_Distance = Vector3.Distance(m_LaserRenderer.transform.position, l_HitInfo.point);
            m_LaserRenderer.SetPosition(1, new Vector3(0, 0, l_Distance));

            if (l_HitInfo.collider.CompareTag("RefractionCube"))
            {
                //Reflect ray
                l_HitInfo.collider.GetComponent<RefractionCube>().CreateRefraction();
            }
            else if (l_HitInfo.collider.CompareTag("Turret"))
            {
                Turret l_Turret = l_HitInfo.collider.GetComponent<Turret>();
                StartCoroutine(l_Turret.TurretDeathCoroutine(l_Turret, l_HitInfo.collider.gameObject));
            }
            else if (l_HitInfo.collider.CompareTag("Player") && l_HitInfo.collider.TryGetComponent(out PlayerLifeController l_PlayerLifeController))
            {
                float l_LaserDuration = l_PlayerLifeController.m_TimeToKillPlayer;
                float l_PlayerHealth = l_PlayerLifeController.m_MaxPlayerHealth;
                float l_DamagePerSecond = l_PlayerHealth / l_LaserDuration;
                GameManager.instance.ReportPlayerDamaged(l_DamagePerSecond);
            }
            else if (l_HitInfo.collider.CompareTag("LaserReceiver"))
            {
                OnLaserReceived?.Invoke();
            }
        }
        else
            m_LaserRenderer.gameObject.SetActive(false);
    }

    public void PortalAnimation()
    {
        Vector3 l_Size = m_StartSizePortal * m_PortalSize;
        m_AnimationProgress += m_SpeedAnimation * Time.deltaTime;

        transform.localScale = Vector3.Lerp(m_StartSizeAnimation, l_Size, m_AnimationProgress);

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, m_StartSizePortal.z);

        if (m_AnimationProgress >= 1f)
        {
            m_PortalAnimation = false;
            m_AnimationProgress = 0f;
        }
    }

    public void CloneObject(GameObject l_Object, Portal l_Mirror)
    {
        TeleportableObjects l_TeleportableObject = l_Object.GetComponent<TeleportableObjects>();
        CloneObjectController l_Controller = null;  

        if (l_Object.CompareTag("CompanionCube"))
        {
            l_Controller = m_Cube.GetComponent<CloneObjectController>(); 
        }
        else if (l_Object.CompareTag("Turret"))
        {
            l_Controller = m_Turret.GetComponent<CloneObjectController>();
        }
        else if (l_Object.CompareTag("Weapon"))
        {
            l_Controller = m_Gun.GetComponent<CloneObjectController>(); 
        }

        m_WindowPortalController.SetCloneObject(l_Controller);
        l_Controller.TeleportObjectClone(l_TeleportableObject, m_MirrorPortal, m_OtherPortalTransform);

    }

    public void DesactiveObject(GameObject l_Object)
    {
        if (l_Object.CompareTag("CompanionCube"))
        {
            m_Cube.SetActive(false);
        }
        else if (l_Object.CompareTag("Turret"))
        {
            m_Turret.SetActive(false);
        }
        else if (l_Object.CompareTag("Weapon"))
        {
            m_Gun.SetActive(false);  
        }
    }
} 

