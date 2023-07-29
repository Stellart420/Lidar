using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuWindow : MonoBehaviour
{
    [SerializeField] private GameObject settingsWindow;
    [SerializeField] private GameObject modelListWindow;

    private bool _menuOn;

    private FastMenuRotateWithCamera _windowRotator;

    public event Action OpenUIWindowEvent;

    private void Start()
    {
        _windowRotator = GetComponent<FastMenuRotateWithCamera>();
    }

    // Toggle on controller
    public void ToggleControllerMenu()
    {
        if (_menuOn)
            CloseMenu();
        else
            ShowControllerMenu();
    }

    private void ShowControllerMenu()
    {
        // Close all windows
        modelListWindow.SetActive(false);
        settingsWindow.SetActive(false);

        gameObject.SetActive(true);
        _menuOn = true;
        if (_windowRotator != null)
        {
            // Move UI to player
            var rigCamera = Camera.main.transform;
            transform.position = rigCamera.position + rigCamera.forward * _windowRotator.RigDistance;
            transform.rotation = Quaternion.Euler(rigCamera.rotation.eulerAngles.x, rigCamera.rotation.eulerAngles.y, 0f);

            //_windowRotator.enabled = true;
        }
    }

    public void CloseMenu()
    {
        _menuOn = false;
        /*if (_windowRotator != null)
            _windowRotator.enabled = false;*/

        gameObject.SetActive(false);
    }

    public void OnExitClick()
    {
        Application.Quit();
    }

    public void OnModelClick()
    {
        CloseMenu();
        modelListWindow.SetActive(true);
    }

    public void OnSettingsClick()
    {
        CloseMenu();
        settingsWindow.SetActive(true);
    }
}
