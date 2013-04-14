namespace Webby
{
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Web;
	using System.Web.Hosting;

	public class WcfVirtualFile : VirtualFile
	{
		private readonly string _service;

		private readonly string _factory;

		public WcfVirtualFile(string vp, string service, string factory)
			: base(vp)
		{
			Debug.WriteLine("vp: {0}, service: {1}, factory: {2}", vp, service, factory);
			this._service = service;
			this._factory = factory;
		}

		public override Stream Open()
		{
			var ms = new MemoryStream();
			var tw = new StreamWriter(ms);

			tw.Write(
				string.Format(
					CultureInfo.InvariantCulture,
					"<%@ServiceHost Language=\"C#\" Debug=\"true\" Service=\"{0}\" Factory=\"{1}\"%>",
					HttpUtility.HtmlEncode(this._service),
					HttpUtility.HtmlEncode(this._factory)));

			tw.Flush();

			ms.Position = 0;

			return ms;
		}
	}
}