using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashDamage : MonoBehaviour
{
    public float damage;
    //public GameObject player;

    private void Start()
    {
        //player = GameObject.Find("Wizard(Clone)");

        //damage = player.GetComponent<WizardMovement>().getDashDamage();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other == null)
        {
            return;
        }
        else if (other.tag == "Enemy")
        {
            other.GetComponent<BasicEnemy>().doDamage(damage, Vector3.zero, 0, 0);
        }
        else
        {
            return;
        }

    }
}
