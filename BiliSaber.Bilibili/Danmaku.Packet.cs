using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BiliSaber.Bilibili {
  public class DanmakuPacket {
    public const int HeaderLength = 16;
    public const int PacketOffset = 0;
    public const int HeaderOffset = 4;
    public const int VersionOffset = 6;
    public const int OperationOffset = 8;
    public const int SequenceOffset = 12;

    public byte[] PacketBuffer { get; private set; }

    /// <summary>
    /// Create Packet.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="operation"></param>
    /// <param name="sequence"></param>
    /// <param name="body"></param>
    private void CreatePacket (int version, DanmakuOperation operation, int sequence, string body) {
      var headerBytes = new byte[HeaderLength];
      var bodyBuffer = Encoding.UTF8.GetBytes(body);
      DataView.SetInt32(headerBytes, PacketOffset, HeaderLength + bodyBuffer.Length);
      DataView.SetInt16(headerBytes, HeaderOffset, HeaderLength);
      DataView.SetInt16(headerBytes, VersionOffset, version);
      DataView.SetInt32(headerBytes, OperationOffset, (int)operation);
      DataView.SetInt32(headerBytes, SequenceOffset, sequence);

      var packetBuffer = DataView.MergeBytes(new List<byte[]> {
        headerBytes, bodyBuffer
      });
      this.PacketBuffer = packetBuffer;
    }

    /// <summary>
    /// Constructor of DanmakuPacket.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="operation"></param>
    /// <param name="sequence"></param>
    /// <param name="body"></param>
    public DanmakuPacket (int version, DanmakuOperation operation, int sequence, string body) {
      this.CreatePacket(version, operation, sequence, body);
    }

    /// <summary>
    /// Create greeting packet.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="roomId"></param>
    /// <returns></returns>
    public static DanmakuPacket CreateGreetingPacket (int uid, int roomId) {
      return new DanmakuPacket(
        1, DanmakuOperation.GreetingReq, 1,
        JsonConvert.SerializeObject(new {
          uid = uid,
          roomid = roomId
        })
      );
    }

    /// <summary>
    /// Create HeartBeat Packet..
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="roomId"></param>
    /// <returns></returns>
    public static DanmakuPacket CreateHeartBeatPacket (int uid, int roomId) {
      return new DanmakuPacket(
        1, DanmakuOperation.HeartBeatReq, 1,
        JsonConvert.SerializeObject(new {
          uid = uid,
          roomid = roomId
        })
      );
    }
  }
}
