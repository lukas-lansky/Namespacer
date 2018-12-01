using Namespacer.Configuration;

namespace Namespacer.Engine
{
    public static class EvaluationEngine
    {
        public static bool IsOk(string caller, string callee, ConfigFile configFile)
            => configFile.Rules.TrueForAll(r => IsOk(caller, callee, r));

        private static bool IsOk(string caller, string callee, Rule role)
        {
            if (Match(caller, role.NamespaceFrom) && Match(callee, role.NamespaceTo))
            {
                foreach (var p in role.Prescriptions)
                {
                    if (p.Type == PrescriptionType.Allow && Match(caller, p.NamespaceFrom) && Match(callee, p.NamespaceTo))
                    {
                        return true;
                    }

                    if (p.Type == PrescriptionType.Disallow && Match(caller, p.NamespaceFrom) && Match(callee, p.NamespaceTo))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool Match(string codeName, string configName)
        {
            if (configName == "*")
            {
                return true;
            }

            return codeName.StartsWith(configName);
        }
    }
}
