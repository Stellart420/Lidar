using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelListWindow : MonoBehaviour
{
    [SerializeField] private Transform content;

    private void OnEnable()
    {
        FillModelList();
    }

    public void OnCloseCLick()
    {
        gameObject.SetActive(false);
    }

    private async void FillModelList()
    {
        await NetworkBehviour.Instance.Connect();

        NetworkBehviour.Instance.OnModelListReceived += (list) =>
        {
            NetworkBehviour.Instance.OnModelListReceived = null;
            for (int j = 0; j < content.childCount; j++)
            {
                Destroy(content.GetChild(j).gameObject);
            }
            content.DetachChildren();

            var i = 0;
            list.Reverse();
            foreach (var model in list)
            {
                var go = Instantiate(AppManager.Instance.ModelListItem, content);
                string name = list[i];
                go.GetComponentInChildren<TMP_Text>().text = name;
                go.GetComponentInChildren<Button>().onClick.AddListener(delegate { GetModel(name); });
                i++;
            }
        };

        NetworkBehviour.Instance.SendGetModelList();
    }

    public void GetModel(string name)
    {
        NetworkBehviour.Instance.OnModelReceived += async (byte[] d) =>
        {
            Debug.Log($"Received model {d.Length}");
            NetworkBehviour.Instance.OnModelReceived = null;

            await new WaitForUpdate();

            var serializer = new ModelSerializer();
            serializer.MaterialForDeserialize = AppManager.Instance.DeserializeMaterial;

            if (AppManager.Instance.LoadedModel != null)
                Destroy(AppManager.Instance.LoadedModel);

            AppManager.Instance.LoadedModel = serializer.Deserialize(d, 0);
        };

        OnCloseCLick();
        NetworkBehviour.Instance.GetModel(name);
    }
}
