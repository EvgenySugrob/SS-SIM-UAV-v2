using System.Collections;
using System.Collections.Generic;
using UniStorm;
using UnityEngine;
using UnityEngine.Rendering;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("WeatherType")]
    [SerializeField] List<WeatherType> weatherType;

    [SerializeField]private WeatherType selectedWeather;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void SelectedWeather(int weatherIndex)
    {
        selectedWeather = weatherType[weatherIndex];
    }

    public void TransitWeather()
    {
        UniStormManager.Instance.ChangeWeatherWithTransition(selectedWeather);
    }
    
}
