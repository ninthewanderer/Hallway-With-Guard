using UnityEngine;

public class InvisibilityPowerUp : MonoBehaviour
{
    [Header("Power-up Settings")]
    [SerializeField] private float duration = 10f;

    [Header("UI")]
    [SerializeField] private CanvasGroup activeUI;

    [Header("Mouse Detection")]
    [SerializeField] private string mouseTag = "Player";
    [SerializeField] private int invisibleLayer = 0; // set this in Inspector

    private void Awake()
    {
        SetUIVisible(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(mouseTag))
            return;

        MouseInvisibility mouseInvisibility = other.GetComponentInParent<MouseInvisibility>();
        if (mouseInvisibility == null)
            return;

        mouseInvisibility.Activate(duration, activeUI, invisibleLayer);

        // Remove the pickup after use
        Destroy(gameObject);
    }

    private void SetUIVisible(bool visible)
    {
        if (activeUI == null) return;

        activeUI.gameObject.SetActive(visible);
        activeUI.alpha = visible ? 1f : 0f;
        activeUI.interactable = false;
        activeUI.blocksRaycasts = false;
    }
}