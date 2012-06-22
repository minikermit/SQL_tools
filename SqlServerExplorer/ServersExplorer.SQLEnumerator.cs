using System;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;

    /// <summary>
    /// Класс, реализующий IEnumerator, возвращающий имена серверов в сети.
    /// </summary>
    public class SQLEnumerator : IEnumerator {
        #region Instance variables
        private IntPtr serverInfoPtr;
        private Int32 currentItem;
        private UInt32 itemCount;
        private string currentServerName;
        private static Int32 SERVER_INFO_101_SIZE;
        private bool disposed = false;
        #endregion

        static SQLEnumerator() {
            // Получаем размер структуры NetBiosAPI.SERVER_INFO_101
            SQLEnumerator.SERVER_INFO_101_SIZE = Marshal.SizeOf(typeof(NetBiosAPI.SERVER_INFO_101));
        }

        public SQLEnumerator(ServerType serverType, string domainName) {
            UInt32 level = 101;
            UInt32 prefMaxLen = UInt32.MaxValue;
            UInt32 entriesReadead = 0;
            UInt32 totalEntries = 0;
            this.Reset();
            this.serverInfoPtr = IntPtr.Zero;
            UInt32 res = NetBiosAPI.NetServerEnum(IntPtr.Zero, level, ref serverInfoPtr,
                                                  prefMaxLen, ref entriesReadead, ref totalEntries,
                                                  (UInt32)serverType, null, IntPtr.Zero);
            itemCount = entriesReadead;
        }

        public SQLEnumerator(ServerType serverType) : this(serverType, null) { }

        #region IEnumerator functions
        public object Current {
            get { return currentServerName; }
        }

        public bool MoveNext() {
            bool res = false;
            currentItem = currentItem + 1;
            if (currentItem < itemCount) {
                int num = serverInfoPtr.ToInt32() + (SERVER_INFO_101_SIZE * currentItem);
                NetBiosAPI.SERVER_INFO_101 si =
                    (NetBiosAPI.SERVER_INFO_101)
                    Marshal.PtrToStructure(new IntPtr(num), typeof(NetBiosAPI.SERVER_INFO_101));
                currentServerName = Marshal.PtrToStringAuto(si.lpszServerName);
                res = true;
            }
            return res;
        }

        public void Reset() {
            IPHostEntry host = Dns.GetHostEntry("");
            currentItem = -1;
            currentServerName = null;
        }
        #endregion

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    // TODO: put code to dispose managed resources
                }
                // TODO: put code to free unmanaged resources here
            }
            this.disposed = true;
        }

        #region IDisposable Support
        public virtual void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

    public class SQLServersItems : IEnumerable, IDisposable {
        private ServerType serverType_;
        private string domainName_;
        private bool disposed = false;

        #region Properties
        public ServerType Type {
            get { return serverType_; }
            set { serverType_ = value; }
        }

        public string Domain {
            get { return domainName_; }
            set { domainName_ = value; }
        }
        #endregion

        public SQLServersItems() {
            serverType_ = ServerType.None;
        }

        public SQLServersItems(ServerType type) {
            serverType_ = type;
        }

        #region IEnumerator functions
        public IEnumerator GetEnumerator() {
            return new SQLEnumerator(serverType_, domainName_);
        }
        #endregion

        public static ServerType GetServerType(string serverName) {
            ServerType res = ServerType.None;
            IntPtr ptr = IntPtr.Zero;
            UInt32 num = NetBiosAPI.NetServerGetInfo(serverName, 101, ref ptr); // ptr
            if (num != 0) {
                NetBiosAPI.SERVER_INFO_101 server_info_1 =
                    (NetBiosAPI.SERVER_INFO_101)Marshal.PtrToStructure(ptr, typeof(NetBiosAPI.SERVER_INFO_101));
                res = (ServerType)server_info_1.dwType;
                NetBiosAPI.NetApiBufferFree(ptr);
                ptr = IntPtr.Zero;
            }
            return res;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    // TODO: put code to dispose managed resources
                }
                // TODO: put code to free unmanaged resources here
            }
            disposed = true;
        }

        #region IDisposable Support
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

