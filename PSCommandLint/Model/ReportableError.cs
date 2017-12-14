using System;
using System.Management.Automation.Language;

namespace PSCommandLint.Model
{
    /// <summary>
    /// An error about the script that we will report to the user.
    /// </summary>
    public class ReportableError : ParseError
    {
        public ReportableError(IScriptExtent extent, string message)
            : base(extent, "", message)
        {
            if (extent == null) throw new ArgumentNullException(nameof(extent));
            if (message == null) throw new ArgumentNullException(nameof(message));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ReportableError);
        }

        private bool Equals(ReportableError obj)
        {
            if (obj == null) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return this.ErrorId == obj.ErrorId
                   && this.Message == obj.Message
                   && this.Extent.Equals(obj.Extent);
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + Extent.GetHashCode();
            hash = (hash * 7) + Message.GetHashCode();
            return hash;
        }
    }
}
