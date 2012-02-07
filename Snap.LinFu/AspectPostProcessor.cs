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
using LinFu.IoC;
using LinFu.IoC.Interfaces;

namespace Snap.LinFu
{
    public class AspectPostProcessor : IPostProcessor
    {
    	private readonly ProxyFactory _proxyFactory = new ProxyFactory(new ProxyGenerator());

        public void PostProcess(IServiceRequestResult result)
        {
            var instance = result.ActualResult;
            // instance cannot be resolved (e.g. not registered in container)
            if (instance == null)
            {
                return;
            }
            var instanceTypeName = instance.GetType().FullName;
           
            // Ignore any LinFu factories or Snap-specific instances.
            if (instanceTypeName.Contains("LinFu.") || instanceTypeName == "Snap.AspectConfiguration"
                || instanceTypeName == "Snap.IMasterProxy" || instanceTypeName == "Snap.MasterProxy")
            {
                return;
            }

            // inteceptors could not be intercepted too, thus skip the code below
            if(instance is IInterceptor)
            {
                return;
            }

            var proxy = result.Container.GetService<IMasterProxy>();

            //Don't bother proxying anything if there are no interceptors
            if (!instance.IsDecorated(proxy.Configuration) || proxy.Configuration.Interceptors.Count <= 0)
            {
                return;
            }

        	result.ActualResult = _proxyFactory.CreateProxy(instance, proxy);
        }
    }
}
