using System;
using System.Runtime.InteropServices;


namespace Koturn.Windows.Consoles.Internals
{
    /// <summary>
    /// Flag values for first argument of FormatMessage.
    /// </summary>
    [Flags]
    internal enum FormatMessageFlags
    {
        /// <summary>
        /// There are no output line width restrictions.
        /// The function stores line breaks that are in the message definition text into the output buffer.
        /// </summary>
        NoRestrict = 0x00000000,
        /// <summary>
        /// The function ignores regular line breaks in the message definition text.
        /// The function stores hard-coded line breaks in the message definition text into the output buffer.
        /// The function generates no new line breaks.
        /// </summary>
        MaxWidthMask = 0x000000ff,
        /// <summary>
        /// <para>The function allocates a buffer large enough to hold the formatted message, and places a pointer to the allocated buffer at the address specified by lpBuffer.
        /// The lpBuffer parameter is a pointer to an LPTSTR; you must cast the pointer to an LPTSTR (for example, (LPTSTR)&amp;lpBuffer).
        /// The nSize parameter specifies the minimum number of TCHARs to allocate for an output message buffer.
        /// The caller should use the LocalFree function to free the buffer when it is no longer needed.</para>
        /// <para>If the length of the formatted message exceeds 128K bytes, then FormatMessage will fail
        /// and a subsequent call to <see cref="Marshal.GetLastWin32Error"/> will return ERROR_MORE_DATA.</para>
        /// <para>In previous versions of Windows, this value was not available for use when compiling Windows Store apps.
        /// As of Windows 10 this value can be used.</para>
        /// <para>Windows Server 2003 and Windows XP:</para>
        /// <para>If the length of the formatted message exceeds 128K bytes, then FormatMessage will not automatically fail with an error of ERROR_MORE_DATA.</para>
        /// </summary>
        AllocateBuffer = 0x00000100,
        /// <summary>
        /// <para>The Arguments parameter is not a va_list structure, but is a pointer to an array of values that represent the arguments.</para>
        /// <para>This flag cannot be used with 64-bit integer values. If you are using a 64-bit integer, you must use the va_list structure.</para>
        /// </summary>
        ArgumentArray = 0x00002000,
        /// <summary>
        /// <para>The lpSource parameter is a module handle containing the message-table resource(s) to search.
        /// If this lpSource handle is NULL, the current process's application image file will be searched.
        /// This flag cannot be used with FORMAT_MESSAGE_FROM_STRING.</para>
        /// <para>If the module has no message table resource, the function fails with ERROR_RESOURCE_TYPE_NOT_FOUND.</para>
        /// <para></para>
        /// </summary>
        FromHmodule = 0x00000800,
        /// <summary>
        /// <para>The lpSource parameter is a pointer to a null-terminated string that contains a message definition.
        /// The message definition may contain insert sequences, just as the message text in a message table resource may.
        /// This flag cannot be used with FORMAT_MESSAGE_FROM_HMODULE or FORMAT_MESSAGE_FROM_SYSTEM.</para>
        /// </summary>
        FromString = 0x00000400,
        /// <summary>
        /// <para>The function should search the system message-table resource(s) for the requested message.
        /// If this flag is specified with FORMAT_MESSAGE_FROM_HMODULE, the function searches the system message table if the message is not found in the module specified by lpSource.
        /// This flag cannot be used with FORMAT_MESSAGE_FROM_STRING.</para>
        /// <para>If this flag is specified, an application can pass the result of the GetLastError function to retrieve the message text for a system-defined error.</para>
        /// </summary>
        FromSystem = 0x00001000,
        /// <summary>
        /// Insert sequences in the message definition such as %1 are to be ignored and passed through to the output buffer unchanged.
        /// This flag is useful for fetching a message for later formatting.
        /// If this flag is set, the Arguments parameter is ignored.
        /// </summary>
        IgnoreInserts = 0x00000200
    }
}
