using System;
using System.Reflection;

namespace PP_Helper.Utils
{
    static class ReflectionUtils
    {
        public static MethodInfo GetPrivateMethod(Object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
