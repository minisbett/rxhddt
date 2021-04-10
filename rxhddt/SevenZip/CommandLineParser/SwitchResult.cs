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
