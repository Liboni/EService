
namespace SamhashoService.Extensions
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public static class Extension
    {
        public static Dictionary<string, string> ToDictionary(this object myObj)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (myObj != null)
            {
                foreach (PropertyDescriptor descriptor in
                    TypeDescriptor.GetProperties(myObj))
                {
                    object propValue = descriptor?.GetValue(myObj);
                    if (propValue != null)
                    {
                        dictionary.Add(descriptor.Name, $"{propValue}");
                    }
                }
            }

            return dictionary;
        }
    }
}