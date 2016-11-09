using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OData.Models
{

    public class KoGridModel
    {
        public string GroupActionKey { get; set; }
        public string OdataSegment { get; set; }
        public string PrimaryKey { get; set; }
        public IEnumerable<string> Specs { get; set; }
        public MvcHtmlString BeforeKoScript { get; set; }
        public MvcHtmlString BeforeBindingScript { get; set; }
        /// <summary>
        /// Metadata used to generate the Search Form(Filter) part of the grid
        /// </summary>
        public IEnumerable<ModelMetadata> FilterMetadata { get; set; }
        public Type ModelType { get; set; }
        public SearchModel SearchModel { get; set; }
    }
}