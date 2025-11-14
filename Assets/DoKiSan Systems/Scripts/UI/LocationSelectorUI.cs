// Assets/Scripts/Location/LocationSelectorUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic; // если используешь TMP

public class LocationSelectorUI : MonoBehaviour
{
    [Header("CameraPosition")]
    [SerializeField] List<Transform> cameraLocationPositions;
    [SerializeField] Transform cameraTransform;

    [Header("UI (assign)")]
    public Button campButton;
    public Button factoryButton;
    public Button cityButton;
    public Button confirmButton;


    [Header("Scene")]
    public string mainSceneName = "MainScene"; // укажи имя основной сцены

    private void Start()
    {
        if (campButton != null) campButton.onClick.AddListener(() => Select(LocationSelectionService.Location.Camp));
        if (factoryButton != null) factoryButton.onClick.AddListener(() => Select(LocationSelectionService.Location.Factory));
        if (cityButton != null) cityButton.onClick.AddListener(() => Select(LocationSelectionService.Location.City));
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmAndLoad);

        UpdatePosition();
    }

    public void Select(LocationSelectionService.Location loc)
    {
        LocationSelectionService.SetSelected(loc);
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        LocationSelectionService.Location location = LocationSelectionService.GetSelectedOrDefault();

        switch (location)
        {
            case LocationSelectionService.Location.Camp:
                cameraTransform.parent = cameraLocationPositions[0];
                cameraTransform.localPosition = Vector3.zero;
            break;
            case LocationSelectionService.Location.Factory:
            cameraTransform.parent = cameraLocationPositions[1];
                cameraTransform.localPosition = Vector3.zero;
            break;
            case LocationSelectionService.Location.City:
            cameraTransform.parent = cameraLocationPositions[2];
                cameraTransform.localPosition = Vector3.zero;
            break;
        }
    }

    public void ConfirmAndLoad()
    {
        if (!LocationSelectionService.HasSelection)
        {
            // если не выбрано — ставим дефолт
            LocationSelectionService.SetSelected(LocationSelectionService.Location.Camp);
        }

        if (string.IsNullOrEmpty(mainSceneName))
        {
            Debug.LogError("[LocationSelectorUI] mainSceneName not set.");
            return;
        }

        SceneManager.LoadScene(mainSceneName);
    }
}
