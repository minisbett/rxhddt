using RXHDDT.Util;
using SevenZip.Compression.LZMA;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RXHDDT
{
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      Console.WriteLine("Select the replay file.");
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Filter = "Osu Replay Files|*.osr";
      if (ofd.ShowDialog() != DialogResult.OK)
        return;
      Console.Clear();
      Console.WriteLine("Loading replay...");
      ReplayFile replay = ReplayHelper.ReadFile(ofd.FileName);
      OsuHelper.Mods modsBefore = (OsuHelper.Mods)replay.UsedMods;

      List<(string fullname, string shortname, int priority, OsuHelper.Mods mod, char key)> availableMods = new List<(string fullname, string shortname, int priority, OsuHelper.Mods mod, char key)>()
      { // RXEZFLHRHDHTDTNCNF
        ("Relax","RX", 0, OsuHelper.Mods.Relax, '1'),
        ("Double Time","DT", 6, OsuHelper.Mods.DoubleTime, '2'),
        ("Hidden","HD", 4, OsuHelper.Mods.Hidden, '3'),
        ("Hard Rock", "HR", 3, OsuHelper.Mods.HardRock, '4'),
        ("Nightcore","NC", 6, OsuHelper.Mods.Nightcore, '5'),
        ("Flashlight","FL", 2, OsuHelper.Mods.Flashlight, '6'),
        ("Easy", "EZ", 1, OsuHelper.Mods.Easy, '7'),
        ("No Fail", "NF", 7, OsuHelper.Mods.NoFail, '8'),
        ("Half Time", "HT", 5, OsuHelper.Mods.HalfTime, '9'),
      };

      List<(string fullname, string shortname, int priority, OsuHelper.Mods mod, char key)> selectedMods = new List<(string fullname, string shortname, int priority, OsuHelper.Mods mod, char key)>();

      selectedMods.AddRange(availableMods.Where(x => (x.mod & (OsuHelper.Mods)replay.UsedMods) != 0));

      Console.Clear();

      while (true)
      {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine("Select the mods. Press enter to continue.");
        Console.WriteLine();

        string[] mods = selectedMods.OrderBy(x => x.priority).Select(x => x.shortname).ToArray();
        mods = mods.Where(x => x != "DT" || !mods.Contains("NC")).ToArray(); // remove DT if NC is enabled so it doesn't show "DTNC"
        string strSelectedMods = string.Join("", mods);
        Console.WriteLine(("Selected mods: " + strSelectedMods).PadRight(Console.WindowWidth - 1));

        Console.WriteLine();
        for (int i = 0; i < availableMods.Count; i++)
        {
          Console.ForegroundColor = ConsoleColor.Gray;
          if (selectedMods.Contains(availableMods[i]))
            Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"[{availableMods[i].key}] ({availableMods[i].shortname}) {availableMods[i].fullname}");
          Console.ForegroundColor = ConsoleColor.Gray;
        }

        ConsoleKeyInfo cki = Console.ReadKey(true);

        if (cki.Key == ConsoleKey.Enter)
        {
          break;
        }
        else
        {
          var selectedMod = availableMods.FirstOrDefault(x => x.key == cki.KeyChar);
          if (selectedMod.fullname != "")
          {
            if (selectedMod.mod == OsuHelper.Mods.Nightcore && selectedMods.Any(x => x.mod == OsuHelper.Mods.DoubleTime))
              selectedMods.Remove(availableMods.First(x => x.mod == OsuHelper.Mods.DoubleTime));
            if (selectedMod.mod == OsuHelper.Mods.DoubleTime && selectedMods.Any(x => x.mod == OsuHelper.Mods.Nightcore))
              selectedMods.Remove(availableMods.First(x => x.mod == OsuHelper.Mods.Nightcore));
            if (selectedMods.Contains(selectedMod))
              selectedMods.Remove(selectedMod);
            else
            {
              if (selectedMod.mod == OsuHelper.Mods.Nightcore && !selectedMods.Any(x => x.mod == OsuHelper.Mods.DoubleTime))
                selectedMods.Add(availableMods.First(x => x.mod == OsuHelper.Mods.DoubleTime));
              selectedMods.Add(selectedMod);
            }
          }
        }
      }

      replay.UsedMods = (int)OsuHelper.Mods.None;
      foreach (var selectedMod in selectedMods)
        replay.UsedMods |= (int)selectedMod.mod;
      if(((OsuHelper.Mods)replay.UsedMods & OsuHelper.Mods.HardRock) != (modsBefore & OsuHelper.Mods.HardRock))
        replay = flipNotes(replay);

      Console.Clear();
      Console.WriteLine("Select a path for the new replay file.");
      SaveFileDialog sfd = new SaveFileDialog();
      sfd.Filter = "Osu Replay Files|*.osr";
      if (sfd.ShowDialog() != DialogResult.OK)
        return;
      if (File.Exists(sfd.FileName))
        File.Delete(sfd.FileName);
      Console.Clear();
      Console.WriteLine("Saving replay...");
      replay.ReplayHash = ReplayHelper.GetReplayHash(replay);
      ReplayHelper.SaveFile(sfd.FileName, replay);
    }

    static ReplayFile flipNotes(ReplayFile replay)
    {
      Console.Clear();
      Console.WriteLine("Flipping notes (HR was toggled)...");
      string[] list = Encoding.ASCII.GetString(SevenZipHelper.Decompress(replay.Replay)).Split(','));
      for (int index = 0; index < list.Length; index++)
      {
        int percentage = (int)((double)index / list.Length * 100);
        Console.SetCursorPosition(0, 1);
        Console.WriteLine(percentage.ToString().PadLeft(3) + "% done.");

        string[] strArray = list[index].Split('|');
        if (strArray.Length == 4)
        {
          double num = 384.0 - double.Parse(strArray[2], new CultureInfo("en-US"));
          strArray[2] = num.ToString(new CultureInfo("en-US"));
          strArray[1] = "300";
          strArray[2] = "300";
          list[index] = string.Join("|", strArray);
        }
      }

      Console.WriteLine("Compressing replay data...");
      replay.Replay = SevenZipHelper.Compress(Encoding.ASCII.GetBytes(string.Join(",", list)));
      return replay;
    }
  }
}