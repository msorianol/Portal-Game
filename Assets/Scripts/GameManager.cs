using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IRestartGame
{
    void RestartGame();
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private GameObject m_DeathUI;
    [SerializeField] private GameObject m_GameUI;
    [SerializeField] private CharacterController m_CharacterController;
    [SerializeField] private Player_Controller m_PlayerController;
    [SerializeField] private PortalWeaponController m_PortalWeaponController;
    [SerializeField] private PlayerLifeController m_PlayerLifeController;
    [SerializeField] private TMP_Text m_WinDeathText;
    [SerializeField] private TMP_Text m_NewGameButtonText;
    [SerializeField] private FadeController m_FadeController;

    public List<IRestartGame> m_RestartGame = new List<IRestartGame>();

    private Animator m_PortalWeaponAnimator;

    public bool m_Restart;
    private bool m_GameHasEnded = false;
    private bool m_CoroutineStarted = false;
    private bool m_PlayerBeingDamagedThisFrame = false;
    private float m_TotalDamagePerSecond = 0f;

    private void Awake()
    {
        instance = this;
        m_PortalWeaponAnimator = m_PortalWeaponController.GetComponentInChildren<Animator>();
        m_PortalWeaponAnimator.enabled = true;
    }

    private void Update()
    {
        StartCoroutine(CheckPlayerHitStatus());
    }

    public void ReportPlayerDamaged(float l_DamagePerSecond)
    {
        m_PlayerBeingDamagedThisFrame = true;
        m_TotalDamagePerSecond += l_DamagePerSecond;
    }

    private IEnumerator CheckPlayerHitStatus()
    {
        yield return new WaitForEndOfFrame();

        if (m_PlayerBeingDamagedThisFrame)
        {
            Turret.OnPlayerDamagedByLaser?.Invoke(m_TotalDamagePerSecond);
        }
        else
        {
            Turret.OnPlayerNotDamagedByLaser?.Invoke();
            m_PlayerLifeController.m_HitSoundPlayed = false;
        }

        m_PlayerBeingDamagedThisFrame = false;
        m_TotalDamagePerSecond = 0f;
    }


    public void SetPlayer(Player_Controller l_Player)
    {
        m_PlayerController = l_Player;
    }

    public Player_Controller GetPlayer()
    {
        return m_PlayerController;
    }

    public void AddRestartGame(IRestartGame l_Restart)
    {
        m_RestartGame.Add(l_Restart);
    }

    public void ReStartGame(bool l_EndGame)
    {
        if (l_EndGame)
        {
            m_PortalWeaponAnimator.enabled = false;
            m_WinDeathText.text = "You Win";
            m_NewGameButtonText.text = "Menu";
            m_GameHasEnded = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        m_CharacterController.enabled = false;
        m_PlayerController.enabled = false;
        m_PortalWeaponController.enabled = false;
        m_PortalWeaponAnimator.enabled = false;
        m_GameUI.SetActive(false);
        m_DeathUI.SetActive(true);
    }

    public void RestartPosition()
    {
        if (m_GameHasEnded == false)
        {
            m_GameUI.SetActive(true);
            m_DeathUI.SetActive(false);

            m_Restart = true;

            foreach (IRestartGame l_Controller in m_RestartGame)
            {
                l_Controller.RestartGame();
            }

            Cursor.lockState = CursorLockMode.Locked;
            StartCoroutine(PlayerActive());
        }
    }

    private IEnumerator PlayerActive()
    {
        yield return new WaitForSeconds(1f);
        m_CharacterController.enabled = true;
        m_PlayerController.enabled = true;
        m_PortalWeaponController.enabled = true;
        m_PortalWeaponAnimator.enabled = true;
        m_PlayerController.SetSpeed();
        m_Restart = false;
        m_CoroutineStarted = false;
    }

    public void NewGame()
    {
        StartCoroutine(NewGameCoroutine());
    }

    public IEnumerator NewGameCoroutine()
    {
        if (m_GameHasEnded == true)
        {
            m_FadeController.StartFade();
            yield return new WaitForSeconds(2.0f);
            m_GameHasEnded = false;
            SceneManager.LoadSceneAsync("MainMenu");
        }

        if (m_CoroutineStarted == false)
        {
            m_FadeController.StartFade();
            m_CoroutineStarted = true;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

