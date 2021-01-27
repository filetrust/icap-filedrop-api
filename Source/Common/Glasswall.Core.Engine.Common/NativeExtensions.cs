using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Glasswall.Core.Engine.Common
{
    public static class NativeExtensions
    {
        public static unsafe string MarshalNativeToManaged(this IntPtr nativeString)
        {
            if (nativeString == IntPtr.Zero)
                return (string) null;
            string stringUni;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                uint* numPtr = (uint*) (void*) nativeString;
                int num = 0;
                while (*numPtr != 0U)
                {
                    ++numPtr;
                    ++num;
                }

                byte[] numArray = new byte[num * 4];
                Marshal.Copy(nativeString, numArray, 0, numArray.Length);
                stringUni = Encoding.UTF32.GetString(numArray);
            }
            else
                stringUni = Marshal.PtrToStringUni(nativeString);

            return stringUni;
        }

        public static IntPtr MarshalManagedToNative(this string managedString)
        {
            if (managedString == null)
                return IntPtr.Zero;
            IntPtr num;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                byte[] bytes = Encoding.UTF32.GetBytes(managedString);
                num = Marshal.AllocHGlobal(bytes.Length + 4);
                Marshal.Copy(bytes, 0, num, bytes.Length);
                Marshal.WriteInt32(num, bytes.Length, 0);
            }
            else
                num = Marshal.StringToHGlobalUni(managedString);

            return num;
        }

        public static void CleanUpNativeData(this IntPtr nativeString)
        {
            if (!(nativeString != IntPtr.Zero))
                return;
            Marshal.FreeHGlobal(nativeString);
        }
    }
}
