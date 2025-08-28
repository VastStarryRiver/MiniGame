using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

#elif MINIGAME_SUBPLATFORM_WEIXIN
using WeChatWASM;

#elif MINIGAME_SUBPLATFORM_DOUYIN
using TTSDK;
#endif



public class MessageNetManager : Singleton<MessageNetManager>
{
    private string serverUrl = "wss://";
    private int serverPort = 0;
    private Dictionary<string, List<Action<string>>> messageCallBacks = new Dictionary<string, List<Action<string>>>();

#if UNITY_EDITOR
    private ClientWebSocket clientWebSocket;
    private CancellationTokenSource cancellationTokenSource;

#elif MINIGAME_SUBPLATFORM_WEIXIN
    private WXTCPSocket wXTCPSocket;

#elif MINIGAME_SUBPLATFORM_DOUYIN
    private TTUDPSocket tTUDPSocket;
#endif



    public void BindReceiveMessage(string messageName, Action<string> callBack)
    {
        if (!messageCallBacks.ContainsKey(messageName))
        {
            messageCallBacks[messageName] = new List<Action<string>>();
        }

        if (!messageCallBacks[messageName].Contains(callBack))
        {
            messageCallBacks[messageName].Add(callBack);
        }
    }

    public void UnbindReceiveMessage(string messageName, Action<string> callBack)
    {
        if (messageCallBacks.ContainsKey(messageName) && messageCallBacks[messageName].Contains(callBack))
        {
            messageCallBacks[messageName].Remove(callBack);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 连接服务器
    /// </summary>
    public async void Play()
    {
        try
        {
            cancellationTokenSource = new CancellationTokenSource();
            clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(new Uri(serverUrl), cancellationTokenSource.Token);
            ConnectSuccessBackCall();
        }
        catch (Exception ex)
        {
            Debug.LogError($"连接失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 客户端断开连接
    /// </summary>
    public async void Stop()
    {
        if (clientWebSocket != null)
        {
            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "用户关闭", cancellationTokenSource.Token);
            clientWebSocket.Dispose();
        }

        cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// 发送数据到服务器端
    /// </summary>
    /// <param name="text"></param>
    public async void Send(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        if (clientWebSocket?.State == WebSocketState.Open)
        {
            await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
            Debug.Log("客户端数据发送成功!");
        }
    }

#elif MINIGAME_SUBPLATFORM_WEIXIN
    public void Play()
    {
        if (wXTCPSocket == null)
        {
            wXTCPSocket = WX.CreateTCPSocket();

            wXTCPSocket.OnConnect((result) =>
            {
                if (string.IsNullOrEmpty(result.errMsg))
                {
                    ConnectSuccessBackCall();
                }
                else
                {
                    Debug.LogError($"连接失败: {result.errMsg}");
                }
            });

            wXTCPSocket.OnMessage((result) =>
            {
                ReceiveMessagesBackCall(result.message, result.message.Length);
            });
        }

        wXTCPSocket.Connect(new TCPSocketConnectOption() { address = serverUrl, port = serverPort, timeout = 60000 });
    }

    public void Stop()
    {
        if (wXTCPSocket != null)
        {
            wXTCPSocket.Close();
        }
    }

    public void Send(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        wXTCPSocket.Write(bytes);
        Debug.Log("客户端数据发送成功!");
    }

#elif MINIGAME_SUBPLATFORM_DOUYIN
    public void Play()
    {
        if(tTUDPSocket == null)
        {
            tTUDPSocket = TT.CreateUDPSocket();

            tTUDPSocket.OnMessage((result) =>
            {
                ReceiveMessagesBackCall(result.message, result.message.Length);
            });
        }

        try
        {
            tTUDPSocket.Connect(new UDPSocketConnectOption() { address = serverUrl, port = serverPort });
        }
        catch (Exception ex)
        {
            Debug.LogError($"连接失败: {ex.Message}");
        }
    }

    public void Stop()
    {
        if(tTUDPSocket != null)
        {
            tTUDPSocket.Close();
        }
    }

    public void Send(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        tTUDPSocket.Send(new UDPSocketSendOption() { address = serverUrl, port = serverPort, message = bytes });
        Debug.Log("客户端数据发送成功!");
    }
#endif

    /// <summary>
    /// 服务器连接成功的回调
    /// </summary>
    private void ConnectSuccessBackCall()
    {
        Debug.Log("服务器连接成功");

#if UNITY_EDITOR
        _ = ReceiveMessages();
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// 开始接收服务器数据
    /// </summary>
    private async Task ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        while (clientWebSocket?.State == WebSocketState.Open)
        {
            var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                ReceiveMessagesBackCall(buffer, result.Count);
            }
        }
    }
#endif

    /// <summary>
    /// 接收服务器数据的回调
    /// </summary>
    private void ReceiveMessagesBackCall(byte[] buffer, int count)
    {
        string message = Encoding.UTF8.GetString(buffer, 0, count);

        Debug.Log($"收到消息: {message}");

        if (message.Contains("|"))
        {
            string[] strings = message.Split('|');

            if (messageCallBacks.ContainsKey(strings[0]) && messageCallBacks[strings[0]].Count > 0)
            {
                foreach (Action<string> callBack in messageCallBacks[strings[0]])
                {
                    callBack.Invoke(strings[1]);
                }
            }
        }
    }
}