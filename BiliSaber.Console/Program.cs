using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using BiliSaber.Bilibili;

namespace BiliSaber.Console {
  class Program {
    static void Main (string[] args) {
      var roomId = 31917;
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
              using (var deflatedStream = new GZipStream(new MemoryStream(rawContent), CompressionMode.Decompress)) {
                using (var stream = new MemoryStream()) {
                  deflatedStream.CopyTo(stream);
                  inflatedBytes = stream.ToArray();
                }
              }

              var reg = new Regex("\\{.+");
              var jsonMatch = reg.Match(Encoding.UTF8.GetString(inflatedBytes));
              if (!jsonMatch.Success) {
                return;
              }

              var jsonString = jsonMatch.Value; 
              System.Console.WriteLine(jsonString);
            } else {
              System.Console.WriteLine(message.Body);
            }
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