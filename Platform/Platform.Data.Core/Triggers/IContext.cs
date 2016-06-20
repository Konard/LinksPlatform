namespace Platform.Data.Core.Triggers
{
    internal interface IContext
    {
        ITrigger Trigger { get; }
        // Before
        // After
        // Event Type
        // Timestamp
        // Subject/User (System, Person, Robot, ...)
        // Cancel (true|false)
        // ...
    }
}