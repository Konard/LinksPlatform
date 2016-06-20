using System;

namespace Platform.Data.Core.Triggers
{
    internal interface ITrigger
    {
        ICondition Condition { get; }
        // or IEvent MatchingEvent { get; }
        // or IPattern Restriction { get; }

        Action<IContext> Action { get; }
    }
}