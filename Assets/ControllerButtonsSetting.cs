using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class ControllerButtonsSetting : MonoBehaviour
{
    [SerializeField] private XRController rightHandRayController;
    [SerializeField] private XRController leftHandRayController;

    [SerializeField] private InputActionProperty rightActivateButton;
    [SerializeField] private InputActionProperty leftActivateButton;
    [SerializeField] private InputActionProperty rightSecondaryButton;
    [SerializeField] private InputActionProperty leftSecondaryButton;

    [SerializeField] private InputActionProperty rightTeleportButton;
    [SerializeField] private InputActionProperty leftTeleportButton;

    [SerializeField] private InputActionProperty _rightVerticalMoveButton;
    [SerializeField] private InputActionProperty _leftVerticalMoveButton;

    private LocomotionManager _locomotionManager;

    private bool _showRightRay = true;
    private bool _showLeftRay = true;

    private void Awake()
    {
        rightActivateButton.action.started += RightRayToggle;
        leftActivateButton.action.started += LeftRayToggle;
        rightSecondaryButton.action.started += MenuToggle;
        leftSecondaryButton.action.started += MenuToggle;

        rightTeleportButton.action.started += TeleportToggle;
        leftTeleportButton.action.started += TeleportToggle;

        _locomotionManager = GetComponent<LocomotionManager>();
        /*if (ExhibitionBehaviour.Instance != null)
        {
            ExhibitionBehaviour.Instance.UI.OpenUIWindowEvent += OnOpenUIWindowEvent;
            ExhibitionBehaviour.Instance.UI.MenuWindow.OpenUIWindowEvent += OnOpenUIWindowEvent;
        }*/
    }

    private void Start()
    {
        ToggleRays(_showRightRay, _showLeftRay);
    }

    private void OnDestroy()
    {
        rightActivateButton.action.started -= RightRayToggle;
        leftActivateButton.action.started -= LeftRayToggle;
        rightSecondaryButton.action.started -= MenuToggle;
        leftSecondaryButton.action.started -= MenuToggle;

        /*if (ExhibitionBehaviour.Instance != null)
        {
            ExhibitionBehaviour.Instance.UI.OpenUIWindowEvent -= OnOpenUIWindowEvent;
            ExhibitionBehaviour.Instance.UI.MenuWindow.OpenUIWindowEvent -= OnOpenUIWindowEvent;
        }*/

        rightTeleportButton.action.started -= TeleportToggle;
        leftTeleportButton.action.started -= TeleportToggle;

    }

    public void ToggleRays(bool showRightRay, bool showLeftRay)
    {
        leftHandRayController.gameObject.SetActive(showLeftRay);
        rightHandRayController.gameObject.SetActive(showRightRay);

        // For the non controllers external toggle call
        _showLeftRay = showLeftRay;
        _showRightRay = showRightRay;
    }

    private void RightRayToggle(InputAction.CallbackContext context)
    {
        if (_showRightRay)
            _showRightRay = false;
        else
        {
            _showRightRay = true;
            _showLeftRay = false;
        }

        ToggleRays(_showRightRay, _showLeftRay);
    }

    private void LeftRayToggle(InputAction.CallbackContext context)
    {
        if (_showLeftRay)
            _showLeftRay = false;
        else
        {
            _showLeftRay = true;
            _showRightRay = false;
        }

        ToggleRays(_showRightRay, _showLeftRay);
    }

    public void MenuToggle(InputAction.CallbackContext context)
    {
        AppManager.Instance.menuUI.ToggleControllerMenu();
    }

    private void TeleportToggle(InputAction.CallbackContext context)
    {
        if (_locomotionManager != null)
            if (_locomotionManager.enabled)
            {
                leftHandRayController.gameObject.SetActive(false);
                rightHandRayController.gameObject.SetActive(false);
                _showLeftRay = false;
                _showRightRay = false;
            }
    }

    private void OnOpenUIWindowEvent()
    {
        // Turn on interaction rays on UI windows opened
        ToggleRays(true, true);
    }
}