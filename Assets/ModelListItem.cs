using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelListItem : MonoBehaviour
{
    [SerializeField] private Button modelMainButton;

    private void Start()
    {
        modelMainButton.onClick.AddListener(OnModelMainButtonClick);
    }

    private void OnDestroy()
    {
        modelMainButton.onClick.RemoveListener(OnModelMainButtonClick);
    }

    private void OnModelMainButtonClick()
    {
        AppManager.Instance.downloadingWindow.SetActive(true);
    }    
}
