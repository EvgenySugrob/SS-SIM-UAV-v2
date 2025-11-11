using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScenarioConfigUI : MonoBehaviour
{
    [Header("Controls")]
    public Slider totalDronesSlider;            // 1..10
    public TMP_Dropdown droneTypeDropdown;      // 0=Copter,1=Wing,2=Both
    public Slider copterCountSlider;            // visible only when Both
    public Slider wingCountSlider;              // visible only when Both
    public TMP_Dropdown behaviorDropdown;       // 0=Recon,1=Attack,2=Mixed

    public Button applyButton;

    [Header("Manager reference")]
    public ScenarioManager scenarioManager;

    private void Start()
    {
        if (applyButton != null) applyButton.onClick.AddListener(OnApplyClicked);
        if (droneTypeDropdown != null) droneTypeDropdown.onValueChanged.AddListener(_ => RefreshUI());
        if (totalDronesSlider != null) totalDronesSlider.onValueChanged.AddListener(_ => SyncSliders());
        if (copterCountSlider != null) copterCountSlider.onValueChanged.AddListener(_ => SyncSliders(fromCopter:true));
        if (wingCountSlider != null) wingCountSlider.onValueChanged.AddListener(_ => SyncSliders(fromCopter:false));

        // initialize constraints
        if (totalDronesSlider != null)
        {
            totalDronesSlider.minValue = 1;
            totalDronesSlider.maxValue = 10;
            if (totalDronesSlider.value < 1) totalDronesSlider.value = 1;
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (applyButton != null) applyButton.onClick.RemoveListener(OnApplyClicked);
    }

    private void RefreshUI()
    {
        bool both = droneTypeDropdown != null && droneTypeDropdown.value == 2;
        if (copterCountSlider != null) copterCountSlider.gameObject.SetActive(both);
        if (wingCountSlider != null) wingCountSlider.gameObject.SetActive(both);

        // enforce min/max for both-sliders
        if (copterCountSlider != null)
        {
            copterCountSlider.minValue = 1;
            copterCountSlider.maxValue = Mathf.Max(1, Mathf.RoundToInt(totalDronesSlider.value) - 1);
            if (copterCountSlider.value < copterCountSlider.minValue) copterCountSlider.value = copterCountSlider.minValue;
        }
        if (wingCountSlider != null)
        {
            wingCountSlider.minValue = 1;
            wingCountSlider.maxValue = Mathf.Max(1, Mathf.RoundToInt(totalDronesSlider.value) - 1);
            if (wingCountSlider.value < wingCountSlider.minValue) wingCountSlider.value = wingCountSlider.minValue;
        }

        SyncSliders();
    }

    private void SyncSliders(bool fromCopter = true)
    {
        if (droneTypeDropdown == null || totalDronesSlider == null) return;
        bool both = droneTypeDropdown.value == 2;
        int total = Mathf.RoundToInt(totalDronesSlider.value);

        if (!both)
        {
            if (copterCountSlider != null) copterCountSlider.SetValueWithoutNotify(total);
            if (wingCountSlider != null) wingCountSlider.SetValueWithoutNotify(total);
            return;
        }

        int copter = copterCountSlider != null ? Mathf.RoundToInt(copterCountSlider.value) : 1;
        int wing = wingCountSlider != null ? Mathf.RoundToInt(wingCountSlider.value) : 1;

        if (fromCopter)
        {
            copter = Mathf.Clamp(copter, 1, Mathf.Max(1, total - 1));
            wing = total - copter;
            if (wing < 1) { wing = 1; copter = total - wing; }
        }
        else
        {
            wing = Mathf.Clamp(wing, 1, Mathf.Max(1, total - 1));
            copter = total - wing;
            if (copter < 1) { copter = 1; wing = total - copter; }
        }

        if (copterCountSlider != null) copterCountSlider.SetValueWithoutNotify(copter);
        if (wingCountSlider != null) wingCountSlider.SetValueWithoutNotify(wing);
    }

    private void OnApplyClicked()
    {
        if (scenarioManager == null)
        {
            Debug.LogWarning("[ScenarioConfigUI] ScenarioManager not assigned.");
            return;
        }

        int total = totalDronesSlider != null ? Mathf.RoundToInt(totalDronesSlider.value) : 0;
        int copterCount = 0;
        int wingCount = 0;

        int dtype = droneTypeDropdown != null ? droneTypeDropdown.value : 0;
        int beh = behaviorDropdown != null ? behaviorDropdown.value : 0;

        if (dtype == 2)
        {
            copterCount = copterCountSlider != null ? Mathf.RoundToInt(copterCountSlider.value) : 1;
            wingCount = wingCountSlider != null ? Mathf.RoundToInt(wingCountSlider.value) : (total - copterCount);
        }
        else if (dtype == 0)
        {
            copterCount = total;
            wingCount = 0;
        }
        else
        {
            copterCount = 0;
            wingCount = total;
        }

        scenarioManager.ApplySettings(total, copterCount, wingCount, (ScenarioManager.BehaviorMode)beh);
    }
}
