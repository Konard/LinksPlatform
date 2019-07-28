using System;

namespace Platform.Data.Exceptions
{
    public class LinksLimitReachedException : Exception
    {
        public static readonly string DefaultMessage = "Достигнут лимит количества связей в хранилище.";
        public LinksLimitReachedException(string message) : base(message) { }
        public LinksLimitReachedException(ulong limit) : this(FormatMessage(limit)) { }
        public LinksLimitReachedException() : base(DefaultMessage) { }
        private static string FormatMessage(ulong limit) => $"Достигнут лимит количества связей в хранилище ({limit}).";
    }
}