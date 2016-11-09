using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OData
{
    public static class Links
    {
        public static class Bundles
        {
            public static class Scripts
            {
                public const string jqueryui = "~/Scripts/bundles/jqueryui";
                public const string knockout = "~/scripts/bundles/knockout";
            }
        }

        public static class Scripts
        {
            public const string Path = "~/" + nameof(Scripts);
            public static class KO
            {
                public const string Path = Scripts.Path + "/ko";
                public const string Knockout = Path + "/knockout-3.4.0.js";
                public const string kosearch_js = Path + "/kosearch.js";
                public const string knockout_extensions_js = Path + "/knockout-extensions.js";

            }
        }
    }

    public static class MvcT4
    {
        public static class Views
        {
            public const string Path = "~/" + nameof(Views);
            public static class Shared
            {
                public const string Path = Views.Path + "/" + nameof(Shared);
                public static class KOSearch
                {
                    public const string Path = Shared.Path + "/kosearch";
                    public const string KOItem = Path + "/koitem.cshtml";
                    public const string FilterUi = Path + "/FilterUi.cshtml";
                    public const string GroupActions = Path + "/GroupActions.cshtml";
                    public const string KOColumns = Path + "/kocolumns.cshtml";
                    public const string KOFiltering = Path + "/kofiltering.cshtml";
                    public const string KoContext = Path + "/KoContext.cshtml";
                }
            }

        }

    }
}