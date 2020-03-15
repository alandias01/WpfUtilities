using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace WpfUtilities
{
    public static class MyExtensionMethods
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            items.ToList().ForEach(collection.Add);
        }

        public static string[] BreakStringIntoArrayEveryNspaces(this string text, int lineLength)
        {
            return Regex.Matches(text, ".{1," + lineLength + "}").Cast<Match>().Select(m => m.Value).ToArray();
        }
                
        
    }
}
