using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("Profiles")]
    public WeatherProfile clearProfile;
    public WeatherProfile rainProfile;
    public WeatherProfile fogProfile;
    public WeatherProfile snowProfile;
    public WeatherProfile windyProfile;

    [Header("Runtime settings")]
    public Transform particleFollowTarget; // camera or player (particles spawned near this)
    public Transform particleParent; // parent for spawned particle systems (optional)
    public WindZone windZone; // assign one WindZone from scene (optional)
    public Volume globalVolume; // optional: URP Global Volume to swap profiles

    // internal
    GameObject currentParticleInstance;
    WeatherProfile activeProfile;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // default — clear (or whatever default you prefer)
        ApplyProfile(clearProfile);
    }

    public void ApplyProfileForName(string name)
    {
        switch (name.ToLower())
        {
            case "clear": ApplyProfile(clearProfile); break;
            case "rain": ApplyProfile(rainProfile); break;
            case "fog": ApplyProfile(fogProfile); break;
            case "snow": ApplyProfile(snowProfile); break;
            case "windy": ApplyProfile(windyProfile); break;
            default: Debug.LogWarning("[WeatherManager] Unknown profile name: " + name); break;
        }
    }

    public void ApplyProfile(WeatherProfile profile)
    {
        if (profile == null)
        {
            Debug.LogWarning("[WeatherManager] profile is null");
            return;
        }

        activeProfile = profile;

        // Ambient / skybox
        RenderSettings.ambientLight = profile.ambientColor;
        if (profile.skyboxMaterial != null)
        {
            RenderSettings.skybox = profile.skyboxMaterial;
            // If you want to update environment lighting
            DynamicGI.UpdateEnvironment();
        }

        // Fog
        RenderSettings.fog = profile.enableFog;
        RenderSettings.fogColor = profile.fogColor;

        // We'll use exponential fog density: (RenderSettings.fogDensity)
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = profile.fogDensity;

        // Particle systems (rain / snow)
        SetParticleInstance(profile.particlePrefab, profile.particleIntensity);

        // Wind
        ConfigureWind(profile);

        // Volume (URP) - optional: swap global profile
        if (globalVolume != null)
        {
            if (profile.volumeProfile != null)
            {
                globalVolume.profile = profile.volumeProfile;
                globalVolume.gameObject.SetActive(true);
            }
            else
            {
                // If profile has none, optionally disable volume or keep previous
                // I'll just keep the volume but clear profile
                globalVolume.profile = null;
            }
        }

        Debug.Log($"[WeatherManager] Applied weather: {profile.profileName}");
    }

    void SetParticleInstance(GameObject prefab, float intensity)
    {
        // destroy existing
        if (currentParticleInstance != null)
        {
            Destroy(currentParticleInstance);
            currentParticleInstance = null;
        }

        if (prefab == null) return;

        // spawn near the follow target (if set) else at origin
        Vector3 spawnPos = Vector3.zero;
        if (particleFollowTarget != null) spawnPos = particleFollowTarget.position;
        currentParticleInstance = Instantiate(prefab, spawnPos, Quaternion.identity, particleParent);

        // try to adapt emission rate / size by intensity
        var ps = currentParticleInstance.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var emission = ps.emission;
            // multiply each burst / rate by intensity
            // try to set rateOverTime (if present)
            var rate = emission.rateOverTime;
            if (rate.constant > 0f)
            {
                float baseRate = rate.constant;
                rate = new ParticleSystem.MinMaxCurve(baseRate * Mathf.Max(0.0001f, intensity));
                emission.rateOverTime = rate;
            }

            // scale start size
            var main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(main.startSize.constant * Mathf.Max(0.01f, intensity));

            // If prefabs have multiple particle systems as children, scale them too
            foreach (var child in currentParticleInstance.GetComponentsInChildren<ParticleSystem>())
            {
                var m = child.main;
                m.startSize = new ParticleSystem.MinMaxCurve(m.startSize.constant * Mathf.Max(0.01f, intensity));
                var e = child.emission;
                var r = e.rateOverTime;
                if (r.constant > 0f) e.rateOverTime = new ParticleSystem.MinMaxCurve(r.constant * Mathf.Max(0.0001f, intensity));
            }
        }
    }

    void ConfigureWind(WeatherProfile profile)
    {
        if (windZone == null)
        {
            // optional: auto-find a WindZone in scene
            windZone = FindObjectOfType<WindZone>();
        }

        if (windZone == null)
        {
            // nothing to do
            return;
        }

        windZone.gameObject.SetActive(profile.enableWind);
        if (!profile.enableWind) return;

        windZone.transform.rotation = Quaternion.LookRotation(profile.windDirection.normalized, Vector3.up);
        windZone.windMain = profile.windMain;
        windZone.windTurbulence = profile.windTurbulence;
        windZone.windPulseMagnitude = profile.windPulseMagnitude;
    }

    void LateUpdate()
    {
        // keep particles following the camera/player horizontally (so rain appears centered)
        if (currentParticleInstance != null && particleFollowTarget != null)
        {
            Vector3 p = particleFollowTarget.position;
            // keep Y from particle instance to avoid vertical jitter
            currentParticleInstance.transform.position = new Vector3(p.x, currentParticleInstance.transform.position.y, p.z);
        }
    }

    // convenience methods
    public void SetClear() => ApplyProfile(clearProfile);
    public void SetRain() => ApplyProfile(rainProfile);
    public void SetFog() => ApplyProfile(fogProfile);
    public void SetSnow() => ApplyProfile(snowProfile);
    public void SetWindy() => ApplyProfile(windyProfile);
}
