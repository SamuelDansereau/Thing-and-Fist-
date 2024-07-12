using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeBoat : MonoBehaviour
{
    GameObject firstPlayer, secondPlayer, Car;
    [SerializeField]
    string endScene;

    public Image fade;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (firstPlayer != null && secondPlayer != null)
        {
            Color newcolor = new Color(0, 0, 0, fade.color.a + Time.deltaTime);
            fade.color = newcolor;
            if (fade.color.a >= 1)
                boatEscape();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (firstPlayer == null)
            {
                firstPlayer = other.gameObject;
            }
            else if (secondPlayer == null && other.gameObject != firstPlayer)
            {
                secondPlayer = other.gameObject;
            }
        }
    }

    void boatEscape()
    {
        SceneManager.LoadScene(endScene);   
    }
}
