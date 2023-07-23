using kcp2k;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telepathy;
using UnityEngine;

public class NetworkBehviour : MonoBehaviour
{
    public static NetworkBehviour Instance;

    public Action<byte[]> OnModelReceived;
    public Action<List<string>> OnModelListReceived;

    public Action OnIncomingCall;
    public Action<bool> OnConnectedToCall;
    public Action<int, int, byte[]> OnAudioFrameReceived;

    [SerializeField] private int _networkPortTCP;
    [SerializeField] private int _operationsPerUpdateCount = 1000;
    [SerializeField] private string _applicationServerIp;

    private Client _clientTCP;
    private int _connectionTimeOutMs = 10000;
    private float _tcpKeepAliveNextTime;
    private float _tcpKeepAliveDelaySec = 30;

    private KcpClient _clientUDP;
    private int _udpId = 0;

    private string _networkName;

    public ModelReceiver ModelReceiver { get; private set; }

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

        var config = new KcpConfig() { DualMode = false, NoDelay = true };
        _clientUDP = new KcpClient(OnConnectedUDP, OnDataUDP, OnDisconnectedUDP, OnErrorUDP, config);

        _networkName = $"User{UnityEngine.Random.Range(1, 9999999)}";
    }

    private void Update()
    {
        _clientTCP.Tick(_operationsPerUpdateCount);

        _clientUDP.Tick();

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
        _clientUDP.Disconnect();
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

    public async Task SendModel(byte[] modelData, string name, Action<float> onPercentChange)
    {
        Debug.Log("Connected");
        await Task.Delay(500);

        var helloMessage = new List<byte>();
        helloMessage.AddRange(BitConverter.GetBytes((int)MessageType.HELLO));
        helloMessage.AddRange(BitConverter.GetBytes(true));

        SendNetworkMessage(helloMessage.ToArray());

        await Task.Delay(500);

        var infoMessage = new List<byte>();
        infoMessage.AddRange(BitConverter.GetBytes((int)MessageType.ModelFromPhone));
        infoMessage.AddRange(BitConverter.GetBytes(0)); // model info

        var namaArray = System.Text.Encoding.UTF8.GetBytes(name);
        infoMessage.AddRange(BitConverter.GetBytes(namaArray.Length));
        infoMessage.AddRange(namaArray);

        infoMessage.AddRange(BitConverter.GetBytes(modelData.Length));
        SendNetworkMessage(infoMessage.ToArray());

        await Task.Delay(500);

        int sendedBytes = 0;

        do
        {
            if (!IsConnected)
            {
                Debug.Log("Disconnected while transfering");
                break;
            }

            var chunckMsg = new List<byte>();
            chunckMsg.AddRange(BitConverter.GetBytes((int)MessageType.ModelFromPhone));
            chunckMsg.AddRange(BitConverter.GetBytes(1)); // model data

            var chunckLenght = (modelData.Length - sendedBytes < 90000) ? modelData.Length - sendedBytes : 90000;
            chunckMsg.AddRange(BitConverter.GetBytes(chunckLenght));
            chunckMsg.AddRange(new ArraySegment<byte>(modelData, sendedBytes, chunckLenght));

            SendNetworkMessage(chunckMsg.ToArray());
            sendedBytes += chunckLenght;
            Debug.Log($"Sended: {sendedBytes}/{modelData.Length}");
            onPercentChange?.Invoke(((float)sendedBytes / (float)modelData.Length) * 100);
            await Task.Delay(500);
        }
        while (sendedBytes < modelData.Length);

        Debug.Log("Sended ALL");

    }

    private void OnConnectedToServer()
    {
    }

    private async void OnDisconnected()
    {

    }

    private void OnDataReceived(ArraySegment<byte> bufferSegment)
    {
        var arrayData = new byte[bufferSegment.Count];
        Buffer.BlockCopy(bufferSegment.Array, 0, arrayData, 0, arrayData.Length);

        int offset = 0;
        var messageType = (MessageType)BitConverter.ToInt32(arrayData, offset);
        offset += 4;
        Debug.Log($"msg: {messageType}");
        switch (messageType)
        {
            case MessageType.KEEP_ALIVE:
                break;
            case MessageType.GotModelsListFromServer:
                HandleModelList(arrayData, offset);
                break;
            case MessageType.GotModelFromServer:
                if (ModelReceiver == null)
                    ModelReceiver = new ModelReceiver();

                if(ModelReceiver.HandleMessage(arrayData, offset))
                {
                    if(ModelReceiver.IsReceiveCompleted)
                    {
                        OnModelReceived?.Invoke(ModelReceiver.FullModelData);
                        ModelReceiver = null;
                    }
                }
                break;
            case MessageType.ConnectedToCall:
                HandleConnectedToCall(arrayData, offset);
                break;
            case MessageType.CallRoomListUpdate:
                HandleIncomingCall();
                break;

            default:
                throw new Exception($"Unhandled message type: {messageType}");
        }
    }


    private void HandleModelList(byte[] data, int offset)
    {
        var count = BitConverter.ToInt32(data, offset);
        offset += 4;

        List<string> names = new List<string>();

        for (int i = 0; i < count; ++i)
        {
            var nameLenght = BitConverter.ToInt32(data, offset);
            offset += 4;

            var nameData = new byte[nameLenght];
            Buffer.BlockCopy(data, offset, nameData, 0, nameLenght);
            offset += nameLenght;

            var name = System.Text.Encoding.UTF8.GetString(nameData);
            names.Add(name);

            Debug.Log(name);
        }

        OnModelListReceived?.Invoke(names);
    }

    private void HandleConnectedToCall(byte[] data, int offset)
    {
        OnConnectedToCall?.Invoke(true);
    }

    private void HandleIncomingCall()
    {
        OnIncomingCall?.Invoke();
    }

    [ContextMenu("GetModelList")]
    public async void SendGetModelList()
    {
        var helloMessage = new List<byte>();
        helloMessage.AddRange(BitConverter.GetBytes((int)MessageType.HELLO));
        helloMessage.AddRange(BitConverter.GetBytes(true));
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(_networkName);
        helloMessage.AddRange(BitConverter.GetBytes(nameBytes.Length));
        helloMessage.AddRange(nameBytes);


        SendNetworkMessage(helloMessage.ToArray());

        await Task.Delay(500);

        var buffer = new List<byte>();
        buffer.AddRange(BitConverter.GetBytes((int)MessageType.GetModelsListFromVR));

        SendNetworkMessage(buffer.ToArray());
        /*List<string> list = new List<string>();
        list.Add("Test room");
        OnModelListReceived?.Invoke(list);*/
    }

    [ContextMenu("GetModel")]
    public async void GetModel(string name)
    {
        var buffer = new List<byte>();

        buffer.AddRange(BitConverter.GetBytes((int)MessageType.GetModelFromVR));

        var nameData = System.Text.Encoding.UTF8.GetBytes(name);
        buffer.AddRange(BitConverter.GetBytes(nameData.Length));
        buffer.AddRange(nameData);

        SendNetworkMessage(buffer.ToArray());

        /*var file = File.ReadAllBytes("C:\\Users\\Makc\\Desktop\\model.bin");
        Debug.Log(file.Length);
        await Task.Delay(1000);
        OnModelReceived?.Invoke(file);*/
    }

    #region UDP_CALLBACKS
    private void SendNetworkMessageUDP(byte[] data, KcpChannel channel)
    {
        try
        {
            _clientUDP.Send(new ArraySegment<byte>(data), channel);
        }
        catch (Exception e)
        {
            Debug.LogError("HANDLE EXCEPTION UDP:");
            Debug.LogException(e);
        }
    }

    private void OnConnectedUDP()
    {
        Debug.Log($"UDP connect");
    }

    private void OnDataUDP(ArraySegment<byte> data, KcpChannel channel)
    {
        var arrayData = new byte[data.Count];
        Buffer.BlockCopy(data.Array, data.Offset, arrayData, 0, data.Count);


        int offset = 0;
        var messageType = (MessageTypeUDP)BitConverter.ToInt32(arrayData, offset);
        offset += 4;

        switch (messageType)
        {
            case MessageTypeUDP.HELLO:
                _udpId = BitConverter.ToInt32(arrayData, offset);
                break;
            case MessageTypeUDP.CALL_FRAME:
                var chunkNumber = BitConverter.ToInt32(arrayData, offset);
                offset += 4;
                var channels = BitConverter.ToInt32(arrayData, offset);
                offset += 4;

                var frameData = new byte[arrayData.Length - offset];
                Buffer.BlockCopy(arrayData, offset, frameData, 0, frameData.Length);
                OnAudioFrameReceived?.Invoke(chunkNumber, channels, frameData);
                break;
            default:
                throw new Exception($"Unhandled message type: {messageType}");
        }
    }

    private void OnDisconnectedUDP()
    {
        Debug.Log($"UDP disconnected");
        OnConnectedToCall?.Invoke(false);
    }

    private void OnErrorUDP(ErrorCode error, string message)
    {
        Debug.LogError($"UDP error:  {error.ToString()}\n {message}");

    }
    #endregion

    public async void InitCall()
    {
        _clientUDP.Connect(_applicationServerIp, (ushort)(_networkPortTCP + 1));

        while (_udpId == 0)
        {
            await Task.Delay(100);
        }

        SendInitCall();
    }

    public async void ConnectToCall()
    {
        _clientUDP.Connect(_applicationServerIp, (ushort)(_networkPortTCP + 1));

        while (_udpId == 0)
        {
            await Task.Delay(100);
        }

        SendConnectToCall();
    }

    public async void EndCall()
    {
        OnConnectedToCall?.Invoke(false);
    }

    private void SendInitCall()
    {
        var buffer = new List<byte>();
        buffer.AddRange(BitConverter.GetBytes((int)MessageType.InitCall));
        buffer.AddRange(BitConverter.GetBytes(_udpId));


        SendNetworkMessage(buffer.ToArray());
    }

    private void SendConnectToCall()
    {
        var buffer = new List<byte>();
        buffer.AddRange(BitConverter.GetBytes((int)MessageType.ConnectToCall));
        buffer.AddRange(BitConverter.GetBytes(_udpId));

        SendNetworkMessage(buffer.ToArray());
    }

    public void SendCallFrame(byte[] data, int channels, int chunkNumber)
    {
        var buffer = new List<byte>();
        buffer.AddRange(BitConverter.GetBytes((int)MessageTypeUDP.CALL_FRAME));
        buffer.AddRange(BitConverter.GetBytes(chunkNumber));
        buffer.AddRange(BitConverter.GetBytes(channels));
        buffer.AddRange(data);

        SendNetworkMessageUDP(buffer.ToArray(), KcpChannel.Reliable);
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
    GotModelFromServer = 21,

    InitCall = 50,
    ConnectToCall = 51,
    ConnectedToCall = 52,
    CallRoomListUpdate = 53
}

public enum MessageTypeUDP
{
    HELLO = 0,
    CALL_FRAME = 1
}

public class ModelReceiver
{
    private enum ModelMessageType
    {
        ModelInfo = 0,
        ModelDataPart = 1
    }

    public string ModelName;
    public byte[] FullModelData;

    public Action OnModelReceivedError;

    public bool IsReceiveCompleted { get; private set; } = false;

    public int FullModelLenght { get; private set; }
    public int FullModelOffset { get; private set; } = 0;
    public bool HandleMessage(byte[] data, int offset)
    {
        var msgType = (ModelMessageType)BitConverter.ToInt32(data, offset);
        offset += 4;

        switch (msgType)
        {
            case ModelMessageType.ModelInfo:
                HandleInfo(data, offset);
                break;
            case ModelMessageType.ModelDataPart:
                HandleData(data, offset);
                break;
            default:
                OnModelReceivedError?.Invoke();
                break;
        }

        return true;
    }

    private void HandleInfo(byte[] data, int offset)
    {
        //int nameLenght = BitConverter.ToInt32(data, offset);
        //offset += 4;

        //var nameArray = new byte[nameLenght];
        //Buffer.BlockCopy(data, offset, nameArray, 0, nameLenght);
        //offset += nameLenght;

        //var name = System.Text.Encoding.UTF8.GetString(nameArray);
        var modelLenght = BitConverter.ToInt32(data, offset);


        //ModelName = name;
        FullModelLenght = modelLenght;

        FullModelData = new byte[FullModelLenght];

        Debug.Log($"Start receive model: . Lenght: {FullModelLenght}");
    }

    private void HandleData(byte[] data, int offset)
    {
        var chunkLenght = BitConverter.ToInt32(data, offset);
        offset += 4;

        Buffer.BlockCopy(data, offset, FullModelData, FullModelOffset, chunkLenght);
        FullModelOffset += chunkLenght;

        if (FullModelData.Length == FullModelOffset)
            IsReceiveCompleted = true;

        Debug.Log($"Receiving model: . {FullModelOffset}/{FullModelLenght}");
    }
}