using TMPro;
using UnityEngine;

public class DownloadingWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text percentsText;
    [SerializeField] private GameObject successWindow;
    [SerializeField] private GameObject errorWindow;

    private bool _listenForModelReciever;

    private void OnEnable()
    {
        percentsText.text = "0";
    }

    private void Update()
    {
        if (NetworkBehviour.Instance.ModelReceiver != null)
        {
            if (!_listenForModelReciever)
            {
                NetworkBehviour.Instance.OnModelReceived += OpenSuccessWindow;
                NetworkBehviour.Instance.ModelReceiver.OnModelReceivedError += OpenErrorWindow;
                _listenForModelReciever = true;
            }

            float x = NetworkBehviour.Instance.ModelReceiver.FullModelOffset;
            float y = NetworkBehviour.Instance.ModelReceiver.FullModelLenght;
            float value = (x / y) * 100f;
            value = Mathf.RoundToInt(value);

            if (value > 0)
                percentsText.text = value.ToString();
        }
        else
            _listenForModelReciever = false;
    }

    private void OpenSuccessWindow(byte[] d)
    {
        NetworkBehviour.Instance.OnModelReceived -= OpenSuccessWindow;
        NetworkBehviour.Instance.ModelReceiver.OnModelReceivedError -= OpenErrorWindow;
        successWindow.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OpenErrorWindow()
    {
        NetworkBehviour.Instance.OnModelReceived -= OpenSuccessWindow;
        NetworkBehviour.Instance.ModelReceiver.OnModelReceivedError -= OpenErrorWindow;
        errorWindow.SetActive(true);
        gameObject.SetActive(false);
    }
}
