using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Helpers
{
    /// <summary>
    ///     Helper class to bind an Enum type as an control's ItemsSource.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumItemsSource<T> where T : struct, IConvertible
    {
        public string FullTypeString { get; set; }
        public string Name { get; set; }
        public string LocalizedName { get; set; }
        public T Value { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="fullString"></param>
        public EnumItemsSource(string name, T value, string fullTypeString)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("EnumItemsSource only accept Enum type.");

            Name = name;
            Value = value;
            FullTypeString = fullTypeString;

            // Retrieve localized name
            //if(Windows.UI.Core.CoreWindow.GetForCurrentThread() != null)

                //var ctx = new Microsoft.Windows.ApplicationModel.Resources.ResourceContext();
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("EnumNames");
            LocalizedName = resourceLoader.GetString(FullTypeString.Replace('.', '-'));

            //else LocalizedName = name;
        }

        /// <summary>
        ///     Create a list of EnumItemsSource from an enum type.
        /// </summary>
        /// <returns></returns>
        public static List<EnumItemsSource<T>> ToList()
        {
            // Put to lists
            var namesList = Enum.GetNames(typeof(T));
            var valuesList = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            // Create EnumItemsSource list
            var enumItemsSourceList = new List<EnumItemsSource<T>>();
            for (int i = 0; i < namesList.Length; i++)
                enumItemsSourceList.Add(new EnumItemsSource<T>(namesList[i], valuesList[i], $"{typeof(T).Name}.{namesList[i]}"));

            return enumItemsSourceList;
        }
    }
}
