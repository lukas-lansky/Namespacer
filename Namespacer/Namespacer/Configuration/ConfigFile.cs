using System;
using System.Collections.Generic;

namespace Namespacer.Configuration
{
    public struct ConfigFile
    {
        public List<Rule> Rules;

        public ConfigFile(List<Rule> rules)
        {
            Rules = rules;
        }

        public static ConfigFile? LoadFromFile(string path)
            => LoadFromString(System.IO.File.ReadAllText(path));

        public static ConfigFile? LoadFromString(string content)
        {
            var lines = content.Split('\n');
            var rules = new List<Rule>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Length == 0)
                {
                    continue;
                }

                if (!char.IsWhiteSpace(line[0]))
                {
                    // new rule
                    rules.Add(Rule.FromLine(trimmedLine));
                }
                else
                {
                    rules[rules.Count - 1].Prescriptions.Add(Prescription.FromLine(trimmedLine));
                }
            }

            return new ConfigFile(rules);
        }
    }

    public struct Rule
    {
        public string NamespaceFrom;

        public string NamespaceTo;

        public List<Prescription> Prescriptions;

        public Rule(string from, string to, List<Prescription> prescriptions)
        {
            NamespaceFrom = from;
            NamespaceTo = to;
            Prescriptions = prescriptions;
        }

        public static Rule FromLine(string line)
        {
            var parts = line.Split(new[] { "=>" }, StringSplitOptions.None);
            var to = parts[1].Trim();
            to = to.Substring(0, to.Length - 1);
            return new Rule(parts[0].Trim(), to, new List<Prescription>());
        }
    }

    public struct Prescription
    {
        public PrescriptionType Type;

        public string NamespaceFrom;

        public string NamespaceTo;

        public Prescription(PrescriptionType type, string from, string to)
        {
            Type = type;
            NamespaceFrom = from;
            NamespaceTo = to;
        }

        public static Prescription FromLine(string line)
        {
            if (line.IndexOf("->") > -1)
            {
                var parts = line.Split(new[] { "->" }, StringSplitOptions.None);
                return new Prescription(PrescriptionType.Allow, parts[0].Trim(), parts[1].Trim());
            }
            else
            {
                var parts = line.Split(new[] { "-!>" }, StringSplitOptions.None);
                return new Prescription(PrescriptionType.Disallow, parts[0].Trim(), parts[1].Trim());
            }
        }
    }

    public enum PrescriptionType
    {
        Allow,
        Disallow
    }
}
