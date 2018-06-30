using System;
using System.Text;
using System.Text.RegularExpressions;

namespace C2CSharp
{
    public interface IRule
    {
        bool Execute(string strOrigin, out string strOutput, int iRowNumber);
    }

    public abstract class Rule : IRule
    {
        #region IRuleExecutor Members

        public abstract bool Execute(string strOrigin, out string strOutput, int iRowNumber);

        public abstract string RuleName { get; }

        #endregion
    }
}
