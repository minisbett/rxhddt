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
