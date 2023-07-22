using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristics;    
    private InputDevice targetDevice;
    public Animator handAnimator;

    [SerializeField] private GameObject controllerWithHints;
    [SerializeField] private GameObject interactionRay;

    private bool showController = true;

    private bool applicationUnFocus;
    private bool rememberRay = false;               // When enable keyboard remember interaction ray status to restore after
    private bool rememberControllerHint = false;    // When enable keyboard remember controller hint status to restore after

    void Start()
    {
        TryInitialize();

        UserSettings.Instance.OnShowControllerHints += ShowControllerHints;
    }

    private void OnDestroy()
    {
        UserSettings.Instance.OnShowControllerHints -= ShowControllerHints;
    }

    void TryInitialize()
    {
        List<InputDevice> devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);
        if (devices.Count > 0)
        {
            targetDevice = devices[0];
        }
    }

    void UpdateHandAnimation()
    {
        float summ = 0;

        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);

            summ += triggerValue;
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);

            summ += gripValue;
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }

        // Hide controller when any button pressed
        if (summ > 0.01f)
            controllerWithHints?.SetActive(false);
        else if (showController)
            controllerWithHints?.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!targetDevice.isValid)
        {
            TryInitialize();
        }
        else if (!applicationUnFocus)
        {
            UpdateHandAnimation();
        }
    }

#if !UNITY_EDITOR
    // Disable hands and interactive rays when Application is out of focus (Oculus system keybord is on) 
    private void OnApplicationFocus(bool focus)
    {
        applicationUnFocus = !focus;

        if (applicationUnFocus)
        {
            handAnimator.gameObject.SetActive(false);

            if (interactionRay.activeSelf)
            {
                rememberRay = true;
                interactionRay.SetActive(false);
            }
            if (controllerWithHints.activeSelf)
            {
                rememberControllerHint = true;
                controllerWithHints.SetActive(false);
            }
        }
        else
        {
            handAnimator.gameObject.SetActive(true);

            if (rememberRay)
            {
                interactionRay.SetActive(true);
                rememberRay = false;
            }
            if (rememberControllerHint)
            {
                controllerWithHints.SetActive(true);
                rememberControllerHint = false;
            }
        }
    }
#endif

    private void ShowControllerHints(bool value)
    {
        showController = value;
        controllerWithHints.SetActive(value);
    }
}
