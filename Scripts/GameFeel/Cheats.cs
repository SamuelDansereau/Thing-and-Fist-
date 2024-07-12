using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheats : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddLives();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Respawn();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SkipArea();
        }
    }

    public void AddLives()
    {
        GameObject wizard = FindAnyObjectByType<WizardCombat>().gameObject;
        wizard.GetComponent<PlayerHealth>().totalLives++;
        GameObject big = FindAnyObjectByType<BigGuyMovement>().gameObject;
        big.GetComponent<PlayerHealth>().totalLives++;
    }

    public void Respawn()
    {
        GameObject wizard = FindAnyObjectByType<WizardCombat>().gameObject;
        wizard.GetComponent<PlayerHealth>().totalLives++;
        wizard.GetComponent<PlayerHealth>().ResetSpawn();
        GameObject big = FindAnyObjectByType<BigGuyMovement>().gameObject;
        big.GetComponent<PlayerHealth>().totalLives++;
        big.GetComponent<PlayerHealth>().ResetSpawn();
    }

    public void SkipArea()
    {
        Key key = GameObject.FindObjectOfType<Key>();
        if (key == null)
        {
            BasicEnemy[] allEnemies = GameObject.FindObjectsOfType<BasicEnemy>();
            foreach (BasicEnemy obj in allEnemies)
            {
                obj.doDamage(20,Vector3.zero, 0, 0);
            }
        }
    }
}
