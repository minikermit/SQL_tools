using System;
using System.Data.SqlClient;

    /// <summary>
    /// Auth type for checking SQL Server connection
    /// </summary>
    public enum ConnectionType : byte {
        SSPI = 0,
        SQL = 1
    }

    public sealed class TestConnection {
        #region Instance variables
        private ConnectionType cnType;
        private string sUserId;
        private string sPassword;
        private string sServerName;
        private String sInitialCatalog;

        private static SqlConnection lastTrustedCn;
        private static string cnstr;
        #endregion

        public TestConnection(ConnectionType type) {
            cnType = type;
        }

        public TestConnection(ConnectionType type, string serverName) : this (type) {
            if (serverName.Length == 0)
                throw new ArgumentNullException("serverName");
            sServerName = serverName;
        }

        #region Properties
        public ConnectionType Type {
            get { return cnType; }
            set { cnType = value; }
        }

        public string Server {
            get { return sServerName; }
            set { sServerName = value; }
        }

        public string UserId {
            get {
                if (cnType == ConnectionType.SSPI)
                    throw new NotSupportedException();
                return sUserId;
            }
            set {
                if (cnType == ConnectionType.SSPI)
                    throw new NotSupportedException();
                sUserId = value;
            }
        }

        public string Password {
            get {
                if (cnType == ConnectionType.SSPI)
                    throw new NotSupportedException();
                return sPassword;
            }
            set {
                if (cnType == ConnectionType.SSPI)
                    throw new NotSupportedException();
                sPassword = value;
            }
        }
        
        public string InitialCatalog {
            get { return sInitialCatalog; }
            set { sInitialCatalog = value; }
        }
        #endregion

        /// <summary>
        /// Test to connect 
        /// </summary>
        /// <returns>
        /// True if connection is opened successfuly
        /// </returns>
        public bool Test() {
            bool res = false;
            SqlConnection cn = null;
            try {
                // Configure connection string using SqlConnectionStringBuilder
                SqlConnectionStringBuilder cnBuilder = new SqlConnectionStringBuilder();
                cnBuilder.DataSource = sServerName;
                cnBuilder.Pooling = false;
                cnBuilder.InitialCatalog = sInitialCatalog;
                if (cnType == ConnectionType.SQL) {
                    cnBuilder.IntegratedSecurity = false;
                    cnBuilder.UserID = sUserId;
                    cnBuilder.Password = sPassword;
                } else
                    cnBuilder.IntegratedSecurity = true;
                cn = new SqlConnection(cnBuilder.ToString());
                cn.Open();
                lastTrustedCn = cn;
                cnstr = cnBuilder.ToString();
                res = true ;
            } catch (Exception ex) {
                res = false;
                throw new ArgumentException(ex.Message);
            } finally {
                if (cn != null)
                    cn.Close();
            }
            return res;
        }

        /// <summary>
        /// Return latest successfuly open connection
        /// </summary>
        public static SqlConnection LastTrustedConnection {
            get {
                if (lastTrustedCn == null) {
                    throw new ArgumentNullException();
                }
                lastTrustedCn.ConnectionString = cnstr;
                return lastTrustedCn;
            }
        }

    }

