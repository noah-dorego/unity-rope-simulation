using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_zoom : MonoBehaviour
{
    public Camera cam;
    public float maxZoom = 5;
    public float minZoom = 20;
    public float sensitivity = 1;
    public float speed = 30;
    public bool scroll = false;
    float targetZoom;
    float newSize;

    void Update()
    {
        if (scroll == true)
        {
            targetZoom -= Input.mouseScrollDelta.y * sensitivity;
            targetZoom = Mathf.Clamp(targetZoom, maxZoom, minZoom);
            newSize = Mathf.MoveTowards(cam.orthographicSize, targetZoom, speed * Time.deltaTime);
            cam.orthographicSize = newSize;
        }
    }

    public void CameraZoomDropdown(int val)
    {
        switch (val)
        {
            case 0:
                cam.orthographicSize = 5;
                break;
            case 1:
                cam.orthographicSize = 10;
                break;
            case 2:
                cam.orthographicSize = 20;
                break;
            case 3:
                scroll = true;
                break;
            default:
                break;
        }
        if (val != 3)
        {
            scroll = false;
        }
    }

    public void Change_Bknd()
    {
        cam.backgroundColor = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0f, 0.1f);
    }
}
