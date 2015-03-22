using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OData.Models
{
    public class ODataBoundLink
    {
        public string BaseUrl { get; set; }
        public string Json { get; set; }
        public string AnchorClass { get; set; }
        public string Href { get; set; }
    }
}