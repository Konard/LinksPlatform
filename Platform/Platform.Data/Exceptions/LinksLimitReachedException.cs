using System;

namespace Platform.Data.Exceptions
{
    public class LinksLimitReachedException : Exception
    {
        private const string DefaultMessage = "Достигнут лимит количества связей в хранилище.";

        public LinksLimitReachedException(ulong limit) : this(FormatMessage(limit)) { }

        public LinksLimitReachedException(string message = DefaultMessage) : base(message) { }

        private static string FormatMessage(ulong limit) => $"Достигнут лимит количества связей в хранилище ({limit}).";
    }
}