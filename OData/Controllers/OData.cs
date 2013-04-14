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
    /// OData Contract:<see cref="Contracts.IStartfleetCommander"/>
    /// OData Model:<see cref="Contracts.Universe"/>
    /// JIRA: <see href="http://jira/browse/DEV-775"/>
	/// Odata: <see href="http://localhost/webby/OData/Universes?$top=10&$inlinecount=allpages"/>
    /// Svc: <see href="http://localhost/webby/StartfleetCommander.svc/"/>
    /// Svc JSON: <see href="http://localhost/webby/StartfleetCommander.svc?$format=json"/>
    /// Svc Model: <see href="http://localhost/webby/StartfleetCommander.svc/Universes"/>
    /// Svc Model JSON: <see href="http://localhost/webby/StartfleetCommander.svc/Universes?$format=json"/>
    /// </summary>
    public class UniversesController : ODataController
    {
        private readonly Contracts.IStartfleetCommander _startfleetCommander;

        public UniversesController(Contracts.IStartfleetCommander startfleetCommander)
        {
            _startfleetCommander = startfleetCommander;
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
            // TODO: DEV-865 wire up back end to front
            // TODO: DEV-775 wiring
 
            return Enumerable.Empty<Universe>().AsQueryable();

            // return startfleetCommander.Universes;
        }
    }
}
// ReSharper restore RedundantNameQualifier


