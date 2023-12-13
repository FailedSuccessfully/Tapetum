using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;
using System.Text;
using UnityEngine.Events;

public class UDPReceiver : MonoBehaviour
{
    private string ipAdress = "127.0.0.1";
    [SerializeField]private int listenPort = 6100;
    [SerializeField] UnityEvent<string> eventListeners;

    Thread listener;
    static Queue incomingQ = Queue.Synchronized(new Queue());
    static UdpClient udpClient;
    private IPEndPoint endPoint;
    bool isListening;
    // Start is called before the first frame update
    void Start()
    {
        StartUDP();
    }

    // Update is called once per frame
    void Update()
    {
        lock (incomingQ.SyncRoot) { 
            if (incomingQ.Count > 0)
            {
                string s = incomingQ.Dequeue().ToString();
                // handle here
                eventListeners.Invoke(s);   
            }
        }
    }

    private void OnDestroy()
    {
        EndUDP();
    }

    void StartUDP() { 
        endPoint = new IPEndPoint(IPAddress.Parse(ipAdress), listenPort);
        udpClient = new UdpClient(endPoint);
        
        Debug.Log($"Listening on port: {listenPort}");
        listener = new Thread(new ThreadStart(MessageHandler));
        listener.Start();

    }

    void MessageHandler()
    {
        Debug.Log("handler active");
        isListening = true;
        byte[] buffer = new byte[0];
        while (isListening)
        {
            try
            {
                buffer = udpClient.Receive(ref endPoint);
            }
            catch (Exception e)
            {
                Debug.Log($"UDP Receive Error: {e}");
                Debug.Log(e.StackTrace);
                udpClient.Close();
                isListening = false;
                return;
            }

            string incoming = Encoding.ASCII.GetString(buffer);
            incomingQ.Enqueue(incoming);
        }
    }

    void EndUDP()
    {
        listener?.Abort();
        udpClient?.Close();
        isListening= false;
    }
}

