namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using Ocelot.Values;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class RoundRobin : ILoadBalancer
    {
        private readonly Func<Task<List<Service>>> _services;
        private readonly object _lock = new object();

        private int _last;

        public RoundRobin(Func<Task<List<Service>>> services)
        {
            _services = services;
        }

        public async Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
        {
            lock (_lock)
            {
                var services = _services().GetAwaiter().GetResult();
                if (services != null && services.Count > 0) {
                    if (_last >= services.Count)
                    {
                        _last = 0;
                    }

                    var next = services[_last];
                    _last++;
                    return new OkResponse<ServiceHostAndPort>(next.HostAndPort);
                }
                return new ErrorResponse<ServiceHostAndPort>(new UnableToFindDownstreamRouteError(httpContext.Request.Path, httpContext.Request.Method));
            }
        }

        public void Release(ServiceHostAndPort hostAndPort)
        {
        }
    }
}
