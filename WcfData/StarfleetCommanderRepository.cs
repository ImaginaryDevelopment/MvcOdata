using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfData
{
	using Contracts;

    public class StarfleetCommanderRepository : IStarfleetCommander
    {
       // private readonly Func<dbsEntities> _dbFactory;

        private static IQueryable<Contracts.Universe> InMemoryRepositoryForDemos =
            new[]
                {
                    new Contracts.Universe() {UniverseID = 1, HtmlMap = "StarfleetCommander", ShortName = "sfc"},
                    new Contracts.Universe() {UniverseID = 2,HtmlMap = "SfcExtreme",ShortName = "sfe"},
                    new Contracts.Universe() {UniverseID = 3,HtmlMap = "Sfc2",ShortName = "sf2"},
                    new Contracts.Universe() {UniverseID = 4,HtmlMap = "Sfc3",ShortName = "sf3"},
                    new Contracts.Universe() {UniverseID = 5,HtmlMap = "Sfc4",ShortName = "sf4"},
                    new Contracts.Universe() {UniverseID = 6,HtmlMap = "Sfc5",ShortName = "sf5"},
                }.AsQueryable();

    //public StarfleetCommanderRepository() //Func<dbsEntities> dbFactory)
    //    {
    //        //_dbFactory = dbFactory;
    //    }

		public IQueryable<Contracts.Universe> Universes { get
		{
			//return _dbFactory().Universes.Select(u=>new Contracts.Universe{ UniverseID = u.UniverseId, ShortName = u.ShortName, LongName = u.LongName, HtmlMap = u.HtmlMap});
		    return InMemoryRepositoryForDemos;

		} }

	
	}
}