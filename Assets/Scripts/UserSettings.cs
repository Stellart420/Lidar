using System;
using UnityEngine;

public class UserSettings
{
    private static Lazy<UserSettings> instance = new Lazy<UserSettings>(() => new UserSettings());

    public static UserSettings Instance => instance.Value;

    public event Action OnUpdateVisual;
    public event Action<bool> OnShowTunnel;
    public event Action<bool> OnChangeLocomotionToLeft;
    public event Action<bool> OnShowControllerHints;
    public event Action<float> OnChangePose;
    public event Action OnResetHeight;
    public event Action<float> OnTurnType;
    public event Action OnChangeLanguage;

    public enum Controller
    {
        Left,
        Right
    }

    public enum PlayerPose
    {
        Sit,
        Stand,
        Custom
    }

    private int _tunnel = 1;
    private string _locomotion = Controller.Right.ToString();
    private int _hints = 1;
    private string _pose = PlayerPose.Sit.ToString();
    private float _turn = 45f;
    private float _environmentVolume = 1f;
    private string _language = "";

    public int Tunnel
    {
        get { return _tunnel; }
        set
        {
            _tunnel = value;

            if (_tunnel == 0)
                OnShowTunnel?.Invoke(false);
            else
                OnShowTunnel?.Invoke(true);
        }
    }

    public string Locomotion
    {
        get { return _locomotion; }
        set
        {
            _locomotion = value;

            if (_locomotion == Controller.Left.ToString())
                OnChangeLocomotionToLeft?.Invoke(true);
            else
                OnChangeLocomotionToLeft?.Invoke(false);
        }
    }

    public int Hints
    {
        get { return _hints; }
        set
        {
            _hints = value;

            if (_hints == 0)
                OnShowControllerHints?.Invoke(false);
            else
                OnShowControllerHints?.Invoke(true);
        }
    }

    public string Pose
    {
        get { return _pose; }
        set
        {
            _pose = value;

            if (_pose == PlayerPose.Custom.ToString())
                OnResetHeight?.Invoke();
            else if (_pose == PlayerPose.Stand.ToString())
                OnChangePose?.Invoke(0.07f);
            else
                OnChangePose?.Invoke(0.467f);
        }
    }

    public float Turn
    {
        get { return _turn; }
        set
        {
            _turn = value;

            OnTurnType?.Invoke(value);
        }
    }

    public float EnvironmentVolume
    {
        get { return _environmentVolume; }
        set
        {
            _environmentVolume = value;

            AudioListener.volume = value;
        }
    }

    public string Language
    {
        get { return _language; }
        set { _language = value; }
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("settings_tunnel"))
            Tunnel = PlayerPrefs.GetInt("settings_tunnel");
        else
            Tunnel = _tunnel;

        if (PlayerPrefs.HasKey("settings_locomotion"))
            Locomotion = PlayerPrefs.GetString("settings_locomotion");
        else
            Locomotion = _locomotion;

        if (PlayerPrefs.HasKey("settings_hints"))
            Hints = PlayerPrefs.GetInt("settings_hints");
        else
            Hints = _hints;

        if (PlayerPrefs.HasKey("settings_pose"))
            Pose = PlayerPrefs.GetString("settings_pose");
        else
            Pose = _pose;

        if (PlayerPrefs.HasKey("settings_turn"))
            Turn = PlayerPrefs.GetFloat("settings_turn");
        else
            Turn = _turn;

        if (PlayerPrefs.HasKey("settings_environment_volume"))
            EnvironmentVolume = PlayerPrefs.GetFloat("settings_environment_volume");
        else
            EnvironmentVolume = _environmentVolume;

        if (PlayerPrefs.HasKey("settings_language"))
            Language = PlayerPrefs.GetString("settings_language");
        else
            Language = "English";

        // Save to fill empty PlayerPrefs
        SaveSettings();

        OnUpdateVisual?.Invoke();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("settings_tunnel", Tunnel);
        PlayerPrefs.SetString("settings_locomotion", Locomotion);
        PlayerPrefs.SetInt("settings_hints", Hints);
        PlayerPrefs.SetString("settings_pose", Pose);
        PlayerPrefs.SetFloat("settings_turn", Turn);
        PlayerPrefs.SetFloat("settings_environment_volume", EnvironmentVolume);
        PlayerPrefs.SetString("settings_language", Language);

        PlayerPrefs.Save();

        OnUpdateVisual?.Invoke();
    }
}
