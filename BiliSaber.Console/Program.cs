using BiliSaber.Bilibili;

namespace BiliSaber.Console {
  class Program {
    static void Main (string[] args) {
      var roomId = 14275133;
      var uid = 141042;

      var client = new DanmakuClient(roomId, uid, true);

      client.OnOpen += () => {
        System.Console.WriteLine("Opened.");
      };

      client.OnStringMessage += message => {
        System.Console.WriteLine("OnStringMessage:", message);
      };

      client.OnDanmakuMessage += message => {
        System.Console.WriteLine("Get message:");

        switch (message.Operation) {
          case DanmakuOperation.GreetingAck:
            System.Console.WriteLine("Greeting packet has been sent.");
            break;

          case DanmakuOperation.HeartBeatAck:
            System.Console.WriteLine("HeartBeat packet has been sent.");
            break;

          case DanmakuOperation.ChatMessage:
            System.Console.WriteLine(message.Body);
            break;
        }
      };

      client.Connect();

      while (true) {
        // ...
      }
    }
  }
}