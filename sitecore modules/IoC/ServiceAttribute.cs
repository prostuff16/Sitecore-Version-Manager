using System;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.VersionManager.sitecore_modules.IoC
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute()
        {

        }

        public ServiceAttribute(Type serviceType)
        {
            this.ServiceType = serviceType;
        }

        public ServiceLifetime lifetime = ServiceLifetime.Singleton;
        public ServiceLifetime Lifetime
        {

            get { return lifetime; }

            set { lifetime = value; }

        }
        public Type ServiceType { get; set; }
    }
}
