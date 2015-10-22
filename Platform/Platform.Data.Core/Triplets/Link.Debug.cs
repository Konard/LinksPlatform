using System;
using System.Diagnostics;

namespace Platform.Links.DataBase.CoreNet.Triplets
{
    public partial struct Link
    {
        #region Properties

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local

        [DebuggerDisplay(null, Name = "Source")]
        private Link я_A { get { return this == null ? Itself : Source; } }

        [DebuggerDisplay(null, Name = "Linker")]
        private Link я_B { get { return this == null ? Itself : Linker; } }

        [DebuggerDisplay(null, Name = "Target")]
        private Link я_C { get { return this == null ? Itself : Target; } }

        [DebuggerDisplay("Count = {я_DC}", Name = "ReferersBySource")]
        private Link[] я_D { get { return this.GetArrayOfRererersBySource(); } }

        [DebuggerDisplay("Count = {я_EC}", Name = "ReferersByLinker")]
        private Link[] я_E { get { return this.GetArrayOfRererersByLinker(); } }

        [DebuggerDisplay("Count = {я_FC}", Name = "ReferersByTarget")]
        private Link[] я_F { get { return this.GetArrayOfRererersByTarget(); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Int64 я_DC { get { return this == null ? 0 : ReferersBySourceCount; } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Int64 я_EC { get { return this == null ? 0 : ReferersByLinkerCount; } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Int64 я_FC { get { return this == null ? 0 : ReferersByTargetCount; } }

        [DebuggerDisplay(null, Name = "Timestamp")]
        private DateTime я_H { get { return this == null ? DateTime.MinValue : Timestamp; } }

        // ReSharper restore UnusedMember.Local
        // ReSharper restore InconsistentNaming

        #endregion

        public override string ToString()
        {
            const string nullString = "null";
            if (this == null)
                return nullString;
            else
            {
                string name;
                if (this.TryGetName(out name))
                    return name;
                else
                    return ((long)_link).ToString();
            }
        }
    }
}
