using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using System.Web.Http.OData;
using Contracts;
namespace OdataGenerator
{
    using System.Web.Http.OData.Builder;
    
    public static class ODataConventionModelBuilderEnabler
    {
        public static void WireModels(ODataConventionModelBuilder mb)
        {
            mb.EntitySet<Contracts.Universe>("Universes");
        }
    }
}

// ReSharper disable RedundantNameQualifier
namespace OData.Controllers.OData
{
    /// <summary>
    /// Controller:<see cref="OData.Controllers.UniverseController" />
    /// SearchModel:<see cref="Models.UniverseSearchModel"/>
    /// SearchForm:<see cref="Models.UniverseSearchForm"/>
    /// OData Controller: <see cref="OData.Controllers.OData.UniversesController"/>
    /// OData Contract:<see cref="Contracts.IStarfleetCommander"/>
    /// OData Model:<see cref="Contracts.Universe"/>
	/// Odata: <see href="http://localhost:4339/OData/Universes?$top=10&$inlinecount=allpages"/>
    /// Svc: <see href="http://localhost:4339/StarfleetCommander.svc/"/>
    /// Svc JSON: <see href="http://localhost:4339/StarfleetCommander.svc?$format=json"/>
    /// Svc Model: <see href="http://localhost:4339/StarfleetCommander.svc/Universes"/>
    /// Svc Model JSON: <see href="http://localhost:4339/StarfleetCommander.svc/Universes?$format=json"/>
    /// </summary>
    public class UniversesController : ODataController
    {
        private readonly Contracts.IStarfleetCommander _starfleetCommander;

        public UniversesController(Contracts.IStarfleetCommander starfleetCommander)
        {
            _starfleetCommander = starfleetCommander;
        }

        public static Func<Expression<Func<Contracts.Universe, object>>, string> PropertyNameHelper
        {
            get
            {
                return LinqOp.PropertyNameHelper<Contracts.Universe>();
            }
        }

        public const string OdataSegment = "Universes";

        public static Type ModelType
        {
            get
            {
                return typeof(Contracts.Universe);
            }
        }

        [Queryable]
        public IQueryable<Contracts.Universe> Get()
        {
            return _starfleetCommander.Universes;
        }
    }
}
// ReSharper restore RedundantNameQualifier


