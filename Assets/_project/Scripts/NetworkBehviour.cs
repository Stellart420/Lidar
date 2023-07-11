using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telepathy;
using UnityEngine;

public class NetworkBehviour : MonoBehaviour
{
    public static NetworkBehviour Instance;

    [SerializeField] private int _networkPortTCP;
    [SerializeField] private int _operationsPerUpdateCount = 1000;
    [SerializeField] private string _applicationServerIp;

    private Client _clientTCP;
    private int _connectionTimeOutMs = 10000;
    private float _tcpKeepAliveNextTime;
    private float _tcpKeepAliveDelaySec = 30;

    public bool IsConnected { get { return _clientTCP.Connected; } }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);

        _clientTCP = new Client(100000);
        _clientTCP.ReceiveTimeout = 1000 * 60; // 1 minute


        _clientTCP.OnConnected = () => OnConnectedToServer();
        _clientTCP.OnData = (message) => OnDataReceived(message);
        _clientTCP.OnDisconnected = () => OnDisconnected();
    }

    private void Update()
    {
        _clientTCP.Tick(_operationsPerUpdateCount);


        if (_clientTCP.Connected)
        {
            if (Time.time > _tcpKeepAliveNextTime)
            {
                SendKeepAlive();

                _tcpKeepAliveNextTime += _tcpKeepAliveDelaySec;
            }
        }
    }

    private void OnApplicationQuit()
    {
        //_clientUDP.Disconnect();
        _clientTCP.Disconnect();
    }


    public async Task<bool> Connect()
    {
        _clientTCP.Connect(_applicationServerIp, _networkPortTCP, null);

        int timer = 0;
        while (!_clientTCP.Connected)
        {
            await Task.Delay(100);
            timer += 100;

            if (timer > _connectionTimeOutMs)
                return false;
        }

        _tcpKeepAliveNextTime = Time.time;
        return true;
    }

    private void SendKeepAlive()
    {
        SendNetworkMessage(BitConverter.GetBytes((int)MessageType.KEEP_ALIVE));
    }

    public void SendNetworkMessage(byte[] data)
    {
        if (!_clientTCP.Connected)
            return;

        if (!_clientTCP.Send(new ArraySegment<byte>(data)))
        {
            Debug.LogError($"Sending TCP error. data size: {data.Length}/{_clientTCP.MaxMessageSize}");
            throw new Exception();
        }
    }

    public void Disconnect()
    {
        if (_clientTCP.Connected)
            _clientTCP.Disconnect();
    }



    private void OnConnectedToServer()
    {
    }

    private async void OnDisconnected()
    {

    }

    private void OnDataReceived(ArraySegment<byte> bufferSegment)
    {

        //bufferSegment = new ArraySegment<byte>(bufferSegment.ToArray());

        int offset = 0;
        var messageType = (MessageType)BitConverter.ToInt32(bufferSegment.Array, offset);
        offset += 4;

        int remoteId = -1;
        //Debug.Log($"Message received: {messageType}");
        switch (messageType)
        {
            case MessageType.KEEP_ALIVE:
                break;
         
            default:
                throw new Exception($"Unhandled message type: {messageType}");
        }
    }
}

public enum MessageType
{
    KEEP_ALIVE = 255,

    HELLO = 0,
    ModelFromPhone = 2,
    GetModelsListFromVR = 3,
    GetModelFromVR = 4,

    SendMaodelsListToVR = 10,
    SendModelToVR = 11,

    GotModelsListFromServer = 20,
    GotModelFromServer = 21
}