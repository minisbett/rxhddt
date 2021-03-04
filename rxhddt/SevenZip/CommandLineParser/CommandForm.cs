// Decompiled with JetBrains decompiler
// Type: SevenZip.CommandLineParser.CommandForm
// Assembly: osu!ftw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 43BAB63D-DE2A-4E38-86D4-2EF193F865F8
// Assembly location: C:\Users\Niklas\Downloads\aac_Portable\osu!\osu!ftw.exe

namespace SevenZip.CommandLineParser
{
  public class CommandForm
  {
    public string IDString = "";
    public bool PostStringMode;

    public CommandForm(string idString, bool postStringMode)
    {
      this.IDString = idString;
      this.PostStringMode = postStringMode;
    }
  }
}
