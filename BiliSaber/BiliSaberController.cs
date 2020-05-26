using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using BiliSaber.Bilibili;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace BiliSaber {
  /// <summary>
  /// Monobehaviours (scripts) are added to GameObjects.
  /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
  /// </summary>
  public class BiliSaberController : MonoBehaviour {
    public static BiliSaberController instance { get; private set; }

    private const int RoomId = 31917;
    private const int Uid = 141042;

    private DanmakuClient _danmakuClient;

    private const int MaxDanmakuMessageCount = 10;
    private readonly List<string> _danmakuMessages = new List<string>();
    private TextMeshPro _textMesh;

    private void UpdateText () {
      if (this._textMesh == null) {
        return;
      }

      var message = "";
      this._danmakuMessages.ForEach(item => {
        if (string.IsNullOrEmpty(message)) {
          message += item;
        } else {
          message += "<br>" + item;
        }
      });
      this._textMesh.SetText(message);
    }

    private void AddMessage (string message) {
      if (this._danmakuMessages.Count >= MaxDanmakuMessageCount) {
        this._danmakuMessages.RemoveAt(0);
      }

      this._danmakuMessages.Add(message);
      this.UpdateText();
    }

    private void OnDanmakuMessage (JObject danmakuJson) {
      var info = danmakuJson["info"]?.Value<JArray>();
      if (info != null) {
        var message = info[1]?.Value<string>() ?? "";
        var username = info[2]?.Value<JArray>()?[1]?.Value<string>() ?? "";
        this.AddMessage($"[{username}]: {message}");
        Logger.Log?.Info($"[DanmakuMessage] {username}: {message}");
      }
    }

    private void OnGiftMessage (JObject danmakuJson) {
      var data = danmakuJson["data"]?.Value<JObject>();
      if (data != null) {
        var username = data["uname"]?.Value<string>() ?? "";
        var giftName = data["giftName"]?.Value<string>() ?? "";
        var giftCount = data["num"]?.Value<int>() ?? 0;
        this.AddMessage($"感谢 {username} 投喂 {giftName} x {giftCount}");
        Logger.Log?.Info($"[GiftMessage] Thanks {username} for sending {giftName} x {giftCount}!");
      }
    }

    private void OnWelcomeMessage (JObject danmakuJson) {
      var data = danmakuJson["data"]?.Value<JObject>();
      if (data != null) {
        var username = data["uname"]?.Value<string>() ?? "";
        this.AddMessage($"欢迎 {username} 进入直播间");
        Logger.Log?.Info($"[WelcomeMessage] Welcome {username} to join in this room");
      }
    }

    private void DealWithChatMessage (string message) {
      var danmakuJson = JObject.Parse(message);
      var cmd = danmakuJson["cmd"]?.Value<string>();
      switch (cmd) {
        case "DANMU_MSG":
          this.OnDanmakuMessage(danmakuJson);
          break;

        case "SEND_GIFT":
          this.OnGiftMessage(danmakuJson);
          break;

        case "WELCOME":
          this.OnWelcomeMessage(danmakuJson);
          break;
      }
    }

    private void WsOnOpen () {
      Logger.Log?.Info($"Danmaku Client Opened, Room {RoomId} as User {Uid}.");
    }

    private void WsOnDanmakuMessage (DanmakuMessage message) {
      Logger.Log?.Info("Get message.");

      switch (message.Operation) {
        case DanmakuOperation.GreetingAck:
          Logger.Log?.Info("Greeting packet has been sent.");
          break;

        case DanmakuOperation.HeartBeatAck:
          Logger.Log?.Info("HeartBeat packet has been sent.");
          break;

        case DanmakuOperation.ChatMessage:
          string jsonString;
          
          // Version 2 message is compressed by using GZIP.
          if (message.Version == 2) {
            var buffer = message.Buffer;
            var length = buffer.Length;
            const int headerLength = DanmakuPacket.HeaderLength;

            var rawContent = new byte[length - headerLength];
            for (var i = headerLength; i < length; i++) {
              rawContent[i - headerLength] = buffer[i];
            }

            byte[] inflatedBytes;
            using (var inflateStream = new InflaterInputStream(new MemoryStream(rawContent))) {
              using (var stream = new MemoryStream()) {
                inflateStream.CopyTo(stream);
                inflatedBytes = stream.ToArray();
              }
            }

            var danmakuMessage = DanmakuMessage.ParseFirstPacket(inflatedBytes);
            jsonString = danmakuMessage.Body;
          } else {
            jsonString = message.Body;
          }

          this.DealWithChatMessage(jsonString);
          break;
      }
    }

    private void WsOnClose () {
      Logger.Log?.Info("Danmaku Client is going to shut down...");
    }

    private void WsOnClosed () {
      this._danmakuClient.OnOpen -= this.WsOnOpen;
      this._danmakuClient.OnDanmakuMessage -= this.WsOnDanmakuMessage;
      this._danmakuClient.OnClose -= this.WsOnClose;
      this._danmakuClient.OnClosed -= this.WsOnClosed;
      this._danmakuClient = null;
      Logger.Log?.Info("Danmaku Client closed.");
    }

    private void InitDanmakuClient () {
      if (this._danmakuClient != null) {
        return;
      }

      var client = new DanmakuClient(RoomId, Uid, true);
      client.OnOpen += this.WsOnOpen ;
      client.OnDanmakuMessage += this.WsOnDanmakuMessage ;
      client.OnClose += this.WsOnClose ;
      client.OnClosed += this.WsOnClosed ;
      client.Connect();
      this._danmakuClient = client;
    }

    private void CloseDanmakuClient () {
      this._danmakuClient?.Close();
    }

    private void InitUi () {
      var canvas = this.gameObject.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.WorldSpace;
      
      var textContainer = new GameObject("DanmakuDialogTextContainer");
      textContainer.transform.SetParent(canvas.transform);
      
      var scrollRect = textContainer.AddComponent<ScrollRect>();
      scrollRect.horizontal = false;
      scrollRect.vertical = true;

      var textMesh = textContainer.AddComponent<TextMeshPro>();
      textMesh.fontSize = 2;
      textMesh.color = Color.white;
      textMesh.enableWordWrapping = true;
      textMesh.richText = true;
      textMesh.font = Resources.Load("Source Han Sans Medium", typeof(TMP_FontAsset)) as TMP_FontAsset;
      textMesh.SetText($"BiliSaber<br>RoomId: {RoomId}");
      textMesh.transform.position = new Vector3(6.2f, 0f, -9.5f);
      textMesh.transform.Rotate(0f, 90f, 0f);

      scrollRect.content = textMesh.rectTransform;
      this._textMesh = textMesh;
      
      var scrollbar = textContainer.AddComponent<Scrollbar>();
      scrollbar.direction = Scrollbar.Direction.BottomToTop;
      scrollRect.verticalScrollbar = scrollbar;
    }

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
      this.InitUi();
      Logger.Log?.Debug($"{this.name}: Awake()");
    }

    /// <summary>
    /// Only ever called once on the first frame the script is Enabled.
    /// Start is called after any other script's Awake() and before Update().
    /// </summary>
    private void Start () {
      Logger.Log?.Debug($"{this.name}: Start()");
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
      this.InitDanmakuClient();
    }

    /// <summary>
    /// Called when the script becomes disabled or when it is being destroyed.
    /// </summary>
    private void OnDisable () {
      this.CloseDanmakuClient();
    }

    /// <summary>
    /// Called when the script is being destroyed.
    /// </summary>
    private void OnDestroy () {
      Logger.Log?.Debug($"{this.name}: OnDestroy()");
      this.CloseDanmakuClient();
      this._danmakuClient = null;
      BiliSaberController.instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
    }
  }
}
