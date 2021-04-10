using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RXHDDT.Util
{
  public class OsuHelper
  {
    public static int OsuVersion = 20150305;

    public static string HashString(string input)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(input);
      MD5 md5 = MD5.Create();
      byte[] hash;
      try
      {
        hash = md5.ComputeHash(bytes);
      }
      catch
      {
        return "fail";
      }
      char[] destination = new char[hash.Length * 2];
      for (int index = 0; index < hash.Length; ++index)
        hash[index].ToString("x2", new CultureInfo("en-US", false).NumberFormat).CopyTo(0, destination, index * 2, 2);
      return new string(destination);
    }

    public static string HashFile(string fileName)
    {
      MD5 md5 = MD5.Create();
      lock (md5)
      {
        if (!File.Exists(fileName))
          return string.Empty;
        try
        {
          using (Stream inputStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
          {
            byte[] hash = md5.ComputeHash(inputStream);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte num in hash)
              stringBuilder.Append(num.ToString("x2"));
            return stringBuilder.ToString();
          }
        }
        catch
        {
          return string.Empty;
        }
      }
    }

    private static int GetHits(ReplayFile rf)
    {
      switch (rf.Mode)
      {
        case 2:
          return rf.Count50 + rf.Count100 + rf.Count300;
        case 3:
          return rf.Count50 + rf.Count100 + rf.Count300 + rf.CountGeki + rf.CountKatu;
        default:
          return 0;
      }
    }

    private static int GetTotalHits(ReplayFile rf)
    {
      switch (rf.Mode)
      {
        case 2:
          return rf.Count50 + rf.Count100 + rf.Count300 + rf.CountMiss + rf.CountKatu;
        case 3:
          return rf.Count50 + rf.Count100 + rf.Count300 + rf.CountMiss + rf.CountGeki + rf.CountKatu;
        default:
          return rf.Count50 + rf.Count100 + rf.Count300 + rf.CountMiss;
      }
    }

    public static float GetAccuracy(ReplayFile rf)
    {
      switch (rf.Mode)
      {
        case 1:
          if (GetTotalHits(rf) <= 0)
            return 0.0f;
          return (rf.Count100 * 150 + rf.Count300 * 300) / (float)(GetTotalHits(rf) * 300);
        case 2:
          if (GetTotalHits(rf) == 0)
            return 1f;
          return GetHits(rf) / GetTotalHits(rf);
        case 3:
          if (GetTotalHits(rf) == 0)
            return 1f;
          return (rf.Count50 * 50 + rf.Count100 * 100 + rf.CountKatu * 200 + (rf.Count300 + rf.CountGeki) * 300) / (float)(GetTotalHits(rf) * 300);
        default:
          if (GetTotalHits(rf) <= 0)
            return 0.0f;
          return (rf.Count50 * 50 + rf.Count100 * 100 + rf.Count300 * 300) / (float)(GetTotalHits(rf) * 300);
      }
    }

    public static Rankings GetRanking(ReplayFile rf)
    {
      if (!rf.Passed)
        return Rankings.F;
      bool flag1 = ContainsMods((Mods)rf.UsedMods, Mods.Hidden | Mods.Flashlight);
      bool flag2 = ContainsMods((Mods)rf.UsedMods, Mods.Hidden | Mods.Flashlight | Mods.FadeIn);
      float accuracy = GetAccuracy(rf);
      switch (rf.Mode)
      {
        case 1:
          float num1 = rf.Count300 / GetTotalHits(rf);
          float num2 = rf.Count50 / GetTotalHits(rf);
          if (num1 == 1.0)
            return flag1 ? Rankings.XH : Rankings.X;
          if (num1 > 0.9 && num2 <= 0.01 && rf.CountMiss == 0)
            return flag1 ? Rankings.SH : Rankings.S;
          if (num1 > 0.8 && rf.CountMiss == 0 || num1 > 0.9)
            return Rankings.A;
          if (num1 > 0.7 && rf.CountMiss == 0 || num1 > 0.8)
            return Rankings.B;
          return num1 <= 0.6 ? Rankings.D : Rankings.C;
        case 2:
          if (accuracy == 1.0)
            return flag1 ? Rankings.XH : Rankings.X;
          if (accuracy > 0.98)
            return flag1 ? Rankings.SH : Rankings.S;
          if (accuracy > 0.94)
            return Rankings.A;
          if (accuracy > 0.9)
            return Rankings.B;
          return accuracy <= 0.85 ? Rankings.D : Rankings.C;
        case 3:
          if (accuracy == 1.0)
            return flag2 ? Rankings.XH : Rankings.X;
          if (accuracy > 0.95)
            return flag2 ? Rankings.SH : Rankings.S;
          if (accuracy > 0.9)
            return Rankings.A;
          if (accuracy > 0.8)
            return Rankings.B;
          return accuracy <= 0.7 ? Rankings.D : Rankings.C;
        default:
          float num3 = rf.Count50 / GetTotalHits(rf);
          if (accuracy == 1.0 && rf.FullCombo)
            return flag1 ? Rankings.XH : Rankings.X;
          if (accuracy > 0.9 && num3 <= 0.01 && rf.FullCombo)
            return flag1 ? Rankings.SH : Rankings.S;
          if (accuracy > 0.8 && rf.FullCombo || accuracy > 0.9)
            return Rankings.A;
          if (accuracy > 0.7 && rf.FullCombo || accuracy > 0.8)
            return Rankings.B;
          return GetAccuracy(rf) <= 0.6 ? Rankings.D : Rankings.C;
      }
    }

    private static bool ContainsMods(Mods allMods, Mods modsToCheck)
    {
      return (uint)(allMods & modsToCheck) > 0U;
    }

    public static string GetPath()
    {
      string str = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\osu!\\DefaultIcon")?.GetValue(null).ToString();
      return str?.Substring(1, str.Length - 4);
    }

    public static int GetLastOsuVersion()
    {
      return GetLastOsuVersion(GetPath());
    }

    public static int GetLastOsuVersion(string path)
    {
      try
      {
        using (StreamReader streamReader = new StreamReader(path))
        {
          while (!streamReader.EndOfStream)
          {
            string str = streamReader.ReadLine();
            if (str != null && str.StartsWith("LastVersion"))
              return int.Parse(str.Split('=').Last().Trim().Substring(1, 8));
          }
        }
      }
      catch
      {
      }
      return 0;
    }

    public enum Rankings
    {
      XH,
      SH,
      X,
      S,
      A,
      B,
      C,
      D,
      F,
      N,
    }

    [Flags]
    public enum Mods
    {
      None = 0,
      NoFail = 1,
      Easy = 2,
      NoVideo = 4,
      Hidden = 8,
      HardRock = 16, // 0x00000010
      SuddenDeath = 32, // 0x00000020
      DoubleTime = 64, // 0x00000040
      Relax = 128, // 0x00000080
      HalfTime = 256, // 0x00000100
      Nightcore = 512, // 0x00000200
      Flashlight = 1024, // 0x00000400
      Autoplay = 2048, // 0x00000800
      SpunOut = 4096, // 0x00001000
      Relax2 = 8192, // 0x00002000
      Perfect = 16384, // 0x00004000
      Key4 = 32768, // 0x00008000
      Key5 = 65536, // 0x00010000
      Key6 = 131072, // 0x00020000
      Key7 = 262144, // 0x00040000
      Key8 = 524288, // 0x00080000
      FadeIn = 1048576, // 0x00100000
      Random = 2097152, // 0x00200000
      LastMod = 4194304, // 0x00400000
      Key9 = 16777216, // 0x01000000
      Key10 = 33554432, // 0x02000000
      Key1 = 67108864, // 0x04000000
      Key3 = 134217728, // 0x08000000
      Key2 = 268435456, // 0x10000000
    }
  }
}
