﻿#if !NETFRAMEWORK
using Cosmos.Reflection.ObjectVisitors.Core;

namespace Cosmos.Reflection.ObjectVisitors.SlimSupported.AnonymousServices
{
    internal static class AnonymousServiceSlimObjectCallerBuilder
    {
        public static unsafe ObjectCallerBase Ctor(Type type)
        {
            var caller = AnonymousServiceTypeHelper.Create(type);

            if (caller is null)
                throw new InvalidOperationException("Is not a valid anonymous type.");

            return caller;
        }
    }

    internal static class AnonymousServiceSlimObjectCallerBuilder<T>
    {
        public static Func<ObjectCallerBase> Ctor => () => AnonymousServiceSlimObjectCallerBuilder.Ctor(typeof(T));
    }
}
#endif