﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Fasterflect;

namespace Snap
{
    /// <summary>
    /// Incapsulates proxy creation logic, delegates actual proxy creation to ProxyGenerator of DynamicProxy2 assembly
    /// </summary>
    public class ProxyFactory
    {
        private readonly ProxyGenerator _proxyGenerator;

        public ProxyFactory(ProxyGenerator proxyGenerator)
        {
            _proxyGenerator = proxyGenerator;
        }

        public object CreateProxy(object instanceToProxy, IMasterProxy masterProxy)
        {
            object proxy;
            var interfaceToProxy = GetInterfaceToProxy(instanceToProxy.GetType(), masterProxy.Configuration);
            var interceptorsOfProxy = GetInterceptors(masterProxy);
            if(interfaceToProxy.IsInterface)
            {
                proxy = _proxyGenerator.CreateInterfaceProxyWithTargetInterface(
                    interfaceToProxy,
                    instanceToProxy,
                    interceptorsOfProxy);
            }
            else
            {
                var greediestCtor = interfaceToProxy.GetConstructors().OrderBy(x => x.Parameters().Count).LastOrDefault();
                var ctorDummyArgs =  greediestCtor == null ? new object[0] : new object[greediestCtor.Parameters().Count];
                proxy = _proxyGenerator.CreateClassProxyWithTarget(
                    interfaceToProxy,
                    instanceToProxy,
                    ctorDummyArgs, 
                    interceptorsOfProxy);
            }
            return proxy;
        }
        
        /// <summary>
        /// Determines the type of DynamicProxy that will be created over the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Type GetInterfaceToProxy(Type type, AspectConfiguration configuration)
        {
            var allInterfaces = type.GetInterfaces();

            IEnumerable<Type> baseClassInterfaces = type.BaseType != null ? type.BaseType.GetInterfaces() : new Type[0];
            IEnumerable<Type> topLevelInterfaces = allInterfaces.Except(baseClassInterfaces);

            if (topLevelInterfaces.Count() == 0)
            {
                var types = new[] { type };
                return types.FirstMatch(configuration.Namespaces);
            }
            else
            { 
                //If the top level interface inherits from the baseclassinterface, then return the top one since that's more specific
                var top = topLevelInterfaces.ToArray().FirstMatch(configuration.Namespaces);
                var bi = baseClassInterfaces.ToArray().FirstMatch(configuration.Namespaces);
                if (top != null && bi != null && top.GetInterfaces().Contains(bi))
                    return top;
            }

            return allInterfaces.FirstMatch(configuration.Namespaces);
        }

        private IInterceptor[] GetInterceptors(IMasterProxy masterProxy)
        {
            return new[] { masterProxy }.Concat(Enumerable
                .Range(1, masterProxy.Configuration.Interceptors.Count - 1)
                .Select(i => new PseudoInterceptor())
                .Cast<IInterceptor>())
                .ToArray();
        }

    }
}