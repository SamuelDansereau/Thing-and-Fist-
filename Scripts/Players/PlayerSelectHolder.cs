using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.UI;

public class PlayerSelectHolder : MonoBehaviour
{
    private int characterID = 3, playerId;
    public GameObject player1;
    public GameObject player2;
    public GameObject Ready1, Ready2, events;
    public bool ready = false;
    private PlayerInput pi;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PlayerHolder.Instance.AddPlayer(gameObject);
        pi = GetComponent<PlayerInput>();
        //PlayerHolder.Instance.addDevice(pi);
        List<GameObject> players = PlayerHolder.Instance.GetPlayers();
        for (int i = 0; i < PlayerHolder.Instance.GetPlayerSize(); i++)
        {
            if (players[i] == gameObject)
            {
                playerId = i;
                break;
            }
        }
        if (playerId == 0)
        {
            player1.SetActive(true);
            events = player1.GetComponent<MultiplayerEventSystem>().gameObject;
            
        }

        if (playerId == 1)
        {
            player2.SetActive(true);
            events = player2.GetComponent<MultiplayerEventSystem>().gameObject;
        }
        pi.uiInputModule = events.GetComponent<InputSystemUIInputModule>();

    }

    public void SetCharcterId(int num)
    {
        characterID = num;
        if (playerId == 0)
            Ready1.SetActive(true);
        if (playerId == 1)
            Ready2.SetActive(true);
    }

    public int GetCharcterId()
    {
        return characterID;
    }

    public void Deactivate()
    {
        //player1.SetActive(false);
        //player2.SetActive(false);
        //GetComponent<Camera>().enabled = false;
    }

    public void setReady(bool val)
    {
            ready = val;
            PlayerHolder.Instance.EditPlayerCharacterId(playerId,characterID);
 
    }

    public bool getReady()
    {
        return ready;
    }
}
