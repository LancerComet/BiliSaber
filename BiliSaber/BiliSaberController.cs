using BiliSaber.Bilibili;
using Newtonsoft.Json.Linq;
using UnityEngine;

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

    private void OnDanmakuMessage (JObject danmakuJson) {
      var info = danmakuJson["info"]?.Value<JArray>();
      if (info != null) {
        var message = info[1]?.Value<string>() ?? "";
        var username = info[2]?.Value<JArray>()?[1]?.Value<string>() ?? "";
        Logger.Log?.Info($"[BiliSaber][DanmakuMessage] {username}: {message}");
      }
    }

    private void OnGiftMessage (JObject danmakuJson) {
      var data = danmakuJson["data"]?.Value<JObject>();
      if (data != null) {
        var username = data["uname"]?.Value<string>() ?? "";
        var giftName = data["giftName"]?.Value<string>() ?? "";
        var giftCount = data["num"]?.Value<int>() ?? 0;
        Logger.Log?.Info($"[BiliSaber][GiftMessage] Thanks {username} for sending {giftName} x {giftCount}!");
      }
    }

    private void OnWelcomeMessage (JObject danmakuJson) {
      var data = danmakuJson["data"]?.Value<JObject>();
      if (data != null) {
        var username = data["uname"]?.Value<string>() ?? "";
        Logger.Log?.Info($"[BiliSaber][WelcomeMessage] Welcome {username} to join in this room");
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

    private void InitDanmakuClient () {
      var client = new DanmakuClient(RoomId, Uid, true);

      client.OnOpen += () => {
        Logger.Log?.Info("[BiliSaber] Danmaku Client Opened.");
      };

      client.OnDanmakuMessage += message => {
        Logger.Log?.Info("[BiliSaber] Get message.");

        switch (message.Operation) {
          case DanmakuOperation.GreetingAck:
            Logger.Log?.Info("[BiliSaber] Greeting packet has been sent.");
            break;

          case DanmakuOperation.HeartBeatAck:
            Logger.Log?.Info("[BiliSaber] HeartBeat packet has been sent.");
            break;

          case DanmakuOperation.ChatMessage:
            this.DealWithChatMessage(message.Body);
            break;
        }
      };

      client.Connect();
      this._danmakuClient = client;
    }

    private void DestroyDanmakuClient () {
      if (this._danmakuClient != null) {
        this._danmakuClient.Close();
        this._danmakuClient = null;
      }
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
      this.DestroyDanmakuClient();
      this.InitDanmakuClient();
    }

    /// <summary>
    /// Called when the script becomes disabled or when it is being destroyed.
    /// </summary>
    private void OnDisable () {
      this.DestroyDanmakuClient();
    }

    /// <summary>
    /// Called when the script is being destroyed.
    /// </summary>
    private void OnDestroy () {
      Logger.Log?.Debug($"{this.name}: OnDestroy()");
      this._danmakuClient = null;
      BiliSaberController.instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
    }
  }
}
