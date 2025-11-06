using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ContextMenuInstance : MonoBehaviour
{
    [Header("Links inside prefab")]
    [SerializeField] private RectTransform basePanel;   // фон/контейнер, куда будут добавлены кнопки (RectTransform)
    [Tooltip("Prefab (GameObject) должен содержать Button и TMP_Text в дочерних элементах")]
    [SerializeField] private GameObject buttonPrefab;   // ActionButton prefab (root GameObject)
    [SerializeField] private Button closeButton;        // кнопка закрытия меню
    [SerializeField] private TMP_Text titleText;        // заголовок имени объекта (опционально)

    private Canvas parentCanvas;
    private ContextMenuManager manager;
    private FirstPersonController fpsController;

    private readonly List<(Button btn, ContextMenuItem item, TMP_Text txt)> activeButtons
        = new List<(Button, ContextMenuItem, TMP_Text)>();

    /// <summary>Инициализация — вызывается ContextMenuManager сразу после Instantiate.</summary>
    public void Initialize(InteractableObject target, Vector2 screenPos, Canvas canvas, ContextMenuManager ctxManager, FirstPersonController fps)
    {
        parentCanvas = canvas;
        manager = ctxManager;
        fpsController = fps;

        BuildButtons(target);

        // подписываем кнопку Close
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        // показать и позиционировать
        gameObject.SetActive(true);
        PositionAtScreenPoint(screenPos);
        
        // блокируем ввод игрока и показываем курсор
        if (fpsController != null) fpsController.SetInputEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        ClearButtons();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
    }

    private void BuildButtons(InteractableObject target)
    {
        ClearButtons();

        if (titleText != null) titleText.text = target != null ? target.GetDisplayName() : "";

        var items = target.GetContextMenuItems();
        if (items == null) return;

        foreach (var item in items)
        {
            GameObject go = Instantiate(buttonPrefab, basePanel);
            // ищем Button и TMP_Text внутри
            Button btn = go.GetComponentInChildren<Button>(true);
            TMP_Text txt = go.GetComponentInChildren<TMP_Text>(true);

            if (btn == null || txt == null)
            {
                Debug.LogWarning("[ContextMenuInstance] ActionButton prefab должен содержать Button и TMP_Text внутри.");
                Destroy(go);
                continue;
            }

            txt.text = item.GetLabel();

            // замыкание локальной копии
            ContextMenuItem localItem = item;
            btn.onClick.AddListener(() =>
            {
                try { localItem.Action?.Invoke(); }
                catch (System.Exception ex) { Debug.LogException(ex); }

                // если действие указано как закрывать меню — делаем это; иначе оставляем меню открытым
                if (localItem.CloseOnClick)
                {
                    manager?.Hide();
                }
                else
                {
                    // Если метка динамическая — обновляем вид
                    RefreshDynamicLabels();
                }
            });

            activeButtons.Add((btn, item, txt));
        }

        // Пересчитать layout сразу
        LayoutRebuilder.ForceRebuildLayoutImmediate(basePanel);
    }

    private void RefreshDynamicLabels()
    {
        foreach (var e in activeButtons)
        {
            if (e.item.DynamicLabel != null)
                e.txt.text = e.item.GetLabel();
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(basePanel);
    }

    private void ClearButtons()
    {
        foreach (Transform t in basePanel)
            Destroy(t.gameObject);

        activeButtons.Clear();
    }

    private void OnCloseButtonClicked()
    {
        manager?.Hide();
        // разблокируем ввод и спрячем курсор — ContextMenuManager.Hide удалит этот instance, но на всякий случай:
        if (fpsController != null) fpsController.SetInputEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Размещает меню внутри Canvas по экранной точке screenPos (пиксели).
    /// По умолчанию вызывается для центрирования: передай center = Screen.width/2, Screen.height/2.
    /// </summary>
    private void PositionAtScreenPoint(Vector2 screenPos)
    {
        if (parentCanvas == null) return;

        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        RectTransform rootRt = GetComponent<RectTransform>();

        Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPos, cam, out Vector3 worldPos))
        {
            rootRt.position = worldPos;
        }
        else
        {
            // fallback: поместить в центр
            rootRt.position = canvasRect.position;
        }

        // исправляем выходы за границы экрана
        basePanel.GetWorldCorners(s_corners);
        Vector3 offset = Vector3.zero;

        if (s_corners[2].x > Screen.width) offset.x = Screen.width - s_corners[2].x;
        if (s_corners[0].x < 0) offset.x = -s_corners[0].x;
        if (s_corners[0].y < 0) offset.y = -s_corners[0].y;
        if (s_corners[1].y > Screen.height) offset.y = Screen.height - s_corners[1].y;

        rootRt.position += offset;

        LayoutRebuilder.ForceRebuildLayoutImmediate(basePanel);
    }

    public void OnForcedClose()
    {
        // восстановление ввода/курсива, если нужно
        if (fpsController != null) fpsController.SetInputEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private static readonly Vector3[] s_corners = new Vector3[4];
}
