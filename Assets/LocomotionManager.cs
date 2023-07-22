using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class LocomotionManager : MonoBehaviour
{
    private float _defaultContiniousMoveSpeed;
    private float _defaultContiniousTurnSpeed;

    [SerializeField] private ActionBasedController _rightTeleportationRay;
    [SerializeField] private ActionBasedController _leftTeleportationRay;
    private ActionBasedController _currentTeleportationRay;
    private bool _blockTeleportation = false;

    [SerializeField] private InputActionProperty _rightTeleportActivateControls;
    [SerializeField] private InputActionProperty _leftTeleportActivateControls;
    private InputAction _teleportAction;

    [SerializeField] private InputActionProperty _rightVerticalMoveControls;
    [SerializeField] private InputActionProperty _leftVerticalMoveControls;

    [SerializeField] private InputActionProperty _rightMoveLocomotionControls;
    [SerializeField] private InputActionProperty _leftMoveLocomotionControls;
    [SerializeField] private InputActionProperty _rightTurnLocomotionControls;
    [SerializeField] private InputActionProperty _leftTurnLocomotionControls;

    [SerializeField] private Transform _additionalOffset;

    [SerializeField] private TunnelingVignetteController _tunnel;

    private ActionBasedContinuousMoveProvider _continuousMoveProvider;
    private ActionBasedSnapTurnProvider _snapTurnProvider;
    private ActionBasedContinuousTurnProvider _continuousTurnProvider;
    private ControllerButtonsSetting _controllerButtonsSetting;

    private void Awake()
    {
        _continuousMoveProvider = GetComponent<ActionBasedContinuousMoveProvider>();
        _snapTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();
        _continuousTurnProvider = GetComponent<ActionBasedContinuousTurnProvider>();
        _controllerButtonsSetting = GetComponent<ControllerButtonsSetting>();

        _defaultContiniousMoveSpeed = _continuousMoveProvider.moveSpeed;
        _defaultContiniousTurnSpeed = _continuousTurnProvider.turnSpeed;
    }

    private void Start()
    {
        UserSettings.Instance.OnShowTunnel += ShowTunnel;
        UserSettings.Instance.OnChangeLocomotionToLeft += ChangeLocomotionToLeft;
        UserSettings.Instance.OnChangePose += ChangeRigCameraOffset;
        UserSettings.Instance.OnResetHeight += ResetHeight;
        UserSettings.Instance.OnTurnType += ChangeTurnType;
    }

    private void OnDestroy()
    {
        UserSettings.Instance.OnShowTunnel -= ShowTunnel;
        UserSettings.Instance.OnChangeLocomotionToLeft -= ChangeLocomotionToLeft;
        UserSettings.Instance.OnChangePose -= ChangeRigCameraOffset;
        UserSettings.Instance.OnResetHeight -= ResetHeight;
        UserSettings.Instance.OnTurnType -= ChangeTurnType;
    }

    private void Update()
    {
        // Activate/Deactivate teleportation ray
        if (_currentTeleportationRay && _blockTeleportation)
            _currentTeleportationRay.gameObject.SetActive(false);
        else if (_currentTeleportationRay)
            _currentTeleportationRay.gameObject.SetActive(CheckIfActivated());
    }

    private bool CheckIfActivated()
    {
        // Listening for activation button press
        return (_teleportAction.phase == InputActionPhase.Performed) && (_teleportAction.ReadValue<Vector2>().y > 0.5f);
    }

    public void ResetHeight()
    {
        float defaultHeight = 1.703f;
        float newOffset = defaultHeight - Camera.main.transform.localPosition.y;
        ChangeRigCameraOffset(newOffset);
    }

    private void ChangeRigCameraOffset(float value)
    {
        _additionalOffset.localPosition = new Vector3(_additionalOffset.localPosition.x, value, _additionalOffset.localPosition.z);
    }

    private void ShowTunnel(bool value)
    {
        _tunnel.enabled = value;
    }

    private void ChangeLocomotionToLeft(bool value)
    {
        if (value)
        {
            // Enable Locomotion on Left controller
            _continuousMoveProvider.leftHandMoveAction = _leftMoveLocomotionControls;
            _continuousMoveProvider.rightHandMoveAction = _leftMoveLocomotionControls;

            _snapTurnProvider.leftHandSnapTurnAction = _rightTurnLocomotionControls;
            _snapTurnProvider.rightHandSnapTurnAction = _rightTurnLocomotionControls;

            _continuousTurnProvider.leftHandTurnAction = _rightTurnLocomotionControls;
            _continuousTurnProvider.rightHandTurnAction = _rightTurnLocomotionControls;

            // Change teleportation ray
            _currentTeleportationRay = _rightTeleportationRay;
            _teleportAction = _rightTeleportActivateControls.action.actionMap["Teleport Mode Activate"];
            _blockTeleportation = false;

            _continuousMoveProvider.enableFly = false;
            _continuousMoveProvider.moveSpeed = _defaultContiniousMoveSpeed;
        }
        else
        {
            // Enable Locomotion on Right controller
            _continuousMoveProvider.leftHandMoveAction = _rightMoveLocomotionControls;
            _continuousMoveProvider.rightHandMoveAction = _rightMoveLocomotionControls;

            _snapTurnProvider.leftHandSnapTurnAction = _leftTurnLocomotionControls;
            _snapTurnProvider.rightHandSnapTurnAction = _leftTurnLocomotionControls;

            _continuousTurnProvider.leftHandTurnAction = _leftTurnLocomotionControls;
            _continuousTurnProvider.rightHandTurnAction = _leftTurnLocomotionControls;

            // Change teleportation ray
            _currentTeleportationRay = _leftTeleportationRay;
            _teleportAction = _leftTeleportActivateControls.action.actionMap["Teleport Mode Activate"];
            _blockTeleportation = false;

            _continuousMoveProvider.enableFly = false;
            _continuousMoveProvider.moveSpeed = _defaultContiniousMoveSpeed;
        }
    }

    private void ChangeTurnType(float value)
    {
        if (value != 0)
        {
            _continuousTurnProvider.enabled = false;
            _snapTurnProvider.enabled = true;
            _snapTurnProvider.turnAmount = value;
        }
        else
        {
            _snapTurnProvider.enabled = false;
            _continuousTurnProvider.enabled = true;

            _continuousTurnProvider.turnSpeed = _defaultContiniousTurnSpeed;
        }
    }
}