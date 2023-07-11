using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.EntityClient;
using System.Data.SqlClient;

namespace dal
{
    public class repository
    {

        #region "Private Fields"
        rehabilitation_management_dbEntities db;
        #endregion "Private Fields"
        
        #region "Constructor"
        public repository()
        {

            //Should be called by login service only
        }
        public repository(string connection)
        {
            db = new rehabilitation_management_dbEntities(connection);
        }
        #endregion "Constructor"

        #region "Public Methods"
        #region "Database and Connection"
        public bool Connect(
         string providerName,
         string serverName,
         string databaseName,
         string attacheddb,
         string userName,
         string password,
     string metaData,
          bool IntegratedSecurity)
        {
            try
            {
                EntityConnection conn = new EntityConnection(GetConnectionString(
                    providerName,
                    serverName,
                    databaseName,
                    attacheddb,
                    userName,
                    password,
                    metaData,
                    IntegratedSecurity));

                //overwrite the default context with this one
                db = new rehabilitation_management_dbEntities(conn);

                return true;
            }
            catch (Exception ex)
            {
                ///TODO Log error
                Log.WriteToErrorLogFile(ex);
                db = null;
                return false;
            }
        }
        public bool Connect(string connectiostr)
        {
            try
            {
                //overwrite the default context with this one
                //string encConnection = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

                db = new rehabilitation_management_dbEntities(connectiostr);

                return true;
            }
            catch (Exception ex)
            {
                ///TODO Log error
                Log.WriteToErrorLogFile(ex);
                db = null;
                return false;
            }
        }
        public string GetConnectionString(
           string providerName,
           string serverName,
           string databaseName,
           string attacheddb,
           string userName,
           string password,
            string metaData,
            bool IntegratedSecurity)
        {
            // Initialize the connection string builder for the
            // underlying provider.
            SqlConnectionStringBuilder sqlBuilder =
                new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = serverName;
            sqlBuilder.InitialCatalog = databaseName;
            if (!string.IsNullOrEmpty(attacheddb)) sqlBuilder.AttachDBFilename = attacheddb;
            sqlBuilder.IntegratedSecurity = IntegratedSecurity;
            sqlBuilder.UserID = userName;
            sqlBuilder.Password = password;


            // Build the SqlConnection connection string.
            string providerString = sqlBuilder.ToString();

            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = providerName;

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = providerString;

            // Set the Metadata location.
            //entityBuilder.Metadata = string.Format(@"metadata=res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl",
            //    metaData);
            entityBuilder.Metadata = string.Format(@"res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl",
                metaData);
            return entityBuilder.ToString();
        }
         #endregion "Database and Connection"
        #endregion "Public Methods"
         



    }
}
