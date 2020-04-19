using System;
using System.Timers;
using WebSocketSharp;

namespace BiliSaber.Bilibili {
  public class DanmakuClient {
    private readonly int _roomId;
    private readonly int _uid;
    private readonly bool _useWss;
    private WebSocket _ws;
    private Timer _timer;

    public delegate void OnOpenHandler ();
    public delegate void OnCloseHandler ();
    public delegate void OnClosedHandler ();
    public delegate void OnMessageHandler<in T> (T message);
    public delegate void OnErrorHandler (Exception error);

    public event OnOpenHandler OnOpen;
    public event OnCloseHandler OnClose;
    public event OnClosedHandler OnClosed;
    public event OnMessageHandler<string> OnStringMessage;
    public event OnMessageHandler<DanmakuMessage> OnDanmakuMessage; 
    public event OnErrorHandler OnError;

    private void SendGreetingPacket () {
      var packet = DanmakuPacket.CreateGreetingPacket(this._uid, this._roomId);
      this.Send(packet.PacketBuffer);
    }

    private void SendHeartBeatPacket() {
      var packet = DanmakuPacket.CreateHeartBeatPacket(this._uid, this._roomId);
      this.Send(packet.PacketBuffer);
    }

    private void StartHeartBeat () {
      if (this._timer != null) {
        return;
      }

      var timer = new Timer() {
        Interval = 1000 * 30
      };

      timer.Elapsed += (sender, args) => {
        this.SendHeartBeatPacket();
      };

      timer.Start();
      this._timer = timer;
    }

    private void StopHeartBeat() {
      this._timer?.Stop();
      this._timer = null;
    }

    #region WebSocket Event Handlers.

    private void WsOnOpen (object sender, EventArgs e) {
      this.OnOpen?.Invoke();
      this.SendGreetingPacket();
    }

    private void WsOnClose (object sender, CloseEventArgs e) {
      this.OnClosed?.Invoke();
    }

    private void WsOnMessage (object sender, MessageEventArgs e) {
      if (e.IsText) {
        this.OnStringMessage?.Invoke(e.Data);
        return;
      }

      if (e.IsBinary) {
        var buffer = new byte[e.RawData.Length];
        e.RawData.CopyTo(buffer, 0);

        do {
          var message = DanmakuMessage.ParseFirstPacket(buffer);

          // Receive the greeting ack notify, then a HeartBeat timer should be setup.
          if (message.Operation == DanmakuOperation.GreetingAck) {
            this.StartHeartBeat();
          }

          this.OnDanmakuMessage?.Invoke(message);
          DataView.ByteSlice(ref buffer, message.PacketLength);
        } while (buffer.Length > 0);
      }
    }

    private void WsOnError (object sender, ErrorEventArgs e) {
      this.OnError?.Invoke(e.Exception);
    }

    #endregion

    /// <summary>
    /// Connect to Bilibili Live Danmaku Websocket server.
    /// </summary>
    public void Connect () {
      if (this._ws == null) {
        var url = $"{(this._useWss ? "wss" : "ws")}://broadcastlv.chat.bilibili.com:{(this._useWss ? 2245 : 2244)}/sub";
        var ws = new WebSocket(url);
        ws.OnOpen += this.WsOnOpen;
        ws.OnMessage += this.WsOnMessage;
        ws.OnClose += this.WsOnClose;
        ws.OnError += this.WsOnError;
        this._ws = ws;
      }
     
      this._ws.Connect();
    }

    /// <summary>
    /// Disconnect from server.
    /// </summary>
    public void Close () {
      this.StopHeartBeat();
      this._ws?.Close();
      this.OnClose?.Invoke();
    }

    /// <summary>
    /// Send data to server.
    /// </summary>
    /// <param name="bytes"></param>
    public void Send (byte[] bytes) {
      this._ws?.Send(bytes);
    }

    public DanmakuClient (int roomId, int uid, bool useWss) {
      this._roomId = roomId;
      this._uid = uid;
      this._useWss = useWss;
    }
  }
}