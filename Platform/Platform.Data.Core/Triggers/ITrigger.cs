using System;

namespace Platform.Data.Core.Triggers
{
    public interface ITrigger
    {
        ICondition Condition { get; }
        // or IEvent MatchingEvent { get; }
        // or IPattern Restriction { get; }

        Action<IContext> Action { get; }
    }
}