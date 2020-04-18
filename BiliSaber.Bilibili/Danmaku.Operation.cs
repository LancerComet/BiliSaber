namespace BiliSaber.Bilibili {
  /// <summary>
  /// Operation type in Bilibili Live Danmaku Protocol.
  /// </summary>
  public enum DanmakuOperation {
    // Send HeartBeat packet to server.
    HeartBeatReq = 2,

    // Server has got the HeartBeat packet successfully.
    HeartBeatAck = 3,

    // Chat message from server.
    ChatMessage = 5,

    // Send greeting request to server.
    GreetingReq = 7,

    // Server has got the Greeting packet successfully.
    GreetingAck = 8
  }
}
