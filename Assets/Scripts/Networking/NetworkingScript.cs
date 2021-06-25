using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class NetworkingScript : MonoBehaviour
{
    #region private members 	

    // TCPListener to listen for incomming TCP connection requests. 	
    private TcpListener tcpListener;
    // Background thread for TcpServer workload. 	
    private Thread tcpListenerThread;
    // Create handle to connected tcp client. 	
    private TcpClient connectedTcpClient;
    // Network Address info
    private string addres = "127.0.0.1";
    private int port = 8052;

    // Last message received by the client
    string LastMessage = null;
    #endregion

    void OnApplicationQuit()
    {
        StopNetworking();
    }

    public void StartNetworking()
    {
        UnityEngine.Debug.Log("Start Networking on Address: " + addres + ", via port: " + port.ToString());
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    public void StopNetworking()
    {
        UnityEngine.Debug.Log("Stopped Networking");
        tcpListenerThread.Abort();
    }

    public void SetNetworkingSettings(ConfigurationSettings settings)
    {
        addres = settings.address;
        port = settings.port;
    }

    //Retrieve latest server message
    public string GetLatestMessage()
    {
        return LastMessage;
    }

    //Reset latest server message
    public void ResetLastMessage()
    {
        LastMessage = "";
    }

    //Send new message 
    public void SendMessageToServer(string message)
    {
        SendMessage(message);
    }

    // Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on specified port. 			
            tcpListener = new TcpListener(IPAddress.Parse(addres), port);
            tcpListener.Start();
            //Debug.Log("Server is listening");
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 						
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 							
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            UnityEngine.Debug.Log("client message received as: " + clientMessage);
                            LastMessage = clientMessage;
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            UnityEngine.Debug.Log("SocketException " + socketException.ToString());
        }
    }

    // Send message to client using socket connection. 	
    private void SendMessage(string message)
    {
        UnityEngine.Debug.Log(message);

        if (connectedTcpClient == null)
        {
            UnityEngine.Debug.Log("No TCP Clients Connected");
            return;
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
                string serverMessage = message;
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                UnityEngine.Debug.Log("Server sent his message - should be received by client");
            }
        }
        catch (Exception socketException)
        {
            UnityEngine.Debug.LogError("Socket exception: " + socketException);
        }
    }

    public bool IsClientConnected()
    {
        IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

        TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

        foreach (TcpConnectionInformation c in tcpConnections)
        {
            TcpState stateOfConnection = c.State;

            if (c.LocalEndPoint.Equals(connectedTcpClient.Client.LocalEndPoint) && c.RemoteEndPoint.Equals(connectedTcpClient.Client.RemoteEndPoint))
            {
                if (stateOfConnection == TcpState.Established)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

        }
        return false;
    }
}
