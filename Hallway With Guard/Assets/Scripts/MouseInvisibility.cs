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
            ui.interactable = false;
            ui.blocksRaycasts = false;
        }

        yield return new WaitForSeconds(duration);

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