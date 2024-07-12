using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : MonoBehaviour
{
    private Gamepad gamePad;
    private Coroutine stopRumble;
    private Coroutine rumbleTwo;
    private PlayerInput playerInput;

    public void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public void Rumble(float leftFreq, float rightFreq, float duration)
    {
        gamePad = playerInput.GetDevice<Gamepad>();

        if (gamePad != null)
        {
            gamePad.SetMotorSpeeds(leftFreq, rightFreq);

            stopRumble = StartCoroutine(StopRumble(duration, gamePad));
        }
    }

    private IEnumerator StopRumble(float dur, Gamepad pad)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        pad.SetMotorSpeeds(0, 0);
    }

 
}
