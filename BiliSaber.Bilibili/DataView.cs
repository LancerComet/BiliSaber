using System.Collections.Generic;
using System.Linq;

namespace BiliSaber.Bilibili {
  public static class DataView {
    /// <summary>
    /// Read an Int16 from byte array;
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static int GetInt16 (byte[] bytes, int offset = 0) {
      return ((bytes[offset] & 0xff) << 8) | (bytes[offset + 1] & 0xff);
    }

    /// <summary>
    /// Read an Int32 from byte array.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static int GetInt32 (byte[] bytes, int offset = 0) {
      return ((bytes[offset] & 0xff) << 24) | ((bytes[offset + 1] & 0xff) << 16) | ((bytes[offset + 2] & 0xff) << 8) | (bytes[offset + 3] & 0xff);
    }

    /// <summary>
    /// Set an Int16 to the bytes.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    public static void SetInt16 (byte[] bytes, int offset, int value) {
      bytes[offset] = (byte)((value & 0xff00) >> 8);
      bytes[offset + 1] = (byte)(value & 0x00ff);
    }

    /// <summary>
    /// Set an Int32 to the bytes.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    public static void SetInt32 (byte[] bytes, int offset, int value) {
      bytes[offset] = (byte)((value & 0xff000000) >> 24);
      bytes[offset + 1] = (byte)((value & 0x00ff0000) >> 16);
      bytes[offset + 2] = (byte)((value & 0x0000ff00) >> 8);
      bytes[offset + 3] = (byte)(value & 0x000000ff);
    }

    /// <summary>
    /// Merge a branch of byte array into single one.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] MergeBytes (IEnumerable<byte[]> bytes) {
      var totalLength = bytes.Sum(buffer => buffer.Length);
      var result = new byte[totalLength];
      var offset = 0;

      foreach (var buffer in bytes) {
        for (var i = 0; i < buffer.Length; i++) {
          result[offset + i] = buffer[i];
        }
        offset += buffer.Length;
      }

      return result;
    }

    public static void ByteSlice (ref byte[] bytes, int startIndex) {
      var slice = new byte[bytes.Length - startIndex];
      var j = 0;
      for (var i = startIndex; i < bytes.Length; i++) {
        slice[j] = bytes[i];
        j++;
      }
      bytes = slice;
    }
  }
}
