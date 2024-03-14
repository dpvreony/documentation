using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using Whipstaff.Runtime.Extensions;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown.Helpers
{
    public static class ReflectionHelpers
    {
        public static ImmutableArray<DiagnosticAnalyzer>? GetAnalyzersFromAssembly(Assembly assembly)
        {
            var allTypes = assembly.GetTypes();

            var matchingTypes = allTypes.Where(type => IsDiagnosticAnalyzer(type));

            var result = new List<DiagnosticAnalyzer>();

            foreach (var matchingType in matchingTypes)
            {
                var ctor = matchingType.GetParameterlessConstructor();
                if (ctor == null)
                {
                    continue;
                }

                var instance = ctor.Invoke(null);

                result.Add((DiagnosticAnalyzer)instance);
            }

            return result.ToImmutableArray();
        }

        private static bool IsDiagnosticAnalyzer(Type type)
        {
            if (!type.IsPublic || type.IsAbstract)
            {
                return false;
            }

            if (type.ContainsGenericParameters)
            {
                return false;
            }

            return type.IsAssignableTo(typeof(DiagnosticAnalyzer));
        }
    }
}
