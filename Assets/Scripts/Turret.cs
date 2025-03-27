using System;
using System.Collections;
using UnityEngine;

public class Turret : TeleportableObjects, IRestartGame
{
    [SerializeField] private float m_MaxAngleLaserAlive = 10.0f;

    private Vector3 m_StartPosition;
    private Quaternion m_StartRotation;

    public LineRenderer m_LaserRenderer;
    public float m_MaxDistance = 50.0f;
    private bool m_IsDying = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_TurretDeathSound;
    [SerializeField] private AudioClip m_ExplosionSound;

    [Header("Particles")]
    [SerializeField] private ParticleSystem m_TurretExplosionParticles;

    public static Action<float> OnPlayerDamagedByLaser;
    public static Action OnPlayerNotDamagedByLaser;

    public override void Start()
    {
        base.Start();
        GameManager.instance.AddRestartGame(this);
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
    }

    public override void Update()
    {
        base.Update();

        if (IsLaserAlive())
        {
            Ray l_Ray = new Ray(m_LaserRenderer.transform.position, m_LaserRenderer.transform.forward);

            if (Physics.Raycast(l_Ray, out RaycastHit l_HitInfo, m_MaxDistance, m_LayerMask.value))
            {
                m_LaserRenderer.SetPosition(1, new Vector3(0, 0, l_HitInfo.distance));
                m_LaserRenderer.gameObject.SetActive(true);

                if (l_HitInfo.collider.CompareTag("RefractionCube"))
                {
                    l_HitInfo.collider.GetComponent<RefractionCube>().CreateRefraction();
                }
                else if (l_HitInfo.collider.CompareTag("Turret"))
                {
                    //Animacion
                    if (m_Portal != null)
                    {
                        m_Portal.m_LaserEnabled = false;
                    }

                    Turret l_Turret = l_HitInfo.collider.GetComponent<Turret>();
                    StartCoroutine(l_Turret.TurretDeathCoroutine(l_Turret, l_HitInfo.collider.gameObject));
                }
                else if (l_HitInfo.collider.CompareTag("Portal"))
                {
                    m_Portal = l_HitInfo.collider.GetComponent<Portal>();
                    m_LaserRenderer.SetPosition(1, new Vector3(0, 0, l_HitInfo.distance + Vector3.Distance(l_HitInfo.point, m_Portal.transform.position) + 0.5f));
                    m_Portal.RayReflection(l_Ray, l_HitInfo);
                }
                else if (l_HitInfo.collider.CompareTag("Player") && l_HitInfo.collider.TryGetComponent(out PlayerLifeController l_PlayerLifeController))
                {
                    float l_LaserDuration = l_PlayerLifeController.m_TimeToKillPlayer;
                    float l_PlayerHealth = l_PlayerLifeController.m_MaxPlayerHealth;
                    float l_DamagePerSecond = l_PlayerHealth / l_LaserDuration;
                    GameManager.instance.ReportPlayerDamaged(l_DamagePerSecond);
                }
            }
        }
        else
            m_LaserRenderer.gameObject.SetActive(false);
    }

    private bool IsLaserAlive()
    {
        return Vector3.Dot(transform.up, Vector3.up) > Mathf.Cos(m_MaxAngleLaserAlive * Mathf.Deg2Rad);
    }

    public IEnumerator TurretDeathCoroutine(Turret l_Turret, GameObject l_TurretObject)
    {
        if (m_IsDying) yield break;
        m_IsDying = true;

        l_Turret.m_TurretExplosionParticles.Play();
        SoundsManager.instance.PlaySoundClip(m_TurretDeathSound, transform, 0.2f);
        SoundsManager.instance.PlaySoundClip(m_ExplosionSound, transform, 0.2f);
        yield return new WaitForSeconds(0.2f);

        l_TurretObject.SetActive(false);
        m_IsDying = false;
    }

    public void RestartGame()
    {
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        gameObject.SetActive(true);
    }
}
