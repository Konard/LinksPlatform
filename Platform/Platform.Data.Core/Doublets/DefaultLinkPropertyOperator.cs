using System.Linq;
using Platform.Helpers;

namespace Platform.Data.Core.Doublets
{
    public class DefaultLinkPropertyOperator<TLink> : LinksOperatorBase<TLink>, IPropertyOperator<TLink, TLink, TLink>
    {
        public DefaultLinkPropertyOperator(ILinks<TLink> links) : base(links)
        {
        }

        public TLink GetValue(TLink @object, TLink property)
        {
            var objectProperty = Links.SearchOrDefault(@object, property);
            if (Equals(objectProperty, default(TLink)))
                return default;
            var valueLink = Links.All(Links.Constants.Any, objectProperty).SingleOrDefault();
            if (valueLink == null)
                return default;
            var value = Links.GetTarget(valueLink[Links.Constants.IndexPart]);
            return value;
        }

        public void SetValue(TLink @object, TLink property, TLink value)
        {
            var objectProperty = Links.GetOrCreate(@object, property);
            Links.DeleteMany(Links.All(Links.Constants.Any, objectProperty).Select(x => x[Links.Constants.IndexPart]).ToList());
            Links.GetOrCreate(objectProperty, value);
        }
    }
}
