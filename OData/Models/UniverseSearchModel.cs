using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace OData.Models
{
    #region Convention plumbing

    [Serializable]
    [DataContract]
    /// Requires loading the related lookup data into ViewBag.ItemSources
    public class ItemSource
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public bool IsSelected { get; set; }
    }

    public class GridColumn
    {
        public string Name { get; set; }

        public string PropertyName { get; set; }

        public string FieldTemplate { get; set; }

        public string CssClass { get; set; }

        public bool Sortable { get; set; }

        public string SortField { get; set; }
    }

    public class SearchModel<TSearchForm> : SearchModel
    where TSearchForm : new()
    {
        public TSearchForm SearchForm { get; set; }

        public List<ItemSource> ItemSources { get; set; }

        public SearchModel()
        {
            SearchForm = new TSearchForm();
        }
    }

    [Serializable]
    public class ActionModel
    {
        public ActionModel()
        {
            this.HttpVerb = HttpVerbs.Get;
        }

        public string Action { get; set; }

        public string Controller { get; set; }

        public string Id { get; set; }

        public string QueryString { get; set; }

        public string Url { get; set; }

        public string Icon { get; set; }

        public string CssClass { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public string Description { get; set; }

        public HttpVerbs HttpVerb { get; set; }

        public string GetUrl(UrlHelper urlHelper)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                return Url;
            }

            if (!string.IsNullOrEmpty(Id))
            {
                return urlHelper.Action(Action, Controller, new { id = Id });
            }

            return urlHelper.Action(Action, Controller) + QueryString;
        }
    }

    public class SearchModel
    {
        public List<GridColumn> Columns { get; protected set; }

        public List<ActionModel> GroupActions { get; set; }

        public string DataSourceUrl { get; set; }

        public string DataIdProperty { get; set; }

        public string RowTemplateName { get; set; }

        public string ConfirmTemplateName { get; set; }

        public List<SelectListItem> ItemsPerPage { get; set; }

        public List<SelectListItem> SortableItems { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public string SortField { get; set; }

        public string SortDirection { get; set; }

        public SearchModel()
        {
            GroupActions = new List<ActionModel>();
            Columns = new List<GridColumn>();
            SortableItems = new List<SelectListItem>();
            ItemsPerPage = new List<SelectListItem>();
        }
    }

    [Serializable]
    public class FormModel
    {
        [ScaffoldColumn(false)]
        public IList<ActionModel> Actions { get; set; }

        public FormModel()
        {
            Actions = new List<ActionModel>();
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class PropertyGroupContainerAttribute : Attribute
    {
        public string HtmlClass { get; set; }

        public string GroupDisplaySequence { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ItemSourceAttribute : Attribute
    {
        private readonly string _itemSourceName;

        private readonly bool _allSelection;

        private readonly bool _multiSelect;

        public bool AllSelection
        {
            get
            {
                return _allSelection;
            }
        }

        public string ItemSourceName
        {
            get
            {
                return _itemSourceName;
            }
        }

        public bool MultiSelect
        {
            get
            {
                return _multiSelect;
            }
        }

        public ItemSourceAttribute(string itemSourceName, bool allSelection = false, bool multiSelect = false)
        {
            _itemSourceName = itemSourceName;
            _multiSelect = multiSelect;
            _allSelection = allSelection;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyGroupAttribute : Attribute
    {
        public int DisplayOrder { get; set; }

        public string HtmlClass { get; set; }
    }

    public class PropertyGroupContainerDescriptor
    {
        public string HtmlClass { get; set; }

        public string[] PropertyGroupSequence { get; set; }

        public void SortGroups(List<IGrouping<string, ModelMetadata>> metadataGroups)
        {
            metadataGroups.Sort(
                (x, y) =>
                {
                    var sequenceList = PropertyGroupSequence.ToList();

                    var indexOfX = sequenceList.IndexOf(x.Key);
                    var indexOfY = sequenceList.IndexOf(y.Key);

                    if (indexOfX < indexOfY)
                    {
                        return -1;
                    }

                    if (indexOfX > indexOfY)
                    {
                        return 1;
                    }

                    return 0;
                });
        }
    }

    public class PropertyGroupDescriptor
    {
        public int DisplayOrder { get; set; }

        public string HtmlClass { get; set; }
    }

    #endregion

    // appears this interface is intended to be shared with the actual model
    // consisting of only the searchable props
    public interface IUniverseSearchable
    {
        // pk is typically not searchable
        //public byte UniverseID { get; set; }
        string ShortName { get; }
        string LongName { get; }
        string HtmlMap { get; }
        DateTime Created { get; }
        DateTime? Modified { get; }

    }

    [PropertyGroupContainer(GroupDisplaySequence = "first,second", HtmlClass = "two-column")]
    public class UniverseSearchForm : FormModel, IUniverseSearchable
    {
        [PropertyGroup(HtmlClass = "first")]
        [UIHint("Dropdown")]
        // TODO: demonstrate usage of this property
        //[ItemSource("ShortName")]
        public string ShortName { get; set; }
        [PropertyGroup(HtmlClass = "second")]
        public string LongName { get; set; }
        [PropertyGroup(HtmlClass = "first")]
        [Display(Name ="Map")]
        public string HtmlMap { get; set; }
        [PropertyGroup(HtmlClass = "second")]
        public DateTime Created { get; set; }
        [PropertyGroup(HtmlClass = "first")]
        public DateTime? Modified { get; set; }

    }

    public class UniverseSearchModel : SearchModel<UniverseSearchForm>
    {
    }
}