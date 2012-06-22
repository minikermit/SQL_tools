using System;
using System.Runtime.InteropServices;

    public sealed class NetBiosAPI {
        [DllImport("netapi32.dll")]
        public static extern void NetApiBufferFree(IntPtr bufptr);

        [DllImport("netapi32.dll")]
        public static extern UInt32 NetServerEnum(
            IntPtr ServerName,
            UInt32 level, 
            ref IntPtr siPtr, 
            UInt32 prefmaxlen, 
            ref UInt32 entriesread,
            ref UInt32 totalentries, 
            UInt32 servertype32, 
            String domain, 
            IntPtr resumeHandle);

        [DllImport("netapi32.dll")]
        public static extern UInt32 NetServerGetInfo(String ServerName, int level,ref IntPtr buffPtr);

        /// <summary>
        /// The SERVER_INFO_101 structure contains information about the specified server, including name, platform, type of server, and associated software.
        /// </summary>
        /// <remarks>http://msdn.microsoft.com/library/default.asp?url=/library/en-us/netmgmt/netmgmt/server_info_101_str.asp</remarks>
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SERVER_INFO_101 {
            public Int32 dwPlatformID;
            public IntPtr lpszServerName;
            public Int32 dwVersionMajor;
            public Int32 dwVersionMinor;
            public Int32 dwType;
            public IntPtr lpszComment;
        }
    }

    [Flags()]
    public enum ServerType : uint {
        AFP = 64,
        All = 4294967295,
        AlternateXPort = 536870912,
        BackupBrowser = 131072,
        ClusterNT = 16777216,
        DCE = 268435456,
        DFS = 8388608,
        Dialin = 1024,
        DomainBackupController = 16,
        DomainController = 8,
        DomainEnum = 2147483648,
        DomainMaster = 524288,
        DomainMember = 256,
        ListOnly = 1073741824,
        MasterBrowser = 262144,
        MFPN = 16384,
        None = 0,
        Novell = 128,
        NT = 4096,
        NTServer = 32768,
        OSF = 1048576,
        PotentialBrowser = 65536,
        PrintQueue = 512,
        Server = 2,
        SQLServer = 4,
        TerminalServer = 33554432,
        TimeSource = 32,
        Unix = 2048,
        VMS = 2097152,
        WFW = 8192,
        Windows = 4194304,
        Workstation = 1,
        Xenix = 2048
    }

