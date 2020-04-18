using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace BiliSaber {
  /// <summary>
  /// Monobehaviours (scripts) are added to GameObjects.
  /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
  /// </summary>
  public class BiliSaberController : MonoBehaviour {
    public static BiliSaberController instance { get; private set; }

    private const int RoomId = 31917;
    private WebSocket _ws;

    /// <summary>
    /// 关闭 WS 连接.
    /// </summary>
    private void WsDisconnect () {
      this._ws?.Close();
    }

    /// <summary>
    /// 初始化 WebSocket.
    /// </summary>
    private void WsConnect () {
      if (this._ws != null) {
        this.WsDisconnect();
      }

      var ws = new WebSocket("");
      ws.OnOpen += this.OnWsOpen;
      ws.OnClose += this.OnWsClose;
      ws.OnMessage += this.OnWsMessage;
      this._ws = ws;
    }

    private void OnWsOpen (object sender, EventArgs e) {
      Logger.Log?.Info("Websocket opened!");
    }

    private void OnWsMessage (object sender, MessageEventArgs e) {
    }

    private void OnWsClose(object sender, CloseEventArgs e) {
      Logger.Log?.Info("Bilibili Live Danmaku Websocket closed.");
      this._ws = null;
    }

    /// <summary>
    /// Awake 是 Unity MonoBehaviour 的生命周期.
    /// 在加载场景时运行, 即在游戏开始之前初始化变量或者游戏状态, 只执行一次.
    /// </summary>
    private void Awake () {
      // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
      // and destroy any that are created while one already exists.
      if (BiliSaberController.instance != null) {
        Logger.Log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
        GameObject.DestroyImmediate(this);
        return;
      }
      GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
      BiliSaberController.instance = this;
      Logger.Log?.Debug($"{this.name}: Awake()");
    }

    /// <summary>
    /// Only ever called once on the first frame the script is Enabled.
    /// Start is called after any other script's Awake() and before Update().
    /// </summary>
    private void Start () {
      Logger.Log?.Debug($"{this.name}: Start()");
      this.WsConnect();
    }

    /// <summary>
    /// Called every frame if the script is enabled.
    /// </summary>
    private void Update () {

    }

    /// <summary>
    /// Called every frame after every other enabled script's Update().
    /// </summary>
    private void LateUpdate () {

    }

    /// <summary>
    /// Called when the script becomes enabled and active
    /// </summary>
    private void OnEnable () {

    }

    /// <summary>
    /// Called when the script becomes disabled or when it is being destroyed.
    /// </summary>
    private void OnDisable () {

    }

    /// <summary>
    /// Called when the script is being destroyed.
    /// </summary>
    private void OnDestroy () {
      Logger.Log?.Debug($"{this.name}: OnDestroy()");
      BiliSaberController.instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
    }
  }
}
