namespace WcfData
{
	using System.Configuration;

	public partial class MaslowJax_dbsEntities
	{
		public MaslowJax_dbsEntities(string connectionStringName)
			:base(new System.Data.EntityClient.EntityConnectionStringBuilder()
			{Provider = "System.Data.SqlClient", ProviderConnectionString =ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString,
				Metadata = "res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl"}.ToString())
		{}
	}
}