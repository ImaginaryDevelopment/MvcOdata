using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contracts
{
	public interface IStartfleetCommander
	{
		IQueryable<Universe> Universes { get; }
		
	}
}
