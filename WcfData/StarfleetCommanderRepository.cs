using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfData
{
	using Contracts;

	public class StarfleetCommanderRepository : IStarfleetCommander
	{
		readonly Func<MaslowJax_dbsEntities> _dbFactory;

		public StarfleetCommanderRepository(Func<MaslowJax_dbsEntities> dbFactory)
		{
			_dbFactory = dbFactory;
		}

		public IQueryable<Contracts.Universe> Universes { get
		{
			return _dbFactory().Universes.Select(u=>new Contracts.Universe{ UniverseID = u.UniverseId, ShortName = u.ShortName, LongName = u.LongName, HtmlMap = u.HtmlMap});
		} }

	
	}
}