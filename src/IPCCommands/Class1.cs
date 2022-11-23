using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCShared
{
    public enum VersionCompatibilitiy
    {
        BackwardCompatible,
        StrictVersionCompatible,
        AllVersionsCompatible
    }

    public enum VersionSelector
    {
        AllCompatibleVersions,
        LatestVersion,
        MinimumVersion
    }
}
