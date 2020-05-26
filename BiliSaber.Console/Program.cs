using System.IO;
using System.IO.Compression;
using BiliSaber.Bilibili;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json.Linq;

namespace BiliSaber.Console {
  class Program {
    private static void OnDanmakuMessage (JObject danmakuJson) {
      var info = danmakuJson["info"]?.Value<JArray>();
      if (info != null) {
        var message = info[1]?.Value<string>() ?? "";
        var username = info[2]?.Value<JArray>()?[1]?.Value<string>() ?? "";
        System.Console.WriteLine($"[{username}]: {message}");
      }
    }

    private static void DealWithChatMessage (string message) {
      var danmakuJson = JObject.Parse(message);
      var cmd = danmakuJson["cmd"]?.Value<string>();
      switch (cmd) {
        case "DANMU_MSG":
          OnDanmakuMessage(danmakuJson);
          break;

        default:
          System.Console.WriteLine(cmd);
          break;
      }
    }


    static void Main (string[] args) {
      var roomId = 21379626;
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
            string jsonString;

            // Version 2 message is compressed by using GZIP.
            if (message.Version == 2) {
              var buffer = message.Buffer;
              var length = buffer.Length;
              const int headerLength = DanmakuPacket.HeaderLength + 2;
              
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

            DealWithChatMessage(jsonString);
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