using System;
using System.Collections.Generic;

namespace Platform.Data.Core.Pairs
{
    /// <remarks>
    /// Options:
    /// Ensure link references only existing links. Auto creation of point.
    /// +Check for arguments are references to existing links. Exception on error.
    /// Ensure link value is unique: GetOrCreate, GetOrUpdate
    /// +Check link cannot be updated to other's link value: Exception on error.
    /// +Ensure action is cascade. Automatic cascade create, update, delete. +(DefaultLinksNonExistentReferencesCreator, DefaultLinksCascadeUniquenessAndDependenciesResolver, DefaultLinksCascadeDependenciesResolver)
    /// +Check that link have dependencies. Exception on error (reference to non-existent link, update link with children, delete link with children). 
    /// 
    /// +GetOrCreate optimization. +(as extension)
    /// +NonExistentReferencesCreator. +(as extension and decorator DefaultLinksNonExistentReferencesCreator)(!experiment)
    /// +PointAutoCreator +(DefaultLinksNullToSelfReferenceResolver)
    /// 
    /// !!! Hybrids or raw numbers. Allow or dissallow to use higher half of address as actual value.
    /// Make a layer that checks each link to exist or to be external
    /// 
    /// Make non-directed links possible as option.
    /// Triple links using the same options.
    /// 
    /// Allow both stack and stackless models of memory managment. (Now only stack model is supported correctly). The stackless model allows to put a link in any place of available address space, and does not require Create/Delete methods.
    /// </remarks>
    public class Links<T> : LinksBase<T, bool, int>, ILinks<T>
    {
        public Links(ILinksMemoryManager<T> memory, ILinksCombinedConstants<bool, T, int> constants) : base(memory, constants) {}
        public Links(ILinksMemoryManager<T> memory) : base(memory) {}

        public IList<T> GetLink(T link) => Memory.GetLinkValue(link);

        public T Count(params T[] restrictions) => Memory.Count();

        public bool Each(Func<IList<T>, bool> handler, IList<T> restrictions) => Memory.Each(link => handler(Memory.GetLinkValue(link)), restrictions);

        public T Create() => Memory.AllocateLink();

        public T Update(IList<T> restrictions)
        {
            Memory.SetLinkValue(restrictions);
            return restrictions[Constants.IndexPart];
        }

        public void Delete(T link) => Memory.FreeLink(link);
    }
}
