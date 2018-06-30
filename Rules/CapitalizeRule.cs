using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace C2CSharp.Rules
{
    public class CapitalizeRule : Rule
    {
        public override string RuleName
        {
            get { return "Capitalize Rule"; }
        }

        const string pattern = @"(?:_)([a-z])";

        string CapitalizeString(Match matchString)
        {
            return char.ToUpper(matchString.Groups[1].Value[0]).ToString();
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
