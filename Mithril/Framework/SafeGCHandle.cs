using System;
using System.Runtime.InteropServices;

namespace Mithril
{
    public sealed class SafeGCHandle : SafeHandle
    {
        public SafeGCHandle(Object target, GCHandleType type)
            : base(IntPtr.Zero, true)
        {
            SetHandle(GCHandle.ToIntPtr(GCHandle.Alloc(target, type)));
        }

        public GCHandle Handle
        {
            get { return GCHandle.FromIntPtr(handle); }
        }

        public override Boolean IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override Boolean ReleaseHandle()
        {
            try
            {
                GCHandle.FromIntPtr(handle).Free();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IntPtr AddrOfPinnedObject()
        {
            return Handle.AddrOfPinnedObject();
        }
    }
}