using System;
using System.Collections.Generic;
using Platform.Helpers;
using Platform.Helpers.Collections;

namespace Platform.Data.Core.Doublets
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
    public class Links<T> : LinksBase<T, T, int>, ILinks<T>
    {
        public Links(ILinksMemoryManager<T> memory, ILinksCombinedConstants<T, T, int> constants) : base(memory, constants) { }
        public Links(ILinksMemoryManager<T> memory) : base(memory) { }

        public T Count(params T[] restriction) => Memory.Count(restriction);

        

        public T Each(Func<IList<T>, T> handler, IList<T> restrictions)
        {
            // TODO: Move to UniLinksExtensions
            //return Trigger(restrictions, (before, after) => handler(before), null, null);
            //// Trigger(restrictions, null, restrictions, null); - Должно быть синонимом

            return Memory.Each(link => !Equals(handler(Memory.GetLinkValue(link)), Constants.Break), restrictions) ? Constants.Continue : Constants.Break;
        }
        
        public T Create()
        {
            // TODO: Move to UniLinksExtensions
            //// { 0, 0, 0 } => { ifself, 0, 0 }
            //// { 0 } => { ifself, 0, 0 } *

            //T result = default(T);

            //Func<IList<T>, IList<T>, T> substitutedHandler = (before, after) =>
            //{
            //    result = after[Constants.IndexPart];
            //    return Constants.Continue;
            //};

            //// Сейчас будет полагать что соответствие шаблону (ограничению) произойдёт только один раз
            //Trigger(new[] { Constants.Null }, null,
            //        new[] { Constants.Itself, Constants.Null, Constants.Null }, substitutedHandler);

            //// TODO: Возможна реализация опционального поведения (один ноль-пустота, бесконечность нолей-пустот)
            //// 0 => 1

            //// 0 => 1
            //// 0 => 2
            //// 0 => 3
            //// ...

            //return result;

            return Memory.AllocateLink();
        }

        public T Update(IList<T> restrictions)
        {
            // TODO: Move to UniLinksExtensions
            //// { 1, any, any } => { 1, x, y }
            //// { 1 } => { 1, x, y } *
            //// { 1, 3, 4 }

            //Trigger(new[] { restrictions[Constants.IndexPart] }, null, 
            //        new[] { restrictions[Constants.IndexPart], restrictions[Constants.SourcePart], restrictions[Constants.TargetPart] }, null);

            //return restrictions[Constants.IndexPart];

            Memory.SetLinkValue(restrictions);
            return restrictions[Constants.IndexPart];
        }

        public void Delete(T link)
        {
            // TODO: Move to UniLinksExtensions
            //// { 1 } => { 0, 0, 0 }
            //// { 1 } => { 0 } *
            //Trigger(new[] { link }, null,
            //        new[] { Constants.Null }, null);

            Memory.FreeLink(link);
        }
    }
}
