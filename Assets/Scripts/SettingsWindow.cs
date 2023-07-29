using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] Button _onTunnelVisual;
    [SerializeField] Button _offTunnelVisual;
    [SerializeField] Button _leftLocomotionVisual;
    [SerializeField] Button _rightLocomotionVisual;
    [SerializeField] Button _onControllerHintsVisual;
    [SerializeField] Button _offControllerHintsVisual;
    [SerializeField] Button _resetHeightVisual;
    [SerializeField] Button _turn45Visual;
    [SerializeField] Button _turn90Visual;
    [SerializeField] Button _turnContinuousVisual;

    [SerializeField] TMP_Text _appVersionLabel;

    private Color _selectedTextColor = Color.white;
    private Color32 _deSelectedTextColor = new Color32(0, 92, 212, 255);


    public void Awake()
    {
        _onTunnelVisual.onClick.AddListener(ShowTunnel);
        _offTunnelVisual.onClick.AddListener(HideTunnel);
        _leftLocomotionVisual.onClick.AddListener(SetLeftLocomotion);
        _rightLocomotionVisual.onClick.AddListener(SetRightLocomotion);
        _onControllerHintsVisual.onClick.AddListener(ShowControllerHints);
        _offControllerHintsVisual.onClick.AddListener(HideControllerHints);
        _resetHeightVisual.onClick.AddListener(OnResetHeightClick);
        _turn45Visual.onClick.AddListener(OnTurn45Click);
        _turn90Visual.onClick.AddListener(OnTurn90Click);
        _turnContinuousVisual.onClick.AddListener(OnTurnContinuousClick);

        _appVersionLabel.text = Application.version;

        UpdateSettingsVisual();
    }

    private void OnEnable()
    {
        UserSettings.Instance.OnUpdateVisual += UpdateSettingsVisual;
        UpdateSettingsVisual();
    }

    private void OnDisable()
    {
        UserSettings.Instance.OnUpdateVisual -= UpdateSettingsVisual;
    }

    private void SwitchTextColor(TMP_Text selectedText, TMP_Text deSelectedText)
    {
        selectedText.color = _selectedTextColor;
        deSelectedText.color = _deSelectedTextColor;
    }

    private void ShowTunnel()
    {
        UserSettings.Instance.Tunnel = 1;
        UserSettings.Instance.SaveSettings();
    }

    private void HideTunnel()
    {
        UserSettings.Instance.Tunnel = 0;
        UserSettings.Instance.SaveSettings();
    }

    private void SetLeftLocomotion()
    {
        UserSettings.Instance.Locomotion = UserSettings.Controller.Left.ToString();
        UserSettings.Instance.SaveSettings();
    }

    private void SetRightLocomotion()
    {
        UserSettings.Instance.Locomotion = UserSettings.Controller.Right.ToString();
        UserSettings.Instance.SaveSettings();
    }

    private void ShowControllerHints()
    {
        UserSettings.Instance.Hints = 1;
        UserSettings.Instance.SaveSettings();
    }

    private void HideControllerHints()
    {
        UserSettings.Instance.Hints = 0;
        UserSettings.Instance.SaveSettings();
    }

    private void OnResetHeightClick()
    {
        UserSettings.Instance.Pose = UserSettings.PlayerPose.Custom.ToString();
        UserSettings.Instance.SaveSettings();
    }

    private void OnTurn45Click()
    {
        UserSettings.Instance.Turn = 45f;
        UserSettings.Instance.SaveSettings();
    }

    private void OnTurn90Click()
    {
        UserSettings.Instance.Turn = 90f;
        UserSettings.Instance.SaveSettings();
    }

    private void OnTurnContinuousClick()
    {
        UserSettings.Instance.Turn = 0f;
        UserSettings.Instance.SaveSettings();
    }

    private void UpdateSettingsVisual()
    {
        // Tunnel settings visual
        if (UserSettings.Instance.Tunnel == 0)
        {
            SwitchTextColor(_offTunnelVisual.GetComponentInChildren<TMP_Text>(), _onTunnelVisual.GetComponentInChildren<TMP_Text>());
            _onTunnelVisual.interactable = true;
            _offTunnelVisual.interactable = false;
        }
        else
        {
            SwitchTextColor(_onTunnelVisual.GetComponentInChildren<TMP_Text>(), _offTunnelVisual.GetComponentInChildren<TMP_Text>());
            _onTunnelVisual.interactable = false;
            _offTunnelVisual.interactable = true;
        }

        // Locomotion settings visual
        if (UserSettings.Instance.Locomotion == UserSettings.Controller.Left.ToString())
        {
            SwitchTextColor(_leftLocomotionVisual.GetComponentInChildren<TMP_Text>(), _rightLocomotionVisual.GetComponentInChildren<TMP_Text>());
            _leftLocomotionVisual.interactable = false;
            _rightLocomotionVisual.interactable = true;
        }
        else
        {
            SwitchTextColor(_rightLocomotionVisual.GetComponentInChildren<TMP_Text>(), _leftLocomotionVisual.GetComponentInChildren<TMP_Text>());
            _leftLocomotionVisual.interactable = true;
            _rightLocomotionVisual.interactable = false;
        }

        // Hints settings visual
        if (UserSettings.Instance.Hints == 0)
        {
            SwitchTextColor(_offControllerHintsVisual.GetComponentInChildren<TMP_Text>(), _onControllerHintsVisual.GetComponentInChildren<TMP_Text>());
            _onControllerHintsVisual.interactable = true;
            _offControllerHintsVisual.interactable = false;
        }
        else
        {
            SwitchTextColor(_onControllerHintsVisual.GetComponentInChildren<TMP_Text>(), _offControllerHintsVisual.GetComponentInChildren<TMP_Text>());
            _onControllerHintsVisual.interactable = false;
            _offControllerHintsVisual.interactable = true;
        }

        // Turn settings visual
        if (UserSettings.Instance.Turn == 0f)
        {
            SwitchTextColor(_turnContinuousVisual.GetComponentInChildren<TMP_Text>(), _turn45Visual.GetComponentInChildren<TMP_Text>());
            SwitchTextColor(_turnContinuousVisual.GetComponentInChildren<TMP_Text>(), _turn90Visual.GetComponentInChildren<TMP_Text>());
            _turn45Visual.interactable = true;
            _turn90Visual.interactable = true;
            _turnContinuousVisual.interactable = false;
        }
        else if (UserSettings.Instance.Turn == 90f)
        {
            SwitchTextColor(_turn90Visual.GetComponentInChildren<TMP_Text>(), _turn45Visual.GetComponentInChildren<TMP_Text>());
            SwitchTextColor(_turn90Visual.GetComponentInChildren<TMP_Text>(), _turnContinuousVisual.GetComponentInChildren<TMP_Text>());
            _turn45Visual.interactable = true;
            _turn90Visual.interactable = false;
            _turnContinuousVisual.interactable = true;
        }
        else
        {
            SwitchTextColor(_turn45Visual.GetComponentInChildren<TMP_Text>(), _turn90Visual.GetComponentInChildren<TMP_Text>());
            SwitchTextColor(_turn45Visual.GetComponentInChildren<TMP_Text>(), _turnContinuousVisual.GetComponentInChildren<TMP_Text>());
            _turn45Visual.interactable = false;
            _turn90Visual.interactable = true;
            _turnContinuousVisual.interactable = true;
        }
    }

    public void OnCloseCLick()
    {
        gameObject.SetActive(false);
    }
}
