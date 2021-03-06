using SmartQuant.Providers;
using System;
namespace OpenQuant.API
{
	public static class ProviderManager
	{
		public static ProviderList Providers
		{
			get;
			private set;
		}
		static ProviderManager()
		{
			ProviderManager.Providers = new ProviderList();
			foreach (IProvider provider in SmartQuant.Providers.ProviderManager.Providers)
			{
				if (provider is IMarketDataProvider)
				{
					ProviderManager.Providers.Add(new MarketDataProvider(provider as IMarketDataProvider));
				}
				else
				{
					if (provider is IExecutionProvider)
					{
						ProviderManager.Providers.Add(new ExecutionProvider(provider as IExecutionProvider));
					}
					else
					{
						if (provider is IHistoricalDataProvider)
						{
							ProviderManager.Providers.Add(new HistoricalDataProvider(provider as IHistoricalDataProvider));
						}
						else
						{
							if (provider is IInstrumentProvider)
							{
								ProviderManager.Providers.Add(new InstrumentProvider(provider as IInstrumentProvider));
							}
						}
					}
				}
			}
			SmartQuant.Providers.ProviderManager.Added += new ProviderEventHandler(ProviderManager.ProviderManager_Added);
		}
		private static void ProviderManager_Added(ProviderEventArgs args)
		{
			ProviderManager.Providers.Add(new Provider(args.Provider));
		}
	}
}
