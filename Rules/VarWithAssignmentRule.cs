using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace C2CSharp.Rules
{
    public class VarWithAssignmentRule : Rule
    {
        public override string RuleName
        {
            get { return "VarWithAssignmentRule"; }
        }

        /// <summary>Matches a variable declaration with an assignment</summary>
        const string pattern = @" 
           (public[\ \t\r\n]*?|protected[\ \t\r\n]*?|private[\ \t\r\n]*?)?
           (static[\ \t\r\n]*?)?
           (const[\ \t\r\n]*?)?
           (?<type>
             (((un)?signed)[\ \t\r\n]*?)? ((char|wchar_t|void)[\ \t\r\n]*?\*|short[\ \t\r\n]+int|long[\ \t\r\n]+(int|long)|int|char|short|long)
             |[a-zA-Z_][a-zA-Z0-9_]*
           )[\ \t\r\n]*
           (?<name>[a-zA-Z_][a-zA-Z0-9_]*)[\ \t\r\n]*
           (:[\ \t\r\n]*\d+[\ \t\r\n]*)? # match bit fields
           (?<post>.*?;[\t\ ]*(//[^\r?\n]*?(?=\r?\n))?)";


        string DoSomething(Match matchString)
        {
            string strTemp = matchString.ToString();
            return strTemp;
        }

        public override bool Execute(string strOrigin, out string strOutput, int iRowNumber)
        {
            Regex regex = new Regex(pattern);
            string result = strOrigin;
            bool changedFlag = false;
            if (regex.IsMatch(strOrigin))
            {
                result = regex.Replace(strOrigin, new MatchEvaluator(DoSomething));
                changedFlag = true;
            }
            strOutput = result;
            return changedFlag;
        }
    }
}
