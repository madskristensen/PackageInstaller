namespace PackageInstaller
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class GuidList
    {
        public const string guidVSPackageString = "f91d6656-dccf-400f-843d-1ff49242cf4b";
        public const string guidVSPackageCmdSetString = "c0f38f12-aa45-4a08-9305-30003a67fecc";
        public static Guid guidVSPackage = new Guid(guidVSPackageString);
        public static Guid guidVSPackageCmdSet = new Guid(guidVSPackageCmdSetString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageCommands
    {
        public const int MyMenuGroup = 0x1020;
        public const int InstallPackageId = 0x0100;
    }
}
