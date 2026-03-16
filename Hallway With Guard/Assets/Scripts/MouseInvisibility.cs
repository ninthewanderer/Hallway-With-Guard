using System.Collections;
using UnityEngine;

public class MouseInvisibility : MonoBehaviour
{
    [Header("Optional: assign the root object you want to swap layers on")]
    [SerializeField] private GameObject objectToHide;

    private int originalLayer;
    private Coroutine invisibilityRoutine;
    public bool IsInvisible { get; private set; }

    private void Awake()
    {
        if (objectToHide == null)
            objectToHide = gameObject;

        originalLayer = objectToHide.layer;
    }

    public void Activate(float duration, CanvasGroup ui, int invisibleLayer)
    {
        if (invisibilityRoutine != null)
            StopCoroutine(invisibilityRoutine);

        invisibilityRoutine = StartCoroutine(InvisibilityTimer(duration, ui, invisibleLayer));
    }

    private IEnumerator InvisibilityTimer(float duration, CanvasGroup ui, int invisibleLayer)
{
    IsInvisible = true;

    SetLayerRecursively(objectToHide, invisibleLayer);

    if (ui != null)
    {
        ui.gameObject.SetActive(true);
        ui.alpha = 1f;
    }

    float blinkStart = 3f; // seconds before end to start blinking
    float timeRemaining = duration;

    while (timeRemaining > 0)
    {
        if (ui != null && timeRemaining <= blinkStart)
        {
            // blink effect
            ui.alpha = ui.alpha == 1f ? 0.25f : 1f;
        }

        yield return new WaitForSeconds(0.3f);
        timeRemaining -= 0.3f;
    }

    // restore everything
    SetLayerRecursively(objectToHide, originalLayer);

    if (ui != null)
    {
        ui.alpha = 0f;
        ui.gameObject.SetActive(false);
    }

    IsInvisible = false;
    invisibilityRoutine = null;
}

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}