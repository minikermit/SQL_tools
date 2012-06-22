using System;
using System.Data.SqlClient;
using Microsoft.Win32;

public sealed class ServersExplorer {
        private string serverName_ = "";
        private string userName_ = "";
        private string password_ = "";
        private string initialCatalog_ = "";
        private string applicationName_ = "";
        private bool integratedSecurity_ = false;
        private string connectionString_ = "";

        public string UserName {
            get { return userName_; }
            set { userName_ = value; }
        }
        
        public string Password {
            get { return password_; }
            set { password_ = value; }
        }
        
        public string InitialCatalog {
            get { return initialCatalog_; }
            set {
                if (value == null || value.Length == 0)
                    throw new ArgumentException("Invalid initial database catalog");
                initialCatalog_ = value;
            }
        }
        
        public string ApplicationName {
            get { return applicationName_; }
            set { applicationName_ = value; }
        }
        
        public bool IntegratedSecurity {
            get { return integratedSecurity_; }
            set { integratedSecurity_ = value; }
        }
        
        public string ServerName {
            get { return serverName_; }
            set {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException("Invalid argument value", "value");
                serverName_ = value;
            }
        }
        
        public string ConnectionString {
            get {
                SqlConnectionStringBuilder cnBuilder = new SqlConnectionStringBuilder();
                cnBuilder.ApplicationName = applicationName_;
                cnBuilder.DataSource = serverName_;
                cnBuilder.InitialCatalog = initialCatalog_;
                cnBuilder.IntegratedSecurity = integratedSecurity_;
                if (integratedSecurity_ == false) {
                    cnBuilder.UserID = userName_;
                    cnBuilder.Password = password_;
                }
                connectionString_ = cnBuilder.ConnectionString;
                return connectionString_;
            }
            set {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException("value");
                connectionString_ = value;
                UpdateValues();
            }
        }

        public ServersExplorer() {}
    
        public ServersExplorer(string connectionString) {
            ConnectionString = connectionString;
        }
        
        private void UpdateValues() {
            SqlConnectionStringBuilder cnBuilder = new SqlConnectionStringBuilder(connectionString_);
            userName_ = cnBuilder.UserID;
            password_ = cnBuilder.Password;
            serverName_ = cnBuilder.DataSource;
            initialCatalog_ = cnBuilder.InitialCatalog;
            integratedSecurity_ = cnBuilder.IntegratedSecurity;
        }

        public SQLServersItems GetSqlServers() {
            return new SQLServersItems(ServerType.SQLServer);
        }
    
        public System.Data.DataTable GetSmoSqlServers() {
            bool b_ex = false;
            System.Data.DataTable networkSource =  Microsoft.SqlServer.Management.Smo.
                        SmoApplication.EnumAvailableSqlServers(false);
            System.Data.DataTable localSource = networkSource.Clone();
            try {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
                if (rk!=null) {
                    System.Data.DataView dv = new System.Data.DataView(networkSource);
                    dv.RowFilter = "IsLocal=true";
                    string[] instances = (string[])rk.GetValue("InstalledInstances");
                    if (instances.Length > 0){
                        foreach (string element in instances){
                            System.Data.DataRow row = localSource.NewRow();
                            if (element == "MSSQLSERVER")
                                row["Name"] = Environment.MachineName;
                            else
                                row["Name"] = Environment.MachineName + "\\" + element;
                            row["IsLocal"] = true;
                            bool isAdded = false;
                            for (int i = 0; i < dv.Count;i++) {
                                System.Data.DataRowView drv = dv[i];
                                if (row["Name"].ToString() == drv["Name"].ToString()) {
                                    isAdded = true;
                                    break;
                                }
                            }
                            if (!isAdded) localSource.Rows.Add(row);
                        }
                    } 
                    else {
                        b_ex = true;
                    }
                }
                else {
                    b_ex = true;
                }                
            }
            catch {
                b_ex = true;
            }
            if (!b_ex) {
                networkSource.Merge(localSource);
            }
            return networkSource;
        }
        
        public bool TestConnection() {
            return TestConnection(initialCatalog_);
        }
    
        public bool TestConnection(string testDatabase) {
            initialCatalog_ = testDatabase;
            ConnectionType cnType = integratedSecurity_ ? ConnectionType.SSPI : ConnectionType.SQL;
            TestConnection tst = new TestConnection(cnType, serverName_);
            bool res;
            try{
                if (cnType == ConnectionType.SQL){
                    tst.Password = password_;
                    tst.UserId = userName_;
                }
                tst.InitialCatalog = testDatabase;
                res = tst.Test();
            }
            catch (Exception){
                res = false;
            }
            return res;
        }
    
    }
