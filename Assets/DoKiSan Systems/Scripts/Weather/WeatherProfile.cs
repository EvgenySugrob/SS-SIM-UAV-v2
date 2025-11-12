using UnityEngine;

[CreateAssetMenu(menuName = "Weather/Weather Profile", fileName = "WeatherProfile")]
public class WeatherProfile : ScriptableObject
{
    public string profileName = "Clear";

    [Header("General")]
    public Color ambientColor = Color.white;
    public Material skyboxMaterial; // optional - assign skybox for this weather

    [Header("Fog (RenderSettings)")]
    public bool enableFog = false;
    public Color fogColor = Color.grey;
    public float fogDensity = 0.01f; // for exponential fog
    public float fogStartDistance = 0f; // if using linear fog later
    public float fogEndDistance = 300f;

    [Header("Particles")]
    public GameObject particlePrefab; // prefab with ParticleSystem (Rain / Snow) or null
    [Range(0f, 5f)] public float particleIntensity = 1f; // multiplier for emission rate / size

    [Header("Wind")]
    public bool enableWind = false;
    public Vector3 windDirection = new Vector3(1f, 0f, 0f);
    public float windMain = 0.5f; // WindZone main
    public float windTurbulence = 0.1f;
    public float windPulseMagnitude = 0.1f;

    [Header("Other")]
    [Tooltip("Optional post-processing Volume profile to enable/adjust for this weather (optional).")]
    public UnityEngine.Rendering.VolumeProfile volumeProfile;
}
