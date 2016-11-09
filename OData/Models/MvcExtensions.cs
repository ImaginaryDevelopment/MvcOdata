using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using OData.Models;

namespace OData.Mvc { 

    public static class MvcExtensions
    {
        public static T GetAdditionalValue<T, U>(this ModelMetadata modelMetadata, string key, Func<U, T> func, T defaultValue = default(T))
        {
            if (!modelMetadata.AdditionalValues.ContainsKey(key))
            {
                return defaultValue;
            }

            var metadataValue = (U)modelMetadata.AdditionalValues[key];

            return func(metadataValue);
        }

        public static IEnumerable<IGrouping<string, ModelMetadata>> GetFormGroups(this ModelMetadata modelMetadata, ViewDataDictionary viewData)
        {
            PropertyGroupContainerDescriptor propertyGroupContainerDescriptor = null;

            if (viewData.ModelMetadata.AdditionalValues.ContainsKey("PropertyGroupContainerDescriptor"))
            {
                propertyGroupContainerDescriptor = (PropertyGroupContainerDescriptor)viewData.ModelMetadata.AdditionalValues["PropertyGroupContainerDescriptor"];
            }

            // if the property group container descriptor is null AND the Model Type is object, we can try to reflect the actual model type and its custom attributes.
            if (propertyGroupContainerDescriptor == null && modelMetadata.ModelType == typeof(object))
            {
                var type = modelMetadata.Model.GetType();

                var attribute = type.GetCustomAttributes(typeof(PropertyGroupContainerAttribute), true).FirstOrDefault() as PropertyGroupContainerAttribute;

                if (attribute != null)
                {
                    propertyGroupContainerDescriptor = new PropertyGroupContainerDescriptor
                    {
                        HtmlClass = attribute.HtmlClass,
                        PropertyGroupSequence =
                                                                   (attribute.GroupDisplaySequence ?? "").Split(',').ToArray()
                    };
                }
            }

            // if we still fail, there's no way to layout out the form in the appropriate groups so we need to blow up.
            if (propertyGroupContainerDescriptor == null)
            {
                throw new Exception("No Property Group Container Descriptor is defined.");
            }

            var groups =
                viewData.ModelMetadata.Properties.Where(
                    p =>
                    !p.HideSurroundingHtml && p.PropertyName != "Actions" && p.AdditionalValues.ContainsKey("PropertyGroupDescriptor")
                    && !viewData.TemplateInfo.Visited(p))
                        .OrderBy(p => p.GetAdditionalValue<int, PropertyGroupDescriptor>("PropertyGroupDescriptor", x => x.DisplayOrder))
                        .GroupBy(p => p.GetAdditionalValue<string, PropertyGroupDescriptor>("PropertyGroupDescriptor", x => x.HtmlClass))
                        .ToList();

            propertyGroupContainerDescriptor.SortGroups(groups);

            return groups;
        }

        public static IEnumerable<ModelMetadata> GetHiddenProperties(this ModelMetadata modelMetadata)
        {
            var hiddenProperties = modelMetadata.Properties.Where(x => x.TemplateHint == "HiddenInput" || x.TemplateHint == "MultiHidden");

            return hiddenProperties;
        }


        public static bool HasLabel(this ModelMetadata modelMetadata)
        {
            return !modelMetadata.AdditionalValues.ContainsKey("RenderLabel") || (bool)modelMetadata.AdditionalValues["RenderLabel"];
        }

        /// <summary>
        ///     Converts an IEnumerable to a SelectList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">IEnumberable list</param>
        /// <param name="text">Displayed text</param>
        /// <param name="value">Value</param>
        /// <param name="defaultOption">Selected item</param>
        /// <returns>SelectList</returns>
        public static IEnumerable<SelectListItem> ToSelectList<T>(
            this IEnumerable<T> enumerable, Func<T, string> text, Func<T, string> value, string defaultOption)
        {
            var items = enumerable.Select(f => new SelectListItem { Text = text(f).ToString(), Value = value(f).ToString() }).ToList();
            items.Insert(0, new SelectListItem { Text = defaultOption, Value = "-1" });

            return items;

            // ie. of use:
            //var departmentItems = departments.ToSelectList(d => d.Code + " - " + d.Description,d => d.Id.ToString()," - ");
        }

        /// <summary>
        ///     Converts an IEnumerable to a SelectList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">IEnumberable list</param>
        /// <param name="text">Displayed text</param>
        /// <param name="value">Value</param>
        /// <returns>SelectList</returns>
        public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> enumerable, Func<T, string> text, Func<T, string> value)
        {
            var items = enumerable.Select(f => new SelectListItem { Text = text(f).ToString(), Value = value(f).ToString() }).ToList();

            return items;

            // ie. of use:
            //var departmentItems = departments.ToSelectList(d => d.Code + " - " + d.Description,d => d.Id.ToString()," - ");
        }

    }

    public class ItemSourceDescriptor
    {
        public bool IncludeSelectAll { get; set; }

        public string ItemSourceName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SecurityTokenAttribute : Attribute
    {
        public string TokenName { get; set; }
    }

    public class ModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(
            IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            var metadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);

            var attributeList = new List<Attribute>(attributes);

            var uiHintAttribute = attributeList.OfType<UIHintAttribute>().FirstOrDefault();

            var itemSourceAttribute = attributeList.OfType<ItemSourceAttribute>().FirstOrDefault();

            if (itemSourceAttribute != null)
            {
                metadata.AdditionalValues.Add(
                    "ItemSourceDescriptor",
                    new ItemSourceDescriptor { IncludeSelectAll = itemSourceAttribute.AllSelection, ItemSourceName = itemSourceAttribute.ItemSourceName });

                // If no UIHint was specified, properties with an ItemSource attribute should get the DropDown template.
                if (uiHintAttribute == null)
                {
                    metadata.TemplateHint = itemSourceAttribute.MultiSelect ? "MultiSelector" : "DropDown";
                }
            }

            var propertyGroupContainerAttribute = attributeList.OfType<PropertyGroupContainerAttribute>().FirstOrDefault();

            if (propertyGroupContainerAttribute != null)
            {
                metadata.AdditionalValues.Add(
                    "PropertyGroupContainerDescriptor",
                    new PropertyGroupContainerDescriptor
                    {
                        HtmlClass = propertyGroupContainerAttribute.HtmlClass,
                        PropertyGroupSequence =
                                (propertyGroupContainerAttribute.GroupDisplaySequence ?? "").Split(',').ToArray()
                    });
            }

            var propertyGroupAttribute = attributeList.OfType<PropertyGroupAttribute>().FirstOrDefault();

            if (propertyGroupAttribute != null)
            {
                metadata.AdditionalValues.Add(
                    "PropertyGroupDescriptor",
                    new PropertyGroupDescriptor { HtmlClass = propertyGroupAttribute.HtmlClass, DisplayOrder = propertyGroupAttribute.DisplayOrder });
            }

            var securityTokenAttribute = attributeList.OfType<SecurityTokenAttribute>().FirstOrDefault();

            if (securityTokenAttribute != null)
            {
                metadata.AdditionalValues.Add("SecurityToken", securityTokenAttribute.TokenName);
            }

            if (string.IsNullOrEmpty(metadata.DisplayName))
            {
                metadata.DisplayName = metadata.PropertyName.Humanize();
            }

            return metadata;
        }
    }

    public static class HtmlExtensions
    {
        public static MvcHtmlString KoDatabind(this HtmlHelper h, string text)
        {
            return new MvcHtmlString("data-bind=\"" + text + "\"");
        }

        public static IHtmlString JsVar(this HtmlHelper helper, string name, object toSerialize)
        {
            return new MvcHtmlString("var " + name + " = " + JsonConvert.SerializeObject(toSerialize) + ";");
        }

        public static IHtmlString Json(this HtmlHelper helper, object o)
        {
            return helper.Raw(JsonConvert.SerializeObject(o));
        }

        public static MvcHtmlString KoDatabindDisplay(this HtmlHelper h, string valueProperty, string otherKo = null)
        {
            Func<string, string> displayFunc = s => (s.EndsWith("id", StringComparison.InvariantCultureIgnoreCase) ? s.Substring(0, s.Length - 2) : s) + "Display";
            otherKo = otherKo.IsNullOrEmpty() ? string.Empty : ("," + otherKo);
            var text = "text:" + displayFunc(valueProperty) + otherKo;
            return h.KoDatabind(text);
        }
    }

}