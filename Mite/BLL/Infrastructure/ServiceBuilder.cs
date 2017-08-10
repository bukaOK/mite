using Autofac;
using Mite.BLL.Core;

namespace Mite.BLL.Infrastructure
{
    public interface IServiceBuilder
    {
        TService Build<TService>() where TService : IDataService;
    }
    public class ServiceBuilder : IServiceBuilder
    {
        private readonly IComponentContext container;

        public ServiceBuilder(IComponentContext diContainer)
        {
            container = diContainer;
        }

        public TService Build<TService>() where TService : IDataService
        {
            return container.Resolve<TService>();
        }
    }
}