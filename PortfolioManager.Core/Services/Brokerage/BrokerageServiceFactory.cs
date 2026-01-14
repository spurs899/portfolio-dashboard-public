using PortfolioManager.Contracts.Models.Shared;

namespace PortfolioManager.Core.Services.Brokerage;

public interface IBrokerageServiceFactory
{
    IBrokerageService GetBrokerageService(BrokerageType brokerageType);
    IEnumerable<IBrokerageService> GetAllBrokerageServices();
}

public class BrokerageServiceFactory : IBrokerageServiceFactory
{
    private readonly IEnumerable<IBrokerageService> _brokerageServices;

    public BrokerageServiceFactory(IEnumerable<IBrokerageService> brokerageServices)
    {
        _brokerageServices = brokerageServices;
    }

    public IBrokerageService GetBrokerageService(BrokerageType brokerageType)
    {
        var service = _brokerageServices.FirstOrDefault(s => s.BrokerageType == brokerageType);
        
        if (service == null)
            throw new NotSupportedException($"Brokerage type {brokerageType} is not supported");

        return service;
    }

    public IEnumerable<IBrokerageService> GetAllBrokerageServices()
    {
        return _brokerageServices;
    }
}
