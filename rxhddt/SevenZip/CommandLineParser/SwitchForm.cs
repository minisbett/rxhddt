// Decompiled with JetBrains decompiler
// Type: SevenZip.CommandLineParser.SwitchForm
// Assembly: osu!ftw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 43BAB63D-DE2A-4E38-86D4-2EF193F865F8
// Assembly location: C:\Users\Niklas\Downloads\aac_Portable\osu!\osu!ftw.exe

namespace SevenZip.CommandLineParser
{
  public class SwitchForm
  {
    public string IDString;
    public SwitchType Type;
    public bool Multi;
    public int MinLen;
    public int MaxLen;
    public string PostCharSet;

    public SwitchForm(
      string idString,
      SwitchType type,
      bool multi,
      int minLen,
      int maxLen,
      string postCharSet)
    {
      this.IDString = idString;
      this.Type = type;
      this.Multi = multi;
      this.MinLen = minLen;
      this.MaxLen = maxLen;
      this.PostCharSet = postCharSet;
    }

    public SwitchForm(string idString, SwitchType type, bool multi, int minLen)
      : this(idString, type, multi, minLen, 0, "")
    {
    }

    public SwitchForm(string idString, SwitchType type, bool multi)
      : this(idString, type, multi, 0)
    {
    }
  }
}
