#if NET7_0_OR_GREATER
#    define SUPPORT_LIBRARY_IMPORT
#endif  // NET7_0_OR_GREATER

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Koturn.Windows.Consoles.Internals;


namespace Koturn.Windows.Consoles
{
    /// <summary>
    /// Provides P/Invoke wrapper methods about console.
    /// </summary>
#if NET7_0_OR_GREATER
    public static partial class ConsoleUtil
#else
    public static class ConsoleUtil
#endif  // NET7_0_OR_GREATER
    {
        /// <summary>
        /// Special value for the first argument of <see cref="AttachConsole(uint, bool)"/>.
        /// </summary>
        public const uint AttachParentProcess = uint.MaxValue;


        /// <summary>
        /// Ensure console; attach parent console or allocate console.
        /// </summary>
        /// <param name="autoFlush">A flag whether auto flushing buffer stdout or stderr.</param>
        /// <returns>false if a console already attached or allocated, otherwise true.</returns>
        public static bool EnsureConsole(bool autoFlush = true)
        {
            if (!SafeNativeMethods.AttachConsole(AttachParentProcess)
                && !SafeNativeMethods.AllocConsole())
            {
                // Maybe already a console attached or allocated.
                return false;
            }

            SetupConsole(autoFlush);

            return true;
        }

        /// <summary>
        /// Attaches the calling process to the console of the specified process as a client application.
        /// </summary>
        /// <param name="processId">The identifier of the process whose console is to be used.</param>
        /// <param name="autoFlush">A flag whether auto flushing buffer stdout or stderr.</param>
        /// <exception cref="Win32Exception">Thrown when AttachConsole, Win32 API function failed.</exception>
        public static void AttachConsole(uint processId = AttachParentProcess, bool autoFlush = true)
        {
            if (!SafeNativeMethods.AttachConsole(processId))
            {
                ThrowHelper.ThrowLastWin32Exception(nameof(SafeNativeMethods.AttachConsole) + " failed");
            }
            SetupConsole(autoFlush);
        }

        /// <summary>
        /// Allocates a new console for the calling process and setup console.
        /// </summary>
        /// <param name="autoFlush">A flag whether auto flushing buffer stdout or stderr.</param>
        /// <exception cref="Win32Exception">Thrown when AllocConsole, Win32 API function failed.</exception>
        public static void AllocConsole(bool autoFlush = true)
        {
            if (!SafeNativeMethods.AllocConsole())
            {
                ThrowHelper.ThrowLastWin32Exception(nameof(SafeNativeMethods.AllocConsole) + " failed");
            }
            SetupConsole(autoFlush);
        }

        /// <summary>
        /// Get console window handle.
        /// </summary>
        /// <returns>Console window handle.</returns>
        /// <exception cref="Win32Exception">Thrown when GetConsoleWindow, Win32 API function failed.</exception>
        public static IntPtr GetConsoleWindow()
        {
            var hWnd = SafeNativeMethods.GetConsoleWindow();
            if (hWnd == IntPtr.Zero)
            {
                ThrowHelper.ThrowLastWin32Exception(nameof(SafeNativeMethods.GetConsoleWindow) + " failed");
            }
            return hWnd;
        }

        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        /// <exception cref="Win32Exception">Thrown when FreeConsole, Win32 API function failed.</exception>
        public static void FreeConsole()
        {
            if (!SafeNativeMethods.FreeConsole())
            {
                ThrowHelper.ThrowLastWin32Exception(nameof(SafeNativeMethods.FreeConsole) + " failed");
            }
        }

        /// <summary>
        /// Disable close buttons on console window.
        /// </summary>
        /// <exception cref="Win32Exception">Thrown when GetSystemMenu or RemoveMenu, Win32 API functions failed.</exception>
        public static void DisableCloseButton()
        {
            var hWnd = GetConsoleWindow();
            var hMenu = WindowUtil.GetSystemMenu(hWnd, false);
            WindowUtil.RemoveMenu(hMenu, SysCommand.Close);
        }

        /// <summary>
        /// Enable Ctrl-C and Ctrl-Break on console window.
        /// </summary>
        public static void EnableExitKeys()
        {
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress -= Console_CancelKeyPress;
        }

        /// <summary>
        /// Disable Ctrl-C and Ctrl-Break on console window.
        /// </summary>
        public static void DisableExitKeys()
        {
            Console.TreatControlCAsInput = true;
            Console.CancelKeyPress -= Console_CancelKeyPress;  // Prevent duplicate registering.
            Console.CancelKeyPress += Console_CancelKeyPress;
        }

        /// <summary>
        /// Show associated console window.
        /// </summary>
        public static void ShowConsole()
        {
            ShowConsole(CmdShow.Show);
        }

        /// <summary>
        /// Hide associated console window.
        /// </summary>
        public static void HideConsole()
        {
            ShowConsole(CmdShow.Hide);
        }


        /// <summary>
        /// Bind stdin, stdout and stderr to console.
        /// </summary>
        /// <param name="autoFlush">A flag whether auto flushing buffer stdout or stderr.</param>
        private static void SetupConsole(bool autoFlush = true)
        {
            if (!Console.IsInputRedirected)
            {
                Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            }
            if (!Console.IsOutputRedirected)
            {
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput())
                {
                    AutoFlush = autoFlush
                });
            }
            if (!Console.IsErrorRedirected)
            {
                Console.SetError(new StreamWriter(Console.OpenStandardError())
                {
                    AutoFlush = autoFlush
                });
            }
        }

        /// <summary>
        /// Sets the console window's show state.
        /// </summary>
        /// <param name="cmdShow">Controls how the console window is to be shown.</param>
        private static void ShowConsole(CmdShow cmdShow)
        {
            WindowUtil.ShowWindow(GetConsoleWindow(), cmdShow);
        }

        /// <summary>
        /// <para>Callback method for <see cref="Console.CancelKeyPress"/>.</para>
        /// <para>Ignore <see cref="ConsoleSpecialKey.ControlBreak"/>.</para>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="ConsoleCancelEventArgs"/> object that contains the event data.</param>
        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                e.Cancel = true;
            }
        }


        /// <summary>
        /// Provides some native methods.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
#if NET7_0_OR_GREATER
        internal static partial class SafeNativeMethods
#else
        internal static class SafeNativeMethods
#endif  // NET7_0_OR_GREATER
        {
            /// <summary>
            /// Attaches the calling process to the console of the specified process as a client application.
            /// </summary>
            /// <param name="processId">The identifier of the process whose console is to be used. This parameter can be one of the following values.
            /// <list type="table">
            ///   <listheader>
            ///     <term>pid</term>
            ///     <description>Use the console of the specified process.</description>
            ///   </listheader>
            ///   <item>
            ///     <term><see cref="AttachParentProcess"/></term>
            ///     <description>Use the console of the parent of the current process.</description>
            ///   </item>
            /// </list>
            /// </param>
            /// <returns>
            /// <para>If the function succeeds, the return value is true.</para>
            /// <para>If the function fails, the return value is false.
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <para><see href="https://learn.microsoft.com/en-us/windows/console/attachconsole"/></para>
            /// <para>A process can be attached to at most one console.
            /// If the calling process is already attached to a console, the error code returned is ERROR_ACCESS_DENIED.
            /// If the specified process does not have a console, the error code returned is ERROR_INVALID_HANDLE.
            /// If the specified process does not exist, the error code returned is ERROR_INVALID_PARAMETER.</para>
            /// <para>A process can use the FreeConsole function to detach itself from its console.
            /// If other processes share the console, the console is not destroyed,
            /// but the process that called <see cref="FreeConsole"/> cannot refer to it.
            /// A console is closed when the last process attached to it terminates or calls <see cref="FreeConsole"/>.
            /// After a process calls <see cref="FreeConsole"/>, it can call the <see cref="AllocConsole"/> function
            /// to create a new console or <see cref="AttachConsole(uint)"/>
            /// to attach to another console.</para>
            /// <para>This function is primarily useful to applications that were linked with /SUBSYSTEM:WINDOWS,
            /// which implies to the operating system that a console is not needed before entering the program's main method.
            /// In that instance, the standard handles retrieved with GetStdHandle will likely be invalid on startup until AttachConsole is called.
            /// The exception to this is if the application is launched with handle inheritance by its parent process.</para>
            /// <para>To compile an application that uses this function, define _WIN32_WINNT as 0x0501 or later.
            /// For more information, see <see href="https://learn.microsoft.com/en-us/windows/win32/winprog/using-the-windows-headers">Using the Windows Headers</see>.</para>
            /// </remarks>
#if NET7_0_OR_GREATER
            [LibraryImport("kernel32.dll", EntryPoint = nameof(AttachConsole), SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool AttachConsole(uint processId);
#else
            [DllImport("kernel32.dll", EntryPoint = nameof(AttachConsole), ExactSpelling = true, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AttachConsole(uint processId);
#endif  // NET7_0_OR_GREATER

            /// <summary>
            /// Allocates a new console for the calling process.
            /// </summary>
            /// <returns>
            /// <para>If the function succeeds, the return value is true.</para>
            /// <para>If the function fails, the return value is false. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <para><seealso href="https://learn.microsoft.com/en-us/windows/console/allocconsole"/></para>
            /// <para>
            /// A process can be associated with only one console,
            /// so the AllocConsole function fails if the calling process already has a console.
            /// A process can use the <see cref="FreeConsole"/> function to detach itself from its current console,
            /// then it can call AllocConsole to create a new console or AttachConsole to attach to another console.
            /// </para>
            /// If the calling process creates a child process, the child inherits the new console.
            /// <para>
            /// </para>
            /// <see cref="AllocConsole"/> initializes standard input, standard output, and standard error handles for the new console.
            /// The standard input handle is a handle to the console's input buffer,
            /// and the standard output and standard error handles are handles to the console's screen buffer.
            /// To retrieve these handles, use the GetStdHandle function.
            /// <para>
            /// This function is primarily used by a graphical user interface (GUI) application to create a console window.
            /// GUI applications are initialized without a console.
            /// Console applications are initialized with a console, unless they are created as detached processes
            /// (by calling the CreateProcess function with the DETACHED_PROCESS flag).
            /// </para>
            /// </remarks>
#if NET7_0_OR_GREATER
            [LibraryImport("kernel32.dll", EntryPoint = nameof(AllocConsole), SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool AllocConsole();
#else
            [DllImport("kernel32.dll", EntryPoint = nameof(AllocConsole), ExactSpelling = true, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AllocConsole();
#endif  // NET7_0_OR_GREATER

            /// <summary>
            /// Detaches the calling process from its console.
            /// </summary>
            /// <returns>
            /// <para>If the function succeeds, the return value is true.</para>
            /// <para>If the function fails, the return value is false. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <para><seealso href="https://learn.microsoft.com/en-us/windows/console/freeconsole"/></para>
            /// <para>
            /// A process can be attached to at most one console.
            /// A process can use the <see cref="FreeConsole"/> function to detach itself from its console.
            /// If other processes share the console, the console is not destroyed, but the process that called FreeConsole cannot refer to it.
            /// A console is closed when the last process attached to it terminates or calls FreeConsole.
            /// After a process calls <see cref="FreeConsole"/>, it can call the <see cref="AllocConsole"/> function to create a new console
            /// or <see cref="AttachConsole(uint)"/> to attach to another console.
            /// If the calling process is not already attached to a console, the <see cref="FreeConsole"/> request still succeeds.
            /// </para>
            /// </remarks>
#if NET7_0_OR_GREATER
            [LibraryImport("kernel32.dll", EntryPoint = nameof(FreeConsole), SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool FreeConsole();
#else
            [DllImport("kernel32.dll", EntryPoint = nameof(FreeConsole), ExactSpelling = true, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FreeConsole();
#endif  // NET7_0_OR_GREATER

            /// <summary>
            /// Retrieves the window handle used by the console associated with the calling process.
            /// </summary>
            /// <returns>The return value is a handle to the window used by the console associated with the calling process
            /// or <see cref="IntPtr.Zero"/> if there is no such associated console.</returns>
            /// <remarks>
            /// <para><seealso href="https://learn.microsoft.com/en-us/windows/console/getconsolewindow"/></para>
            /// <para>This API is not recommended and does not have a virtual terminal equivalent.
            /// This decision intentionally aligns the Windows platform with other operating systems.
            /// This state is only relevant to the local user, session, and privilege context.
            /// Applications remoting via cross-platform utilities and transports like SSH may not work as expected if using this API.</para>
            /// <para>
            /// For an application that is hosted inside a pseudoconsole session, this function returns a window handle for message queue purposes only.
            /// The associated window is not displayed locally as the pseudoconsole is serializing all actions to a stream for presentation on another terminal window elsewhere.
            /// </para>
            /// </remarks>
#if NET7_0_OR_GREATER
            [LibraryImport("kernel32.dll", EntryPoint = nameof(GetConsoleWindow), SetLastError = true)]
            public static partial IntPtr GetConsoleWindow();
#else
            [DllImport("kernel32.dll", EntryPoint = nameof(GetConsoleWindow), ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetConsoleWindow();
#endif  // NET7_0_OR_GREATER
        }
    }
}
