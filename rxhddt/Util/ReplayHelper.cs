// Decompiled with JetBrains decompiler
// Type: osu_ftw.ReplayHelper
// Assembly: osu!ftw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 43BAB63D-DE2A-4E38-86D4-2EF193F865F8
// Assembly location: C:\Users\Niklas\Downloads\aac_Portable\osu!\osu!ftw.exe

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
    private static BeatmapReader _reader;

    public static ReplayFile ReadFile(string filePath)
    {
      ReplayFile replayFile = new ReplayFile();
      using (ReplayHelper._reader = new BeatmapReader((Stream)File.Open(filePath, FileMode.Open)))
      {
        replayFile.Passed = true;
        replayFile.Mode = ReplayHelper._reader.ReadByte();
        replayFile.Version = ReplayHelper._reader.ReadInt32();
        replayFile.BeatmapHash = ReplayHelper._reader.ReadString();
        replayFile.PlayerName = ReplayHelper._reader.ReadString();
        replayFile.ReplayHash = ReplayHelper._reader.ReadString();
        replayFile.Count300 = ReplayHelper._reader.ReadUInt16();
        replayFile.Count100 = ReplayHelper._reader.ReadUInt16();
        replayFile.Count50 = ReplayHelper._reader.ReadUInt16();
        replayFile.CountGeki = ReplayHelper._reader.ReadUInt16();
        replayFile.CountKatu = ReplayHelper._reader.ReadUInt16();
        replayFile.CountMiss = ReplayHelper._reader.ReadUInt16();
        replayFile.Score = ReplayHelper._reader.ReadInt32();
        replayFile.MaxCombo = ReplayHelper._reader.ReadUInt16();
        replayFile.FullCombo = ReplayHelper._reader.ReadBoolean();
        replayFile.UsedMods = ReplayHelper._reader.ReadInt32();
        replayFile.PerformanceGraphData = ReplayHelper._reader.ReadString();
        replayFile.ReplayDate = ReplayHelper._reader.ReadDateTime();
        replayFile.Replay = ReplayHelper._reader.ReadByteArray();
        if (replayFile.Version >= 20140721)
          replayFile.Long0 = ReplayHelper._reader.ReadInt64();
      }
      return replayFile;
    }

    public static void SaveFile(string filePath, ReplayFile replay)
    {
      using (BeatmapWriter beatmapWriter = new BeatmapWriter((Stream)File.Open(filePath, FileMode.Create)))
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
        string[] list = Encoding.ASCII.GetString(SevenZipHelper.Decompress(replay.Replay)).Split(','));
        beatmapWriter.Write(replay.Replay);
        beatmapWriter.Write(replay.Long0);
      }
    }

    public static string GetReplayHash(ReplayFile replay)
    {
      return OsuHelper.HashString(string.Format("{0}p{1}o{2}o{3}t{4}a{5}r{6}e{7}y{8}o{9}u{10}{11}{12}", (object)((int)replay.Count100 + (int)replay.Count300), (object)replay.Count50, (object)replay.CountGeki, (object)replay.CountKatu, (object)replay.CountMiss, (object)replay.BeatmapHash, (object)replay.MaxCombo, (object)replay.FullCombo, (object)replay.PlayerName, (object)replay.Score, (object)replay.Ranking, (object)replay.UsedMods, (object)replay.Passed));
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
      Type type = this.GetType();
      FieldInfo[] fields = type.GetFields();
      PropertyInfo[] properties = type.GetProperties();
      ReplayFile user = this;
      Dictionary<string, object> values = new Dictionary<string, object>();
      Array.ForEach<FieldInfo>(fields, (Action<FieldInfo>)(field => values.Add(field.Name, field.GetValue((object)user))));
      Action<PropertyInfo> action = (Action<PropertyInfo>)(property =>
      {
        if (!property.CanRead)
          return;
        values.Add(property.Name, property.GetValue((object)user, (object[])null));
      });
      Array.ForEach<PropertyInfo>(properties, action);
      return string.Join<KeyValuePair<string, object>>(", ", (IEnumerable<KeyValuePair<string, object>>)values);
    }
  }
}
