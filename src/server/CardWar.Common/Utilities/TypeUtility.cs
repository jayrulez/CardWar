using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Common.Utilities
{
    public static class TypeUtility
    {
        public static string GetTypeName<T>()
        {
            return typeof(T).Name;
        }
        
        public static string GetTypeName(object type)
        {
            return type.GetType().Name;
        }
    }
}
