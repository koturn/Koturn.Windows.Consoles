#if !NET7_0_OR_GREATER
using System;
#endif  // !NET7_0_OR_GREATER
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
#if !NET7_0_OR_GREATER
using System.Security;
#endif  // !NET7_0_OR_GREATER


namespace Koturn.Windows.Consoles.Internals
{
    /// <summary>
    /// Exception helper.
    /// </summary>
    internal static class ThrowHelper
    {
        /// <summary>
        /// Throw <see cref="Win32Exception"/> associated with last Win32 error.
        /// </summary>
        /// <param name="message">A detailed description of the error.</param>
        /// <exception cref="Win32Exception">Always thrown.</exception>
        [DoesNotReturn]
        public static void ThrowLastWin32Exception(string message)
        {
            ThrowWin32Exception(Marshal.GetLastWin32Error(), message);
        }

        /// <summary>
        /// Throw <see cref="Win32Exception"/>.
        /// </summary>
        /// <param name="error">The Win32 error code associated with this exception.</param>
        /// <param name="message">A detailed description of the error.</param>
        /// <exception cref="Win32Exception">Always thrown.</exception>
        [DoesNotReturn]
        public static void ThrowWin32Exception(int error, string message)
        {
#if NET7_0_OR_GREATER
            throw new Win32Exception(error, $"{message}; [0x{error:X08}] {Marshal.GetPInvokeErrorMessage(error)}");
#else
            throw new Win32Exception(error, $"{message}; [0x{error:X08}] {GetPInvokeErrorMessage(error)}");
#endif  // NET7_0_OR_GREATER
        }


#if !NET7_0_OR_GREATER
        /// <summary>
        /// Returns a string message for the specified Win32 error code.
        /// </summary>
        /// <param name="error">The Win32 error code.</param>
        /// <returns>A string message for the specified Win32 error code.</returns>
        private static string GetPInvokeErrorMessage(int error)
        {
            return GetPInvokeErrorMessage(error, IntPtr.Zero);
        }
 
        /// <summary>
        /// Returns a string message for the specified Win32 error code.
        /// </summary>
        /// <param name="error">The Win32 error code associated with this exception.</param>
        /// <param name="hModule">Module handle.</param>
        /// <returns>A string message for the specified Win32 error code.</returns>
        internal static unsafe string GetPInvokeErrorMessage(int error, IntPtr hModule)
        {
            const int stackBufferSize = 256;

            var flags = FormatMessageFlags.IgnoreInserts
                | FormatMessageFlags.FromSystem
                | FormatMessageFlags.ArgumentArray;
            if (hModule != IntPtr.Zero)
            {
                flags |= FormatMessageFlags.FromHmodule;
            }
 
            // First try to format the message into the stack based buffer. Most error messages willl fit.
            char *stackBufferPtr = stackalloc char[stackBufferSize];  // arbitrary stack limit
            var length = SafeNativeMethods.FormatMessage(
                flags,
                hModule,
                unchecked((uint)error),
                0,
                stackBufferPtr,
                stackBufferSize,
                IntPtr.Zero);
            if (length > 0)
            {
                return GetAndTrimString(stackBufferPtr, length);
            }

            // We got back an error. If the error indicated that there wasn't enough room to store
            // the error message, then call FormatMessage again, but this time rather than passing in
            // a buffer, have the method allocate one, which we then need to free.
            if (Marshal.GetLastWin32Error() == (int)Win32Errors.InsufficientBuffer)
            {
                IntPtr nativeMsgPtr = default;
                try
                {
                    length = SafeNativeMethods.FormatMessage(
                        flags | FormatMessageFlags.AllocateBuffer,
                        hModule,
                        unchecked((uint)error),
                        0,
                        &nativeMsgPtr,
                        0,
                        IntPtr.Zero);
                    if (length > 0)
                    {
                        return GetAndTrimString((char *)nativeMsgPtr, length);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(nativeMsgPtr);
                }
            }
 
            // Couldn't get a message, so manufacture one.
            return $"Unknown error (0x{error:x})";
        }

        /// <summary>
        /// Get trimmed message from specified buffer pointer.
        /// </summary>
        /// <param name="bufferPtr"><see cref="char"/> buffer pointer.</param>
        /// <param name="length">Message length.</param>
        /// <returns>Trimmed message.</returns>
        private static unsafe string GetAndTrimString(char *bufferPtr, int length)
        {
            while (length > 0 && bufferPtr[length - 1] <= 32)
            {
                length--;  // trim off spaces and non-printable ASCII chars at the end of the resource
            }
            return new string(bufferPtr, 0, length);
        }


        /// <summary>
        /// Provides native methods.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            /// <summary>
            /// Formats a message string.
            /// The function requires a message definition as input.
            /// The message definition can come from a buffer passed into the function.
            /// It can come from a message table resource in an already-loaded module.
            /// Or the caller can ask the function to search the system's message table resource(s) for the message definition.
            /// The function finds the message definition in a message table resource based on a message identifier and a language identifier.
            /// The function copies the formatted message text to an output buffer, processing any embedded insert sequences if requested.
            /// </summary>
            /// <param name="flags">
            /// <para>The formatting options, and how to interpret the lpSource parameter.
            /// The low-order byte of dwFlags specifies how the function handles line breaks in the output buffer.
            /// The low-order byte can also specify the maximum width of a formatted output line.</para>
            /// <para>This parameter can be one or more of the <see cref="FormatMessageFlags"/> values.</para>
            /// <para>The low-order byte of dwFlags can specify the maximum width of a formatted output line.
            /// The following are possible values of the low-order byte.</para>
            /// <para>If the low-order byte is a nonzero value other than <see cref="FormatMessageFlags.MaxWidthMask"/>,
            /// it specifies the maximum number of characters in an output line.
            /// The function ignores regular line breaks in the message definition text.
            /// The function never splits a string delimited by white space across a line break.
            /// The function stores hard-coded line breaks in the message definition text into the output buffer.
            /// Hard-coded line breaks are coded with the %n escape sequence.</para>
            /// </param>
            /// <param name="lpSource">
            /// <para>The location of the message definition.
            /// The type of this parameter depends upon the settings in the <paramref name="flags"/> parameter.
            /// <list type="bullet">
            ///   <item><see cref="FormatMessageFlags.FromHmodule"/></item>
            ///   <item><see cref="FormatMessageFlags.FromString"/></item>
            /// </list>
            /// </para>
            /// <para>If neither of these flags is set in dwFlags, then lpSource is ignored.</para>
            /// </param>
            /// <param name="dwMessageId">
            /// The message identifier for the requested message.
            /// This parameter is ignored if dwFlags includes FORMAT_MESSAGE_FROM_STRING.
            /// </param>
            /// <param name="dwLanguageId">
            /// <para>The <see href="https://learn.microsoft.com/en-us/windows/desktop/Intl/language-identifiers">language identifier</see> for the requested message.
            /// This parameter is ignored if <paramref name="flags"/> includes <see cref="FormatMessageFlags.FromString"/>.</para>
            /// <para>If you pass a specific LANGID in this parameter, FormatMessage will return a message for that LANGID only.
            /// If the function cannot find a message for that LANGID, it sets Last-Error to ERROR_RESOURCE_LANG_NOT_FOUND.
            /// If you pass in zero, FormatMessage looks for a message for LANGIDs in the following order:
            /// <list type="number">
            ///   <item>Language neutral</item>
            ///   <item>Thread LANGID, based on the thread's locale value</item>
            ///   <item>User default LANGID, based on the user's default locale value</item>
            ///   <item>System default LANGID, based on the system default locale value</item>
            ///   <item>US English</item>
            /// </list>
            /// </para>
            /// <para>If <see cref="FormatMessage(FormatMessageFlags, IntPtr, uint, int, void*, int, IntPtr)"/> does not locate a message for any of the preceding LANGIDs,
            /// it returns any language message string that is present.
            /// If that fails, it returns ERROR_RESOURCE_LANG_NOT_FOUND.</para>
            /// </param>
            /// <param name="lpBuffer">
            /// <para>A pointer to a buffer that receives the null-terminated string that specifies the formatted message.
            /// If dwFlags includes FORMAT_MESSAGE_ALLOCATE_BUFFER, the function allocates a buffer using the LocalAlloc function,
            /// and places the pointer to the buffer at the address specified in lpBuffer.</para>
            /// <para>This buffer cannot be larger than 64K bytes.</para>
            /// </param>
            /// <param name="nSize">
            /// <para>If the <see cref="FormatMessageFlags.AllocateBuffer"/> flag is not set, this parameter specifies the size of the output buffer, in <see cref="char"/>s.
            /// If <see cref="FormatMessageFlags.AllocateBuffer"/> is set,
            /// this parameter specifies the minimum number of <see cref="char"/>s to allocate for an output buffer.</para>
            /// <para>The output buffer cannot be larger than 64K bytes.</para>
            /// </param>
            /// <param name="arguments">
            /// <para>An array of values that are used as insert values in the formatted message.
            /// A %1 in the format string indicates the first value in the Arguments array; a %2 indicates the second argument; and so on.</para>
            /// <para>The interpretation of each value depends on the formatting information associated with the insert in the message definition.
            /// The default is to treat each value as a pointer to a null-terminated string.</para>
            /// <para>By default, the Arguments parameter is of type va_list*, which is a language-
            /// and implementation-specific data type for describing a variable number of arguments.
            /// The state of the va_list argument is undefined upon return from the function.
            /// To use the va_list again, destroy the variable argument list pointer using va_end and reinitialize it with va_start.</para>
            /// <para>If you do not have a pointer of type va_list*, then specify the <see cref="FormatMessageFlags.ArgumentArray"/> flag
            /// and pass a pointer to an array of DWORD_PTR values; those values are input to the message formatted as the insert values.
            /// Each insert must have a corresponding element in the array.</para>
            /// </param>
            /// <returns>
            /// <para>If the function succeeds, the return value is the number of <see cref="char"/>s stored in the output buffer,
            /// excluding the terminating null character.</para>
            /// <para>If the function fails, the return value is zero.
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-formatmessage"/>
            /// </remarks>
            [DllImport("kernel32.dll", EntryPoint = "FormatMessageW", ExactSpelling = true, SetLastError = true)]
            public static extern unsafe int FormatMessage(FormatMessageFlags flags, IntPtr lpSource, uint dwMessageId, int dwLanguageId, void* lpBuffer, int nSize, IntPtr arguments);
        }
#endif  // NET7_0_OR_GREATER
    }
}
