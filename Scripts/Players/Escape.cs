using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Escape : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            SceneManager.LoadScene("CharacterSelect");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("CharacterSelect 1");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
