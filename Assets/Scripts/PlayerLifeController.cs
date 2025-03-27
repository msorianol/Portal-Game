using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLifeController : MonoBehaviour, IRestartGame
{
    [Header("Sounds")]
    [SerializeField] private AudioClip m_DeathSound;
    [SerializeField] private AudioClip m_DeadZoneSound;
    [SerializeField] private AudioClip m_PlayerHitSound;

    [SerializeField] private CanvasGroup m_BloodImage;
    private Player_Controller m_PlayerController;
    private Animator m_PlayerAnimator;

    [SerializeField] private float m_TimeToHealPlayer = 2f;
    public float m_TimeToKillPlayer;
    public float m_MaxPlayerHealth = 100;
    public float m_Health;

    private float m_DamageTimer = 0f;
    private float m_HealTimer = 0f;
    private bool m_Death = false;

    public bool m_HitSoundPlayed = false;

    private void OnEnable()
    {
        Turret.OnPlayerDamagedByLaser += ApplyLaserDamage;
        Turret.OnPlayerNotDamagedByLaser += HealOverTime;
    }

    private void OnDisable()
    {
        Turret.OnPlayerDamagedByLaser -= ApplyLaserDamage;
        Turret.OnPlayerNotDamagedByLaser -= HealOverTime;
    }

    private void Start()
    {
        GameManager.instance.AddRestartGame(this);
        m_PlayerController = GetComponent<Player_Controller>();
        m_Health = m_MaxPlayerHealth;
        m_PlayerAnimator = GetComponent<Animator>();
    }

    private void ApplyLaserDamage(float l_Damage)
    {
        if (m_Death) return;

        if (m_HitSoundPlayed == false)
        {
            SoundsManager.instance.PlaySoundClip(m_PlayerHitSound, transform, 0.2f);
            m_HitSoundPlayed = true;
        }

        m_DamageTimer += Time.deltaTime;

        float l_Progress = Mathf.Clamp01(m_DamageTimer / m_TimeToKillPlayer);
        m_Health -= l_Damage * Time.deltaTime;

        m_BloodImage.alpha = l_Progress;

        if (l_Progress >= 1f)
        {
            Death();
        }
    }

    private void HealOverTime()
    {
        if (m_Death) return;

        m_Health = Mathf.Clamp(m_Health + Time.deltaTime * 15f, 0f, 100f);

        m_BloodImage.alpha = Mathf.Clamp(m_BloodImage.alpha - Time.deltaTime * 0.6f, 0f, 1f);

        m_DamageTimer = 0f;
    }


    /*private void HealOverTime()
    {
        if (m_Death) return;

        m_Health = Mathf.Min(m_Health + Time.deltaTime * m_TimeToHealPlayer, 100f);

        if (m_BloodImage.alpha > 0f)
        {
            m_HealTimer += Time.deltaTime;
            m_BloodImage.alpha = Mathf.Lerp(m_BloodImage.alpha, 0f, m_HealTimer / m_TimeToHealPlayer);
        }
        else if (m_HealTimer >= m_TimeToHealPlayer)
        {
            m_HealTimer = 0f;
        }

        m_DamageTimer = 0f;
    }*/

    public void Death()
    {
        SoundsManager.instance.PlaySoundClip(m_DeathSound, transform, 0.2f);
        m_BloodImage.alpha = 0.0f;
        GameManager.instance.ReStartGame(false);
    }

    public void KilledByDeadZone()
    {
        SoundsManager.instance.PlaySoundClip(m_DeadZoneSound, transform, 0.2f);
        Death();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("DeadZone"))
        {
            KilledByDeadZone();
        }
    }

    public void RestartGame()
    {
        m_Death = false;
        m_Health = m_MaxPlayerHealth;
        m_DamageTimer = 0f;
        m_BloodImage.alpha = 0.0f;
    }
}
