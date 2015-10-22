using System;

namespace Platform.Links.DataBase.CoreNet.Triggers
{
    public interface ITrigger
    {
        ICondition Condition { get; }
        // or IEvent MatchingEvent { get; }

        Action<IContext> Action { get; }
    }
}