namespace Qs.Parse.Developed
{
    public class AsmGoto
    {
        public int GotoLocation;
        public readonly AsmLabel Label;

        public AsmGoto(int gotoLocation, AsmLabel label)
        {
            GotoLocation = gotoLocation;
            Label = label;
        }

        public bool Freezed { get; set; }
    }
}