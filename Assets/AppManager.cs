using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [SerializeField] public MenuWindow menuUI;
    [SerializeField] public ModelListWindow modelListWindow;
    [SerializeField] public GameObject downloadingWindow;
    [SerializeField] public Material DeserializeMaterial;
    [SerializeField] public GameObject ModelListItem;
    [SerializeField] public GameObject menuUpDown;

    [SerializeField] private float value = 0.1f;

    public GameObject LoadedModel { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        Waiter();
    }

    private void Update()
    {
        if (LoadedModel == null)
            menuUpDown.SetActive(false);
        else
            menuUpDown.SetActive(true);
    }

    private async void Waiter()
    {
        Debug.Log("load settings...");
        await Task.Delay(3000);
        UserSettings.Instance.LoadSettings();
    }

    public void MoveModelUp()
    {
        LoadedModel.transform.position = new Vector3(LoadedModel.transform.position.x, LoadedModel.transform.position.y + value, LoadedModel.transform.position.z);
    }

    public void MoveModelDown()
    {
        LoadedModel.transform.position = new Vector3(LoadedModel.transform.position.x, LoadedModel.transform.position.y - value, LoadedModel.transform.position.z);
    }
}
