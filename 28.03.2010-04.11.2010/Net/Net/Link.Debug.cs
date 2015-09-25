using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Net
{
	public partial class Link
	{
		[DebuggerDisplay(null, Name = "Source")]
		private Link я_A { get { return this.Source; } set { this.Source = value; } }

		[DebuggerDisplay(null, Name = "Linker")]
		private Link я_B { get { return this.Linker; } set { this.Linker = value; } }

		[DebuggerDisplay(null, Name = "Target")]
		private Link я_C { get { return this.Target; } set { this.Target = value; } }

		[DebuggerDisplay("Count = {я_DC}", Name = "ReferersBySource")]
		private List<Link> я_D { get { return this.ReferersBySource.ToList(); } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int я_DC { get { return this.ReferersBySource.Count(); } }

		[DebuggerDisplay("Count = {я_EC}", Name = "ReferersByLinker")]
		private List<Link> я_E { get { return this.ReferersByLinker.ToList(); } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int я_EC { get { return this.ReferersByLinker.Count(); } }

		[DebuggerDisplay("Count = {я_FC}", Name = "ReferersByTarget")]
		private List<Link> я_F { get { return this.ReferersByTarget.ToList(); } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int я_FC { get { return this.ReferersByTarget.Count(); } }
	}
}
