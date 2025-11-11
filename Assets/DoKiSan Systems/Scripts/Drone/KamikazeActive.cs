using System.Collections;
using System.Collections.Generic;
using Micosmo.SensorToolkit;
using UnityEngine;

public class KamikazeActive : MonoBehaviour
{
    [SerializeField] Rigidbody rbDrone;
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] SteeringSensor steeringSensor;
    [SerializeField] GameObject explosion;
    [SerializeField] float secondToDestroy = 1f;

    private Coroutine waitDestroy;

    void Start()
    {
        rbDrone = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        steeringSensor = GetComponent<SteeringSensor>();
    }

    public void Kamikaze()
    {
        steeringSensor.enabled = false;

        rbDrone.useGravity = true;
        boxCollider.isTrigger = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            GameObject objectCollision = collision.gameObject;

            if (objectCollision.layer == 0)
            {
                explosion.SetActive(true);
                
                if(waitDestroy==null)
                    waitDestroy = StartCoroutine(DestroyDrone());
            }
        }
    }
    
    IEnumerator DestroyDrone()
    {
        yield return new WaitForSeconds(secondToDestroy);
        Destroy(gameObject);
    }
}
