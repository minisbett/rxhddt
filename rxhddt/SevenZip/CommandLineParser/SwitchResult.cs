// Decompiled with JetBrains decompiler
// Type: SevenZip.CommandLineParser.SwitchResult
// Assembly: osu!ftw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 43BAB63D-DE2A-4E38-86D4-2EF193F865F8
// Assembly location: C:\Users\Niklas\Downloads\aac_Portable\osu!\osu!ftw.exe

using System.Collections;

namespace SevenZip.CommandLineParser
{
  public class SwitchResult
  {
    public ArrayList PostStrings = new ArrayList();
    public bool ThereIs;
    public bool WithMinus;
    public int PostCharIndex;

    public SwitchResult()
    {
      this.ThereIs = false;
    }
  }
}
