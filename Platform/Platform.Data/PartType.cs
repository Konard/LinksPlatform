// ReSharper disable TypeParameterCanBeVariant

namespace Platform.Data
{
    public enum PartType : ulong
    {
        LinkIndexOrId = 0,
        LinkSourceOrFirst = 1,
        LinkTargetOrSecond = 2,
        LinkLinkerOrThird = 3
    }
}