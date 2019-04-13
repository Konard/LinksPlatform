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
            var valueLink = Links.All(objectProperty).SingleOrDefault();
            if(Equals(valueLink, default(TLink)))
                return default;
            var value = Links.GetTarget(valueLink);
            return value;
        }

        public void SetValue(TLink @object, TLink property, TLink value)
        {
            var objectProperty = Links.GetOrCreate(@object, property);
            Links.DeleteMany(Links.All(objectProperty));
            Links.GetOrCreate(objectProperty, value);
        }
    }
}
