using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RXHDDT.Util
{
  public class ReplayHelper
  {
    private static ReplayReader _reader;

    public static ReplayFile ReadFile(string filePath)
    {
      ReplayFile replayFile = new ReplayFile();
      using (_reader = new ReplayReader(File.Open(filePath, FileMode.Open)))
      {
        replayFile.Passed = true;
        replayFile.Mode = _reader.ReadByte();
        replayFile.Version = _reader.ReadInt32();
        replayFile.BeatmapHash = _reader.ReadString();
        replayFile.PlayerName = _reader.ReadString();
        replayFile.ReplayHash = _reader.ReadString();
        replayFile.Count300 = _reader.ReadUInt16();
        replayFile.Count100 = _reader.ReadUInt16();
        replayFile.Count50 = _reader.ReadUInt16();
        replayFile.CountGeki = _reader.ReadUInt16();
        replayFile.CountKatu = _reader.ReadUInt16();
        replayFile.CountMiss = _reader.ReadUInt16();
        replayFile.Score = _reader.ReadInt32();
        replayFile.MaxCombo = _reader.ReadUInt16();
        replayFile.FullCombo = _reader.ReadBoolean();
        replayFile.UsedMods = _reader.ReadInt32();
        replayFile.PerformanceGraphData = _reader.ReadString();
        replayFile.ReplayDate = _reader.ReadDateTime();
        replayFile.Replay = _reader.ReadByteArray();
        if (replayFile.Version >= 20140721)
          replayFile.Long0 = _reader.ReadInt64();
      }

      return replayFile;
    }

    public static void SaveFile(string filePath, ReplayFile replay)
    {
      using (ReplayWriter beatmapWriter = new ReplayWriter(File.Open(filePath, FileMode.Create)))
      {
        beatmapWriter.Write(replay.Mode);
        beatmapWriter.Write(replay.Version);
        beatmapWriter.Write(replay.BeatmapHash);
        beatmapWriter.Write(replay.PlayerName);
        beatmapWriter.Write(replay.ReplayHash);
        beatmapWriter.Write(replay.Count300);
        beatmapWriter.Write(replay.Count100);
        beatmapWriter.Write(replay.Count50);
        beatmapWriter.Write(replay.CountGeki);
        beatmapWriter.Write(replay.CountKatu);
        beatmapWriter.Write(replay.CountMiss);
        beatmapWriter.Write(replay.Score);
        beatmapWriter.Write(replay.MaxCombo);
        beatmapWriter.Write(replay.FullCombo);
        beatmapWriter.Write(replay.UsedMods);
        beatmapWriter.Write(replay.PerformanceGraphData);
        beatmapWriter.Write(replay.ReplayDate);
        string[] list = Encoding.ASCII.GetString(SevenZipHelper.Decompress(replay.Replay)).Split(',');
        beatmapWriter.Write(replay.Replay);
        beatmapWriter.Write(replay.Long0);
      }
    }

    public static string GetReplayHash(ReplayFile replay)
    {
      return OsuHelper.HashString(string.Format("{0}p{1}o{2}o{3}t{4}a{5}r{6}e{7}y{8}o{9}u{10}{11}{12}", replay.Count100 + replay.Count300, replay.Count50, replay.CountGeki, replay.CountKatu, replay.CountMiss, replay.BeatmapHash, replay.MaxCombo, replay.FullCombo, replay.PlayerName, replay.Score, replay.Ranking, replay.UsedMods, replay.Passed));
    }
  }

  public struct ReplayFile
  {
    public byte Mode;
    public int Version;
    public string BeatmapHash;
    public string PlayerName;
    public string ReplayHash;
    public ushort Count300;
    public ushort Count100;
    public ushort Count50;
    public ushort CountGeki;
    public ushort CountKatu;
    public ushort CountMiss;
    public int Score;
    public ushort MaxCombo;
    public bool FullCombo;
    public int UsedMods;
    public string PerformanceGraphData;
    public DateTime ReplayDate;
    public byte[] Replay;
    public long Long0;
    public OsuHelper.Rankings Ranking;
    public bool Passed;

    public override string ToString()
    {
      Type type = GetType();
      FieldInfo[] fields = type.GetFields();
      PropertyInfo[] properties = type.GetProperties();
      ReplayFile user = this;
      Dictionary<string, object> values = new Dictionary<string, object>();
      Array.ForEach(fields, field => values.Add(field.Name, field.GetValue(user)));
      Action<PropertyInfo> action = property =>
      {
        if (!property.CanRead)
          return;

        values.Add(property.Name, property.GetValue(user, null));
      };
      Array.ForEach(properties, action);
      return string.Join(", ", values);
    }
  }
}
