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
        ("Easy","EZ", 1, OsuHelper.Mods.Easy, '1'),
        ("NoFail","NF", 0, OsuHelper.Mods.NoFail, '2'),
        ("HalfTime","HT", 8, OsuHelper.Mods.HalfTime, '3'),
        ("Hard Rock", "HR", 4, OsuHelper.Mods.HardRock, '4'),
        ("SuddenDeath","SD", 5, OsuHelper.Mods.SuddenDeath, '5'),
        ("Perfect","PF", 14, OsuHelper.Mods.Perfect, '6'),
        ("DoubleTime", "DT", 6, OsuHelper.Mods.DoubleTime, '7'),
        ("Nightcore", "NC", 9, OsuHelper.Mods.Nightcore, '8'),
        ("Hidden", "HD", 3, OsuHelper.Mods.Hidden, '9'),
        ("Flashlight", "FL", 10, OsuHelper.Mods.Flashlight, '0'),
        ("Relax", "RL", 7, OsuHelper.Mods.Relax, 'a'),
        ("AutoPilot", "AP", 13, OsuHelper.Mods.AutoPilot, 'b'),
        ("TargetPractice (Note: This may break things.)", "TP", 23, OsuHelper.Mods.TargetPractice, 'c'),
        ("SpunOut", "SO", 12, OsuHelper.Mods.SpunOut, 'd'),
        ("Autoplay", "AT", 11, OsuHelper.Mods.Autoplay, 'e'),
        ("Cinema", "CM", 22, OsuHelper.Mods.Cinema, 'f'),
        ("ScoreV2", "V2", 29, OsuHelper.Mods.ScoreV2, 'g'),
        ("FadeIn", "FI", 20, OsuHelper.Mods.FadeIn, 'h'),
        ("Key1", "K1", 26, OsuHelper.Mods.Key1, 'i'),
        ("Key2", "K2", 28, OsuHelper.Mods.Key2, 'j'),
        ("Key3", "K3", 27, OsuHelper.Mods.Key3, 'k'),
        ("Key4", "K4", 15, OsuHelper.Mods.Key4, 'l'),
        ("Key5", "K5", 16, OsuHelper.Mods.Key5, 'm'),
        ("Key6", "K6", 17, OsuHelper.Mods.Key6, 'n'),
        ("Key7", "K7", 18, OsuHelper.Mods.Key7, 'o'),
        ("Key8", "K8", 19, OsuHelper.Mods.Key8, 'p'),
        ("Key9", "K9", 24, OsuHelper.Mods.Key9, 'q'),
        ("Co-op", "CP", 25, OsuHelper.Mods.Co_op, 'r'),
        ("Mirror", "MR", 30, OsuHelper.Mods.Mirror, 's'),
        ("Random", "RD", 21, OsuHelper.Mods.Random, 't'),
        ("TouchDevice", "TD", 2, OsuHelper.Mods.TouchDevice, 'u'),
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
      replay.ReplayDate = DateTime.UtcNow; // fix replay data cache problem
      replay.ReplayHash = ReplayHelper.GetReplayHash(replay);
      ReplayHelper.SaveFile(sfd.FileName, replay);
    }

    static ReplayFile flipNotes(ReplayFile replay)
    {
      Console.Clear();
      Console.WriteLine("Flipping notes (HR was toggled)...");
      string[] list = Encoding.ASCII.GetString(SevenZipHelper.Decompress(replay.Replay)).Split(',');
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
          list[index] = string.Join("|", strArray);
        }
      }

      Console.WriteLine("Compressing replay data...");
      replay.Replay = SevenZipHelper.Compress(Encoding.ASCII.GetBytes(string.Join(",", list)));
      return replay;
    }
  }
}