using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CompanionSpawner : MonoBehaviour
{
    public enum SpawnerType
    {
        Normal,
        Impulse
    }

    public SpawnerType m_SpawnerType;

    [SerializeField] private GameObject m_CompanionPrefab;
    [SerializeField] private Transform m_CompanionSpawnPoint;
    [SerializeField] private Animator m_CompanionSpawnerAnimatior;
    [SerializeField] private TMP_Text m_ButtonText;

    private bool m_PlayerInTrigger;
    private bool m_TextIsShowing;
    public bool m_CubeAlreadyExists;


    private void OnEnable()
    {
        CompanionController.OnCubeDestroyed += CompanionCubeDestroyed;
    }

    private void OnDisable()
    {
        CompanionController.OnCubeDestroyed -= CompanionCubeDestroyed;
    }

    private void Update()
    {
        if (m_PlayerInTrigger && Input.GetKeyDown(KeyCode.E) && m_CubeAlreadyExists == false)
            StartCoroutine(Spawn());
    }

    private void ShowText()
    {
        if (m_PlayerInTrigger == true && m_TextIsShowing == false)
            StartCoroutine(ShowTextCoroutine());
    }

    private IEnumerator ShowTextCoroutine()
    {
        m_TextIsShowing = true;
        m_ButtonText.enabled = true;
        m_ButtonText.text = "Press E to spawn a cube";
        yield return new WaitForSeconds(2.0f);
        m_ButtonText.enabled = false;
        m_TextIsShowing = false;
    }

    private IEnumerator Spawn()
    {
        m_CubeAlreadyExists = true;
        GameObject m_Companion = Instantiate(m_CompanionPrefab);
        m_Companion.SetActive(true);

        if (m_SpawnerType == SpawnerType.Impulse)
            m_Companion.GetComponent<Rigidbody>().AddForce(Vector3.forward * 5.0f, ForceMode.VelocityChange);

        m_Companion.transform.position = m_CompanionSpawnPoint.transform.position;
        m_Companion.transform.rotation = m_CompanionSpawnPoint.transform.rotation;

        m_CompanionSpawnerAnimatior.SetBool("SpawnerButtonClicked", true);
        yield return null;

        AnimatorClipInfo[] l_CurrentClipInfo = m_CompanionSpawnerAnimatior.GetCurrentAnimatorClipInfo(0);
        float l_CurrentClipLength = l_CurrentClipInfo[0].clip.length;

        yield return new WaitForSeconds(l_CurrentClipLength);
        m_CompanionSpawnerAnimatior.SetBool("SpawnerButtonClicked", false);
    }

    private void CompanionCubeDestroyed()
    {
        m_CubeAlreadyExists = false;
    }

    public void PlaySound(AudioClip l_AudioClip)
    {
        SoundsManager.instance.PlaySoundClip(l_AudioClip, transform, 0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_PlayerInTrigger = true;
            ShowText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_PlayerInTrigger = false;
            m_ButtonText.enabled = false;
            m_TextIsShowing = false;
        }
    }
}
