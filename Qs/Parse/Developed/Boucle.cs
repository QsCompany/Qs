using Qs.Utils;

namespace Qs.Parse.Developed
{
    public class Boucle : ExtendParse
    {

        public Boucle(BasicParse basicParse)
            : base(EPNames.Boucle, basicParse)
        {
        }

        public override bool Parse(Tree parent)
        {
            Pile.Save();
            var @if = base[EPNames.If];
            var @for = base[EPNames.For];
            var @while = base[EPNames.While];
            var @do = base[EPNames.Do];
            return Pile.Leave(@if.Parse(parent) || @for.Parse(parent) || @while.Parse(parent) || @do.Parse(parent));
        }
    }
}