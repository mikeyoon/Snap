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
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;

namespace Snap.CastleWindsor
{
    /// <summary>
    /// Facility for Castle interceptor registration
    /// </summary>
    public class CastleAspectFacility : AbstractFacility
    {
        /// <summary>
        /// Initializes the facility.
        /// </summary>
        protected override void Init()
        {
            Kernel.ComponentRegistered += KernelComponentRegistered;
        }
        /// <summary>
        /// Registers interceptors with the target type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="handler">The handler.</param>
        private void KernelComponentRegistered(string key, IHandler handler)
        {
            // Ignore any types implementing IAttributeInterceptor or IInterceptor
            if(handler.Service.GetInterfaces().Any(i => i.FullName.Contains("Snap.IAttributeInterceptor")
                || i.FullName.Contains("IInterceptor")))
            {
                return;
            }

            //var proxy = (MasterProxy)Kernel[typeof (MasterProxy)];
            var proxy = Kernel.Resolve<MasterProxy>();

            if (proxy.Configuration.Interceptors.Count > 0)
            {
                handler.ComponentModel.Interceptors.AddIfNotInCollection(new InterceptorReference(typeof(MasterProxy)));

                for (var i = 1; i < proxy.Configuration.Interceptors.Count; i++)
                {
                    handler.ComponentModel.Interceptors.Add(new InterceptorReference(typeof(PseudoInterceptor)));
                }
            }
        }
    }
}
