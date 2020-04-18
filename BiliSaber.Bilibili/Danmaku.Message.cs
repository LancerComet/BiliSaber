using System.Text;

namespace BiliSaber.Bilibili {
  /// <summary>
  /// DanmakuMessage presents the message from the remote.
  /// It contains the data from server.
  /// </summary>
  public class DanmakuMessage {
    public int PacketLength { get; private set; }
    public int HeaderLength { get; private set; }
    public int Version { get; private set; }
    public DanmakuOperation Operation { get; private set; }
    public int Sequence { get; private set; }
    public string Body { get; private set; }

    public static DanmakuMessage ParseFirstPacket (byte[] buffer) {
      var packetLength = DataView.GetInt32(buffer);
      var headerLength = DataView.GetInt16(buffer, DanmakuPacket.HeaderOffset);
      var version = DataView.GetInt16(buffer, DanmakuPacket.VersionOffset);
      var operation = DataView.GetInt32(buffer, DanmakuPacket.OperationOffset);
      var sequence = DataView.GetInt32(buffer, DanmakuPacket.SequenceOffset);
      var body = Encoding.UTF8.GetString(buffer, DanmakuPacket.HeaderLength, packetLength - DanmakuPacket.HeaderLength);
      return new DanmakuMessage() {
        PacketLength = packetLength,
        HeaderLength = headerLength,
        Version = version,
        Operation = (DanmakuOperation)operation,
        Sequence = sequence,
        Body = body
      };
    }
  }
}
