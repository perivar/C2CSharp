using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace C2CSharp.Rules
{
    public class CapitalizeMethodRule : Rule
    {
        public override string RuleName
        {
            get { return "Capitalize Method Rule"; }
        }

        const string pattern = @"\b(?!return|switch|if|for|while)\b[a-z][a-zA-Z0-9_]+\s*\(";

        string CapitalizeString(Match matchString)
        {
            string s = matchString.Groups[0].Value;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public override bool Execute(string strOrigin, out string strOutput, int iRowNumber)
        {
            Regex regex = new Regex(pattern);
            string result = strOrigin;
            bool changedFlag = false;
            if (regex.IsMatch(strOrigin))
            {
                result = regex.Replace(strOrigin, new MatchEvaluator(CapitalizeString));
                changedFlag = true;
            }
            strOutput = result;
            return changedFlag;
        }
    }
}
