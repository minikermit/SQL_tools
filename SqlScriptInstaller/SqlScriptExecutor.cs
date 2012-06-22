using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Xml;

namespace SqlScriptInstaller
{
    public sealed class SqlScriptExecutor{
        
        #region constants
        private const string readerName = "script";
        private const string nameAttribute = "path";
        private const string encoding = "windows-1251";
        #endregion
        
        #region Internal config parser
        
        private sealed class XmlReader{
            internal static string[] ReaderParser(string fileName, string readerName, string nameAttribyte) {
                ArrayList arr = new ArrayList();
                string filePath = AppDomain.CurrentDomain.BaseDirectory + fileName;
                using(FileStream fs = new FileStream(filePath, FileMode.Open)) {
                    XmlTextReader reader = new XmlTextReader(fs);
                    while (reader.Read()) 
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == readerName)
                            arr.Add(AppDomain.CurrentDomain.BaseDirectory +
                                    reader.GetAttribute(nameAttribute).ToString());
                }
                string[] sarr = new string[arr.Count];
                arr.CopyTo(0, sarr, 0, arr.Count);
                return sarr;
            }
        }
        #endregion
        
        private static void ExecuteToString(string script, SqlConnection cn) {
            SqlScriptHelper.ExecuteScript(cn, script);
        }

        public static void ExecuteToFile(string fileName, SqlTransaction tran){
            StringBuilder sb = new StringBuilder();
            sb.Append((new StreamReader(fileName, Encoding.GetEncoding(encoding)).ReadToEnd()).ToString());
            SqlScriptHelper.ExecuteScript(tran, sb.ToString());
        }

        public static void ExecuteToXml(string xmlFile, SqlTransaction tran){
            foreach (string path in XmlReader.ReaderParser(xmlFile, readerName, nameAttribute))
                ExecuteToFile(path, tran);
        }
        
        public static void ExecuteToFile(string fileName, SqlConnection cn) {
            StringBuilder sb = new StringBuilder();
            sb.Append((new StreamReader(fileName, Encoding.GetEncoding(encoding)).ReadToEnd()).ToString());
            SqlScriptHelper.ExecuteScript(cn, sb.ToString());
        }
        
        public static void ExecuteToXml(string xmlFile, SqlConnection cn) {
            foreach(string path in XmlReader.ReaderParser(xmlFile, readerName, nameAttribute)) {
                ExecuteToFile(path, cn);
            }
        }
    }
}
