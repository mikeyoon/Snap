﻿/*
Snap v1.0

Copyright (c) 2010 Tyler Brinks

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using Castle.DynamicProxy;
using StructureMap;
using StructureMap.Interceptors;

namespace Snap.StructureMap
{
    /// <summary>
    /// StructureMap type interceptor
    /// </summary>
    public class StructureMapDefinedAspectInterceptor : TypeInterceptor
    {
        private readonly IContainer _container;
        private readonly ProxyFactory _proxyFactory = new ProxyFactory(new ProxyGenerator());

        /// <summary>
        /// Gets or sets the configuration.
        /// 
        /// @Update:
        ///         10-11-2010 Donald Hamm. Added support for StructureMap 2.5.x
        /// </summary>
        /// <value>The configuration.</value>
        internal AspectConfiguration Configuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapDefinedAspectInterceptor"/> class.
        /// </summary>
        public StructureMapDefinedAspectInterceptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapDefinedAspectInterceptor"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public StructureMapDefinedAspectInterceptor(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Gets the master proxy.
        /// 
        /// @Update:
        ///         10-11-2010 Donald Hamm. Created this to switch how we get 
        ///         the instance of the MasterProxy.
        /// </summary>
        /// <returns></returns>
        private IMasterProxy GetMasterProxy()
        {
            return _container == null 
                ? ObjectFactory.GetInstance<IMasterProxy>() 
                : _container.GetInstance<IMasterProxy>();
        }

        /// <summary>
        /// Wraps interfaces in a Castle dynamic proxy
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public object Process(object target, IContext context)
        {
            var proxy = GetMasterProxy();

            var name = target.GetType().FullName;

            if (proxy.Configuration.Interceptors.Count > 0 && target.IsDecorated(proxy.Configuration) && !name.EndsWith("Proxy")) //Don't create proxies of proxies
            {
                return _proxyFactory.CreateProxy(target, proxy);
            }

            // Don't build up any wrapped proxy types.
            if (!(name.StartsWith("Castle.Proxies.") && name.EndsWith("Proxy")))
            {
                 context.BuildUp(target);
            }
            return target;
        }

        /// <summary>
        /// Matcheses types in the a namespace that implement IInterceptAspect.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public bool MatchesType(Type type)
        {
            return _proxyFactory.GetInterfaceToProxy(type, Configuration) != null;
        }
    }
}