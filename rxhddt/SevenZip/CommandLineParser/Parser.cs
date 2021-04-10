using System;
using System.Collections;

namespace SevenZip.CommandLineParser
{
  public class Parser
  {
    public ArrayList NonSwitchStrings = new ArrayList();
    private SwitchResult[] _switches;
    private const char kSwitchID1 = '-';
    private const char kSwitchID2 = '/';
    private const char kSwitchMinus = '-';
    private const string kStopSwitchParsing = "--";

    public Parser(int numSwitches)
    {
      this._switches = new SwitchResult[numSwitches];
      for (int index = 0; index < numSwitches; ++index)
        this._switches[index] = new SwitchResult();
    }

    private bool ParseString(string srcString, SwitchForm[] switchForms)
    {
      int length1 = srcString.Length;
      if (length1 == 0)
        return false;
      int index1 = 0;
      if (!Parser.IsItSwitchChar(srcString[index1]))
        return false;
      while (index1 < length1)
      {
        if (Parser.IsItSwitchChar(srcString[index1]))
          ++index1;
        int index2 = 0;
        int num1 = -1;
        for (int index3 = 0; index3 < this._switches.Length; ++index3)
        {
          int length2 = switchForms[index3].IDString.Length;
          if (length2 > num1 && index1 + length2 <= length1 && string.Compare(switchForms[index3].IDString, 0, srcString, index1, length2, true) == 0)
          {
            index2 = index3;
            num1 = length2;
          }
        }
        if (num1 == -1)
          throw new Exception("maxLen == kNoLen");
        SwitchResult switchResult = this._switches[index2];
        SwitchForm switchForm = switchForms[index2];
        if (!switchForm.Multi && switchResult.ThereIs)
          throw new Exception("switch must be single");
        switchResult.ThereIs = true;
        index1 += num1;
        int num2 = length1 - index1;
        SwitchType type = switchForm.Type;
        switch (type)
        {
          case SwitchType.PostMinus:
            if (num2 == 0)
            {
              switchResult.WithMinus = false;
              continue;
            }
            switchResult.WithMinus = srcString[index1] == '-';
            if (switchResult.WithMinus)
            {
              ++index1;
              continue;
            }
            continue;
          case SwitchType.LimitedPostString:
          case SwitchType.UnLimitedPostString:
            int minLen = switchForm.MinLen;
            if (num2 < minLen)
              throw new Exception("switch is not full");
            if (type == SwitchType.UnLimitedPostString)
            {
              switchResult.PostStrings.Add((object) srcString.Substring(index1));
              return true;
            }
            string str = srcString.Substring(index1, minLen);
            index1 += minLen;
            for (int index3 = minLen; index3 < switchForm.MaxLen && index1 < length1; ++index1)
            {
              char c = srcString[index1];
              if (!Parser.IsItSwitchChar(c))
              {
                str += c.ToString();
                ++index3;
              }
              else
                break;
            }
            switchResult.PostStrings.Add((object) str);
            continue;
          case SwitchType.PostChar:
            if (num2 < switchForm.MinLen)
              throw new Exception("switch is not full");
            string postCharSet = switchForm.PostCharSet;
            if (num2 == 0)
            {
              switchResult.PostCharIndex = -1;
              continue;
            }
            int num3 = postCharSet.IndexOf(srcString[index1]);
            if (num3 < 0)
            {
              switchResult.PostCharIndex = -1;
              continue;
            }
            switchResult.PostCharIndex = num3;
            ++index1;
            continue;
          default:
            continue;
        }
      }
      return true;
    }

    public void ParseStrings(SwitchForm[] switchForms, string[] commandStrings)
    {
      int length = commandStrings.Length;
      bool flag = false;
      for (int index = 0; index < length; ++index)
      {
        string commandString = commandStrings[index];
        if (flag)
          this.NonSwitchStrings.Add((object) commandString);
        else if (commandString == "--")
          flag = true;
        else if (!this.ParseString(commandString, switchForms))
          this.NonSwitchStrings.Add((object) commandString);
      }
    }

    public SwitchResult this[int index]
    {
      get
      {
        return this._switches[index];
      }
    }

    public static int ParseCommand(
      CommandForm[] commandForms,
      string commandString,
      out string postString)
    {
      for (int index = 0; index < commandForms.Length; ++index)
      {
        string idString = commandForms[index].IDString;
        if (commandForms[index].PostStringMode)
        {
          if (commandString.IndexOf(idString) == 0)
          {
            postString = commandString.Substring(idString.Length);
            return index;
          }
        }
        else if (commandString == idString)
        {
          postString = "";
          return index;
        }
      }
      postString = "";
      return -1;
    }

    private static bool ParseSubCharsCommand(
      int numForms,
      CommandSubCharsSet[] forms,
      string commandString,
      ArrayList indices)
    {
      indices.Clear();
      int num1 = 0;
      for (int index1 = 0; index1 < numForms; ++index1)
      {
        CommandSubCharsSet form = forms[index1];
        int num2 = -1;
        int length = form.Chars.Length;
        for (int index2 = 0; index2 < length; ++index2)
        {
          char ch = form.Chars[index2];
          int num3 = commandString.IndexOf(ch);
          if (num3 >= 0)
          {
            if (num2 >= 0 || commandString.IndexOf(ch, num3 + 1) >= 0)
              return false;
            num2 = index2;
            ++num1;
          }
        }
        if (num2 == -1 && !form.EmptyAllowed)
          return false;
        indices.Add((object) num2);
      }
      return num1 == commandString.Length;
    }

    private static bool IsItSwitchChar(char c)
    {
      if (c != '-')
        return c == '/';
      return true;
    }
  }
}
