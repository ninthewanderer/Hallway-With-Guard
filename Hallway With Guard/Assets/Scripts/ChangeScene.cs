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
    public Text spottedText;

    bool m_IsPlayerAtExit;
    float m_Timer;

    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject == player)
        {
            m_IsPlayerAtExit = true;
        }
    }

    void Update ()
    {
        if(m_IsPlayerAtExit)
        {
            EndLevel();
        }
    }

    void EndLevel ()
    {
        m_Timer += Time.deltaTime;

        spottedText.enabled = false;
        fadeCanvasGroup.alpha = m_Timer / fadeDuration;

        if(m_Timer >= fadeDuration)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}