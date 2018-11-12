using System.Collections.Generic;

namespace Qs.Utils.Indexation
{
    public class VariableGenerator
    {
        private readonly List<string> varsList=new List<string>(20);
        private readonly string _prefix;
        private readonly string _suffix;

        public VariableGenerator(string prefix,string suffix)
        {
            _prefix = prefix;
            _suffix = suffix;
        }

        private int _current = -1;

        public string Current
        {
            get
            {

                return varsList[_current];
            }
        }

        public string GetNew()
        {
            _current++;
            if (_current>=varsList.Count)
            {
                var s = string.Concat(_prefix, _current, _suffix);
                varsList.Add(s);
                return s;
            }
            return Current;
        }

        public void Remove()
        {
            _current--;
        }

        public void Reset()
        {
            _current = -1;
        }
    }
}