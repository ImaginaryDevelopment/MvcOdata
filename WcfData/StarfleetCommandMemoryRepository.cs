using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfData
{
	public class StarfleetCommandMemoryRepository : IStarfleetCommander
	{
		private Contracts.Universe[] _universes = new[] { new Contracts.Universe() { UniverseID = 1, HtmlMap = "oh hai", LongName = "My Universe", ShortName = "MU" } };
		public IQueryable<Contracts.Universe> Universes
		{
			get
			{
				return _universes.AsQueryable();
			}
		}
	}
}
