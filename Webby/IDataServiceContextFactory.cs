using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Webby
{
	using Contracts;

	public interface IDataServiceContextFactory<out TService>
	{
		TService GetContext(IConfig config, string instance = null);
	}
}
