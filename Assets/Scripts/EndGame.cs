using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    [SerializeField] private Animator m_ElevatorAnimator;
    [SerializeField] private Animator m_FadeAnimator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && SceneManager.GetActiveScene().name == "Level01")
            StartCoroutine(GoToNextScene());
    }

    private IEnumerator GoToNextScene()
    {
        m_ElevatorAnimator.SetTrigger("OpenElevator");
        yield return new WaitForSeconds(4.0f);
        m_ElevatorAnimator.SetTrigger("CloseElevator");
        yield return new WaitForSeconds(3.0f);
        GameManager.instance.ReStartGame(true);
        /*m_FadeAnimator.SetTrigger("FadeStarting");
        yield return new WaitForSeconds(m_FadeAnimationClip.length);
        SceneManager.LoadSceneAsync("Level02");*/
    }
}
