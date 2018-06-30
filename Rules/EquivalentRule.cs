using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace C2CSharp.Rules
{
    public class EquivalentRule : Rule
    {
        private string replacement;
        private string name;

        public EquivalentRule()
        {

        }
        public EquivalentRule(string name)
        {
            this.name = name;
        }

        public override string RuleName
        {
            get { return name; }
        }

        public virtual string GetReplacement()
        {
            return replacement;
        }

        public virtual void SetReplacement(string value)
        {
            replacement = value;
        }

        public virtual string Pattern
        {
            get;
            set;
        }

        public override sealed bool Execute(string strOrigin, out string strOutput, int iRowNumber)
        {
            Regex regex = new Regex(this.Pattern);
            string result = strOrigin;
            bool changedFlag = false;

            if (regex.IsMatch(strOrigin))
            {
                // use get replacement directly to support captured groups
                result = regex.Replace(strOrigin, this.GetReplacement());
                changedFlag = true;
            }
            strOutput = result;
            return changedFlag;
        }

        public override string ToString()
        {
            return string.Format("Equivalent Rule '{0}'", this.RuleName);
        }
    }
}
