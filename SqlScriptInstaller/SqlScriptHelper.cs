using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SqlScriptInstaller
{
    public sealed class SqlScriptHelper{
        
        #region constatnts
        private const string REGEX_GO_PATTERN = @"\b(g|G)(o|O)\b";
        private const string REGEX_NON_WHITE_PATTERN  = @"\S+";
        #endregion

        private SqlScriptHelper(){}
        
        #region Static
        
        private static void InternalExecuteScript(SqlCommand cmd , string script) {
            if (cmd == null || cmd.Connection == null) 
                throw new ArgumentException("Invalide argument", "cmd");
            if (script == null || script.Length==0)
                throw new ArgumentException("Invalide argument", "script");
            Regex reg = new Regex(REGEX_GO_PATTERN, RegexOptions.ExplicitCapture);
            string[] commands = reg.Split(script);
            reg = new Regex(REGEX_NON_WHITE_PATTERN, RegexOptions.ExplicitCapture);
            bool isNeedCloseConnection = false;
            if (cmd.Connection.State == ConnectionState.Closed) {
                cmd.Connection.Open();
                isNeedCloseConnection = true;
            }
            cmd.Connection.InfoMessage += new SqlInfoMessageEventHandler(Connection_InfoMessage);
            try {
                 foreach(string cmdText in commands){
                    if (reg.IsMatch(cmdText)) {
                        cmd.CommandText = cmdText;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex) {
                throw ex;
            }
            finally {
                if (isNeedCloseConnection)
                    if (cmd.Connection != null) cmd.Connection.Close();
            }
        }
        
        private static void Connection_InfoMessage(object source, SqlInfoMessageEventArgs e) {
            string s = e.Message;
        }
        #endregion
        
        public static void ExecuteScript(SqlTransaction transaction , string script) {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = transaction.Connection;
            cmd.Transaction = transaction;
            InternalExecuteScript(cmd, script);
        }
        
        public static void ExecuteScript(SqlConnection cn, string script) {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 600;
            cmd.Connection = cn;
            InternalExecuteScript(cmd, script);
        }
        
        public static void ExecuteResourceScript(SqlTransaction transaction, string resourceName) {
            ExecuteScript(transaction, GetScript(resourceName));
        }

        public static void ExecuteResourceScript(SqlConnection cn, string resourceName) {
            ExecuteScript(cn, GetScript(resourceName));
        }

        public static string GetScript(string resourceName) {
            Assembly asmb = Assembly.GetExecutingAssembly();
            Stream sr = asmb.GetManifestResourceStream(asmb.GetName().Name + "." + resourceName);
            using (StreamReader srReader = new StreamReader(sr)){
                return srReader.ReadToEnd();
            }
        }

    }
}
