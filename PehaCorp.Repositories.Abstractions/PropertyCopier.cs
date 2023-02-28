using System.Linq;

namespace PehaCorp.Repositories.Abstractions
{
    public class PropertyCopier<TParent, TChild> where TParent : class
        where TChild : class
    {
        public static void Copy(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }

        public static void Copy<T>(T parent, T child, string[] exludeProperties) where T : class
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType && exludeProperties.All(x => x != parentProperty.Name))
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }
    }

    public static class PropCopierExtensions
    {
        public static void CopyPropertiesFrom<T>(this T targetItem, T dataItem) where T : class
        {
            PropertyCopier<T, T>.Copy(dataItem, targetItem);
        }
        
        public static void CopyPropertiesFrom<T>(this T targetItem, T dataItem, params string[] exludeProperties) where T : class
        {
            PropertyCopier<T, T>.Copy(dataItem, targetItem, exludeProperties);
        }
    }
}