using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDrop : MonoBehaviour
{
    [SerializeField] Rigidbody rbBomb;
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] GameObject explosion;

    public void DropBomb()
    {
        transform.parent = null;

        capsuleCollider.isTrigger = false;
        rbBomb.isKinematic = false;
    }

    void OnCollisionEnter(Collision collision)
    {      
        if(collision!=null)
        {
            GameObject objectCollision = collision.gameObject;

            if(objectCollision.layer == 0)
            {
                explosion.SetActive(true);
            }
        }
    }
}
