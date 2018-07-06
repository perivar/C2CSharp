using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using C2CSharp.Configuration;
using System.Configuration;
using C2CSharp.Rules;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Linq;

namespace C2CSharp
{
    public class RuleEngine
    {

        private IConfigurationRoot config = null;
        private List<Rule> rules = new List<Rule>();

        public RuleEngine(IConfigurationRoot config)
        {
            this.config = config;
        }

        public void AddRule(Rule rule)
        {
            rules.Add(rule);
        }

        /// <summary>
        /// Loads the rules.
        /// </summary>
        public void LoadRules()
        {
            // load from appsettings.json
            var rules = config.GetSection("C2CS:Rules:add").Get<List<RuleSection>>();

            foreach (RuleSection rs in rules)
            {
                EquivalentRule r = new EquivalentRule(rs.Name);
                r.Pattern = rs.Pattern;
                r.SetReplacement(rs.Replacement);
                AddRule(r);
            }

            // load from assembly
            Assembly asm = Assembly.GetExecutingAssembly();
            string nameSpace = "C2CSharp.Rules";
            foreach (Type type in asm.GetTypes())
            {
                if (type.Namespace == nameSpace && !type.IsAbstract && type.Name != "EquivalentRule")
                    AddRule(Activator.CreateInstance(type) as Rule);
            }
        }

        protected static void Log(string message)
        {
            Console.WriteLine(message);
        }
        protected static void LogTip(string message)
        {
            Console.WriteLine(message);
        }

        protected void LogExecute(string ruleName, int iRowNumber)
        {
            Log(string.Format("[Line {0}] {1}", iRowNumber, ruleName));
        }

        public void ConvertDirectory(string sourceDirectory, string targetDirectory)
        {
            // read from file
            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine("Source-directory not found.");
                return;
            }
            if (!Directory.Exists(targetDirectory))
            {
                Console.WriteLine("Target-directory not found.");
                return;
            }

            IEnumerable<string> cFiles =
                Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories)
                .Where(f => ".c".Equals(Path.GetExtension(f).ToLower()));
            Console.Out.WriteLine("Found {0} files in scan directory.", cFiles.Count());

            foreach (var cFile in cFiles)
            {
                string directoryPath = Path.GetDirectoryName(cFile);
                string relativePath = Path.GetRelativePath(sourceDirectory, directoryPath);
                string newFileName = Path.GetFileNameWithoutExtension(cFile) + ".cs";

                string destinationFilePath = "";
                if (!relativePath.Equals("."))
                {
                    destinationFilePath = Path.Combine(new string[] { targetDirectory, relativePath, newFileName });
                }
                else
                {
                    destinationFilePath = Path.Combine(new string[] { targetDirectory, newFileName });
                }

                ConvertFile(cFile, destinationFilePath);
            }
        }

        public void ConvertFile(string sourceFilePath, string destinationFilePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            var sr = new StreamReader(sourceFilePath, true);
            string strOrigin = sr.ReadToEnd();
            sr.Close();

            var sb = new StringBuilder();

            // add using statements, namespace and class;
            sb.AppendLine("using System;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0} {{\r\n\r\n", "C2CSharpConverted");
            sb.AppendFormat("public class {0} {{\r\n\r\n", fileName);

            // add rules
            var arrInput = strOrigin.Split(new char[] { '\n' });
            for (int i = 0; i < arrInput.Length; i++)
            {
                var tmp = arrInput[i].Replace("\r", "");

                int ruleNum = i + 1;
                foreach (Rule rule in rules)
                {
                    if (rule.Execute(tmp, out tmp, ruleNum))
                    {
                        LogExecute(rule.RuleName, ruleNum);
                    }
                }
                sb.AppendLine(tmp);
            }

            // add end brackets;
            sb.AppendLine("}");
            sb.AppendLine("}");

            string convertedFile = sb.ToString();

            // remove double newlines
            string result = Regex.Replace(convertedFile, @"[\r\n]{3,}", "\r\n\r\n");

            // remove module descriptors, like static int  OpenDecoder( vlcObjectT * );
            const string moduleDescs = @"(public[\ \t\r\n]*?|protected[\ \t\r\n]*?|private[\ \t\r\n]*?)?(static[\ \t\r\n]*?)?(const[\ \t\r\n]*?)?(?<type>(((un)?signed)[\ \t\r\n]*?)?((char|wchar_t|void)[\ \t\r\n]*?\*?|short[\ \t\r\n]+int|long[\ \t\r\n]+(int|long)|int|char|short|long))[\ \t\r\n]*(?<name>[a-zA-Z_][a-zA-Z0-9_]*)[\ \t\r\n]*(?<params>\([^\)]*\);)";
            Regex moduleRegex = new Regex(moduleDescs, RegexOptions.Multiline);
            if (moduleRegex.IsMatch(result))
            {
                result = moduleRegex.Replace(result, "");
            }

            // fix structs
            const string structMethods = @"(typedef[\ \t\r\n]*?)?(struct)[\ \t\r\n]*(?:[a-zA-Z0-9_]*)[\ \t\r\n]*(?<body>\{[^\}]*[\ \t\r\n]*\})[\ \t\r\n]*(?<name>[a-zA-Z_][a-zA-Z0-9_]*)[\ \t\r\n]*;";
            Regex structRegex = new Regex(structMethods, RegexOptions.Multiline);
            if (structRegex.IsMatch(result))
            {
                result = structRegex.Replace(result, "public struct ${name} ${body};");
            }

            // fix const array inits
            const string arrayInits = @"(?<access>public[\ \t\r\n]*?|protected[\ \t\r\n]*?|private[\ \t\r\n]*?)?(static[\ \t\r\n]*?)?(const[\ \t\r\n]*?)(?<type>(((un)?signed)[\ \t\r\n]*?)?((char|wchar_t|void)[\ \t\r\n]*?\*?|short[\ \t\r\n]+int|long[\ \t\r\n]+(int|long)|int|char|short|long))[\ \t\r\n]*(?<name>[a-zA-Z_][a-zA-Z0-9_]*)[\ \t\r\n]*(?<size>\[\d+\](\[\d+\])?)[\ \t\r\n]*=";
            Regex arrayInitRegex = new Regex(arrayInits);
            if (arrayInitRegex.IsMatch(result))
            {
                result = arrayInitRegex.Replace(result, "public static readonly ${type}${size} ${name} =");
            }

            // save result to file
            var sw = new StreamWriter(destinationFilePath, false);
            sw.Write(result);
            sw.Close();
        }

        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
