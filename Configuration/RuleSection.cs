using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace C2CSharp.Configuration
{
    public class RuleSection
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string Replacement { get; set; }
    }
}
