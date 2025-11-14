using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniStorm;

public class RanomWeatherInMenu : MonoBehaviour
{
    [SerializeField] float timeWait = 12f;
    void Start()
    {
        StartCoroutine(ChangeWheather());
    }

    private IEnumerator ChangeWheather()
    {
        yield return new WaitForSeconds(timeWait);

        UniStormManager.Instance.RandomWeather();
        StartCoroutine(ChangeWheather());
    }

}
