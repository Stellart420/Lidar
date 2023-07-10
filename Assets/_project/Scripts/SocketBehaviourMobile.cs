using HoloGroup.Networking.Internal.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class SocketBehaviourMobile : ICustomTCP
{
    public byte[] DataForSend;

    public Task<bool> ListenerTransferingProcess(NetworkStream networkStream)
    {
        throw new NotImplementedException();
    }

    public void OnListenerOpened(bool successfully)
    {
        throw new NotImplementedException();
    }

    public void OnSocketListenerStartingError(Exception e)
    {
        throw new NotImplementedException();
    }

    public void OnSocketStartingError(Exception e)
    {
        throw e;
    }

    public async Task SocketTransferingProcess(bool connectSuccessful, NetworkStream networkStream = null)
    {
        if (!connectSuccessful)
            throw new Exception("Bad connect to server");


        await networkStream.WriteAsync(DataForSend, 0, DataForSend.Length);
        await networkStream.FlushAsync();

    }
}
