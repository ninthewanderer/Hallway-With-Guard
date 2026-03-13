using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    public float fadeDuration = 1f;
    public GameObject player;
    public CanvasGroup fadeCanvasGroup;
    public string nextSceneName;
    public Image spottedEyes;

    [Header("Caught Audio")]
    public AudioSource audioSource;
    public AudioClip caughtSound;

    private CharacterController playerController;

    bool m_IsPlayerAtExit;
    float m_Timer;
    bool hasPlayedCaughtSound;

    // A private reference to the CatBehavior script, needed to tell the script when the game is over.
    private CatBehavior gameOverState;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            if (!hasPlayedCaughtSound && audioSource != null && caughtSound != null)
            {
                audioSource.PlayOneShot(caughtSound);
                hasPlayedCaughtSound = true;
            }

            gameOverState.gameOver = true;
            playerController.enabled = false;
            m_IsPlayerAtExit = true;
        }
    }

    void Start()
    {
        gameOverState = FindFirstObjectByType<CatBehavior>();
        playerController = player.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (m_IsPlayerAtExit)
        {
            EndLevel();
        }
    }

    void EndLevel()
    {
        m_Timer += Time.deltaTime;

        gameOverState.gameOver = true;
        playerController.enabled = false;
        spottedEyes.enabled = false;
        fadeCanvasGroup.alpha = m_Timer / fadeDuration;

        if (m_Timer >= fadeDuration)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}