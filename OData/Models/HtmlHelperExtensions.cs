using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

using OData.App_Start;
//using MarkdownSharp;


namespace OData.Mvc
{
    public interface IResolveNamed<T>
    {
        T Get(string name);
    }

    public static class HtmlHelperExtensions
    {
        static readonly string VersionTag;
        static T GetNamedService<T>(string name)
        {
            return DependencyResolver.Current.GetService<IResolveNamed<T>>().Get(name);
        }

        static HtmlHelperExtensions()
        {
            VersionTag = Guid.NewGuid().ToString();
        }

        public static MvcHtmlString Image(this HtmlHelper helper, string id = null, string alt = null, string imageName = null, object htmlAttributes = null)
        {
            var tagBuilder = new TagBuilder("img");

            if (id != null)
            {
                tagBuilder.GenerateId(id);
            }

            if (imageName != null)
            {
                var getFullImagePath = GetNamedService<Func<string, string>>(ExpectedServices.Func.String.ReturnsString.GetFullImagePath);
                tagBuilder.MergeAttribute("src", getFullImagePath(imageName)); //ContentDeliveryPathResolver.Current.GetFullImagePath(imageName));
            }

            if (alt != null)
            {
                tagBuilder.MergeAttribute("alt", alt);
            }

            if (htmlAttributes != null)
            {
                var routeValueDictionary = htmlAttributes as RouteValueDictionary;

                if (routeValueDictionary != null)
                {
                    tagBuilder.MergeAttributes(routeValueDictionary);
                }
                else
                {
                    tagBuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
                }
            }

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString Markdown(this HtmlHelper helper, string wikiMarkup)
        {
            var markdownTransformer = GetNamedService<Func<string, string>>(ExpectedServices.Func.String.ReturnsString.MarkdownTransformer);
            return MvcHtmlString.Create(markdownTransformer(wikiMarkup));
        }

        public static MvcHtmlString Css(this HtmlHelper helper)
        {
            var tagBuilder = new TagBuilder("link");

            var url = GetNamedService<string>(ExpectedServices.String.CssEndpoint); // ContentDeliveryPathResolver.Current.CssEndpoint;

            tagBuilder.MergeAttribute("type", "text/css");
            tagBuilder.MergeAttribute("href", url);
            tagBuilder.MergeAttribute("rel", "stylesheet");

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString JavaScript(this HtmlHelper helper)
        {
            var tagBuilder = new TagBuilder("script");

            tagBuilder.MergeAttribute("src", GetNamedService<string>(ExpectedServices.String.JavaScriptEndpoint));
            tagBuilder.MergeAttribute("type", "text/javascript");

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString JavaScript(this HtmlHelper helper, string scriptName)
        {
            var tagBuilder = new TagBuilder("script");

            tagBuilder.MergeAttribute("src", GetNamedService<Func<string,string>>(ExpectedServices.Func.String.ReturnsString.GetFullScriptPath)(scriptName));
            tagBuilder.MergeAttribute("type", "text/javascript");

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString ScriptResource(this UrlHelper helper, string scriptPath)
        {
            var tagBuilder = new TagBuilder("script");
            // another option: HttpContext.Current.Server.MapPath
            // consider using dependency resolver to get mapPath for testability/portability
            var scriptRelPath = scriptPath.StartsWith("~") ? helper.Content(scriptPath) : scriptPath;
            tagBuilder.MergeAttribute("src", scriptRelPath); // + (scriptPath.Contains("?") ? "&" : "?") + "v=" + VersionTag);
            tagBuilder.MergeAttribute("type", "text/javascript");

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString StyleResource(this HtmlHelper helper, string filePath, string media = "screen,projection")
        {
            var tagBuilder = new TagBuilder("link");

            tagBuilder.MergeAttribute("rel", "stylesheet");
            tagBuilder.MergeAttribute("media", media);
            tagBuilder.MergeAttribute("href", filePath + (filePath.Contains("?") ? "&" : "?") + "v=" + VersionTag);

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString Tmpl(this HtmlHelper helper, string templateId)
        {
            return MvcHtmlString.Create(GetNamedService<Func<string, string>>(ExpectedServices.Func.String.ReturnsString.GetSecuredScript)(templateId)); //TemplateStore.Current.GetSecuredScript(templateId));
        }

        public static MvcHtmlString FormLabel(
            this HtmlHelper helper, Func<string, string> translator, string propertyName, string displayName, string description, bool renderColon = true)
        {
            var labelTagBuilder = new TagBuilder("label");
            labelTagBuilder.Attributes.Add("for", propertyName);

            if (renderColon)
            {
                displayName += ":";
            }

            labelTagBuilder.SetInnerText(displayName);

            var output = labelTagBuilder.ToString(TagRenderMode.Normal);

            if (!String.IsNullOrWhiteSpace(description))
            {
                var desc = translator(description);

                var spanTagBuilder = new TagBuilder("span");

                if (desc.Length > 30)
                {
                    spanTagBuilder.Attributes.Add("class", "long-text");
                }

                spanTagBuilder.SetInnerText(desc);

                output += spanTagBuilder.ToString(TagRenderMode.Normal);
            }

            return MvcHtmlString.Create(output);
        }

        public static MvcHtmlString FormLabel(this HtmlHelper helper, string propertyName, string displayName, string description, bool renderColon = true)
        {
            var labelTagBuilder = new TagBuilder("label");
            labelTagBuilder.Attributes.Add("for", propertyName);

            if (renderColon)
            {
                displayName += ":";
            }

            labelTagBuilder.SetInnerText(displayName);

            var output = labelTagBuilder.ToString(TagRenderMode.Normal);

            if (!String.IsNullOrWhiteSpace(description))
            {
                var desc = GetNamedService<Func<string, string>>(ExpectedServices.Func.String.ReturnsString.Translate)(description); //ServiceLocator.Current.GetService(typeof(TranslatorService)) as TranslatorService;
                //var desc = translator.Translate(description);

                var spanTagBuilder = new TagBuilder("span");

                if (desc.Length > 30)
                {
                    spanTagBuilder.Attributes.Add("class", "long-text");
                }

                spanTagBuilder.SetInnerText(desc);

                output += spanTagBuilder.ToString(TagRenderMode.Normal);
            }

            return MvcHtmlString.Create(output);
        }

        public static MvcHtmlString DialogButton(
            this HtmlHelper helper, string text, string dialogTitle, string dialogController, string dialogAction, string mode = "")
        {
            var anchorTagBuilder = new TagBuilder("a");
            anchorTagBuilder.AddCssClass("t-button");
            anchorTagBuilder.Attributes.Add("href", "#");
            anchorTagBuilder.InnerHtml = text;

            if (string.IsNullOrWhiteSpace(mode))
            {
                anchorTagBuilder.Attributes.Add("onclick", "Dialog.open('" + dialogTitle + "','" + dialogController + "','" + dialogAction + "');");
            }
            else
            {
                anchorTagBuilder.Attributes.Add(
                    "onclick", "Dialog.open('" + dialogTitle + "','" + dialogController + "','" + dialogAction + "', null, '" + mode + "');");
            }

            return MvcHtmlString.Create(anchorTagBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString DialogLink(this HtmlHelper helper, string text, string dialogTitle, string dialogController, string dialogAction)
        {
            var anchorTagBuilder = new TagBuilder("a");
            anchorTagBuilder.Attributes.Add("href", "#");
            anchorTagBuilder.InnerHtml = text;
            anchorTagBuilder.Attributes.Add("onclick", "Dialog.open('" + dialogTitle + "','" + dialogController + "','" + dialogAction + "');");

            return MvcHtmlString.Create(anchorTagBuilder.ToString(TagRenderMode.Normal));
        }

        //public static MvcHtmlString AdvertisementScript(this HtmlHelper helper, AdZone adZone, string[] payerList)
        //{
        //    var tagBuilder = new TagBuilder("script");
        //    tagBuilder.Attributes.Add("type", "text/javascript");
        //    var keywords = Config.ReadAppSetting("AdServerDefaultKeyword") + "," + string.Join(",", payerList);
        //    // "default keyword" ads are available for everyone
        //    tagBuilder.Attributes.Add("src", string.Format(Config.ReadAppSetting("AdServerProxyUrl"), (int)adZone, keywords));

        //    var temp = MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
        //    return temp;
        //}

        private static Type GetNonNullableModelType(ModelMetadata modelMetadata)
        {
            var realModelType = modelMetadata.ModelType;
            var underlyingType = Nullable.GetUnderlyingType(realModelType);

            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }

            return realModelType;
        }

        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
        {
            return EnumDropDownListFor(htmlHelper, expression, null);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(
            this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var enumType = GetNonNullableModelType(metadata);
            var values = Enum.GetValues(enumType).Cast<TEnum>();

            var items = from value in values
                        select new SelectListItem { Text = GetEnumDescription(value), Value = value.ToString(), Selected = value.Equals(metadata.Model) };

            // If the enum is nullable, add an 'empty' item to the collection
            if (metadata.IsNullableValueType)
            {
                items = new[] { new SelectListItem { Text = "", Value = "" } }.Concat(items);
            }

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }
    }
}