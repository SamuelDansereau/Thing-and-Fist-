using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Hitstop : MonoBehaviour
{
    CameraClearFlags oldClear;
    int oldMask;
    Camera playerCam;

    void Start()
    {
        playerCam = GetComponent<Camera>();
    }

    void Update()
    {
        
    }

    public IEnumerator cameraStop(float duration)
    {
        
        StartCoroutine(FreezeCam());
        yield return new WaitForSeconds(duration);
        StartCoroutine(UnfreezeCam());
    }

    IEnumerator FreezeCam()
    {
        oldClear = playerCam.clearFlags;
        oldMask = playerCam.cullingMask;
	    playerCam.clearFlags = CameraClearFlags.Nothing;
	    yield return null;
	    playerCam.cullingMask = 0;
        ScreenCapture.CaptureScreenshot("Hit_Frame.png");
    }

    IEnumerator UnfreezeCam()
    {
	    playerCam.clearFlags = oldClear;
	    yield return null;
	    playerCam.cullingMask = oldMask;
    }
}
