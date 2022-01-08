using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{

    [SerializeField] GameObject menu, menuToggleButton, settings, zoomDropdown, presets, controls, credits, expandables;
    private bool displayNext = false;
    public static bool mouseOverMenu = false;
    

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in expandables.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            mouseOverMenu = true;
        } else
        {
            mouseOverMenu = false;
        }
    }

    public void ZoomDropdown()
    {
        if (zoomDropdown.activeSelf)
        {
            zoomDropdown.SetActive(false);
        } else
        {
            foreach (Transform child in expandables.transform)
            {
                child.gameObject.SetActive(false);
            }
            zoomDropdown.SetActive(true);
        }
    }

    public void Settings()
    {
        if (settings.activeSelf)
        {
            settings.SetActive(false);
        }
        else
        {
            foreach (Transform child in expandables.transform)
            {
                child.gameObject.SetActive(false);
            }
            settings.SetActive(true);
        }
    }

    public void Presets()
    {
        if (presets.activeSelf)
        {
            presets.SetActive(false);
        }
        else
        {
            foreach (Transform child in expandables.transform)
            {
                child.gameObject.SetActive(false);
            }
            presets.SetActive(true);
        }
    }

    public void Controls()
    {
        if (controls.activeSelf)
        {
            controls.SetActive(false);
        }
        else
        {
            foreach (Transform child in expandables.transform)
            {
                child.gameObject.SetActive(false);
            }
            controls.SetActive(true);
        }
    }

    public void Credits()
    {
        if (credits.activeSelf)
        {
            credits.SetActive(false);
        }
        else
        {
            foreach (Transform child in expandables.transform)
            {
                child.gameObject.SetActive(false);
            }
            credits.SetActive(true);
        }
    }

    public void MenuToggle()
    {
        if (displayNext == false)
        {
            menu.SetActive(false);
            foreach (Transform child in expandables.transform)
            {
                child.gameObject.SetActive(false);
            }

            menuToggleButton.transform.Translate(Vector2.right * 130);
            menuToggleButton.transform.localScale = new Vector3(-1, -1, -1);

            displayNext = true;
        } else
        {
            menu.SetActive(true);

            menuToggleButton.transform.Translate(Vector2.left * 130);
            menuToggleButton.transform.localScale = new Vector3(1, 1, 1);

            displayNext = false;
        }
    }
}
