using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class HoldToEndLevel : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float requiredHoldTime = 2f;

    public GameObject player;
    public CanvasGroup fadeCanvasGroup;
    public CanvasGroup interactPrompt;
    public string nextSceneName;

    bool m_IsPlayerAtExit;
    bool m_StartEnding;

    float m_Timer;
    float m_HoldTimer;

    PlayerInput playerInput;
    InputAction interactAction;

    void Start()
    {
        playerInput = player.GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            m_IsPlayerAtExit = true;
            interactPrompt.alpha = 1f;   // show
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            m_IsPlayerAtExit = false;
            m_HoldTimer = 0f;
            interactPrompt.alpha = 0f;   // hide
        }
    }

    void Update()
    {
        /* debug
        if (interactAction.IsPressed())
        {
            Debug.Log("holding interact");
        }
        */

        if (m_StartEnding)
        {
            EndLevel();
            return;
        }

        if (m_IsPlayerAtExit && interactAction.IsPressed())
        {
            m_HoldTimer += Time.deltaTime;

            if (m_HoldTimer >= requiredHoldTime)
            {
                m_StartEnding = true;
            }
        }
        else
        {
            m_HoldTimer = 0f;
        }
    }

    void EndLevel()
    {
        m_Timer += Time.deltaTime;

        fadeCanvasGroup.alpha = m_Timer / fadeDuration;

        if (m_Timer >= fadeDuration)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}