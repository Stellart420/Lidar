using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelListWindow : MonoBehaviour
{
    [SerializeField] private Transform content;

    public bool ListenForModelReciever { get; set; }

    private void OnEnable()
    {
        ClearList();
        EnableButtons(true);
        FillModelList();
    }

    public void OnCloseCLick()
    {
        gameObject.SetActive(false);
    }

    public void EnableButtons(bool value)
    {
        List<Button> buttons = AppManager.Instance.modelListWindow.GetComponentsInChildren<Button>().ToList();

        foreach (var button in buttons)
        {
            button.interactable = value;
        }
    }

    private void ClearList()
    {
        for (int j = 0; j < content.childCount; j++)
        {
            Destroy(content.GetChild(j).gameObject);
        }
        content.DetachChildren();
    }

    private async void FillModelList()
    {
        await VRTeleportation_NetworkBehviour.Instance.Connect();

        VRTeleportation_NetworkBehviour.Instance.OnModelListReceived += (list) =>
        {
            VRTeleportation_NetworkBehviour.Instance.OnModelListReceived = null;
            ClearList();

            var i = 0;
            list.Reverse();
            foreach (var model in list)
            {
                var go = Instantiate(AppManager.Instance.ModelListItem, content);
                
                // Init model item
                string name = list[i];
                go.GetComponent<ModelListItem>().Name = name;

                i++;
            }
        };

        VRTeleportation_NetworkBehviour.Instance.SendGetModelList();
    }
}
