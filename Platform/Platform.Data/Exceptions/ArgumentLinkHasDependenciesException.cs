using System;

namespace Platform.Data.Exceptions
{
    public class ArgumentLinkHasDependenciesException<TLink> : ArgumentException
    {
        public ArgumentLinkHasDependenciesException(TLink link, string paramName) : base(FormatMessage(link, paramName), paramName) { }

        public ArgumentLinkHasDependenciesException(TLink link) : base(FormatMessage(link)) { }

        private static string FormatMessage(TLink link, string paramName) => $"У связи [{link}] переданной в аргумент [{paramName}] присутствуют зависимости, которые препятствуют изменению её внутренней структуры.";

        private static string FormatMessage(TLink link) => $"У связи [{link}] переданной в качестве аргумента присутствуют зависимости, которые препятствуют изменению её внутренней структуры.";
    }
}