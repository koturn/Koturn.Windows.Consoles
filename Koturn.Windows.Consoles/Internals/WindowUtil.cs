#if NET7_0_OR_GREATER
#    define SUPPORT_LIBRARY_IMPORT
#endif  // NET7_0_OR_GREATER

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;


namespace Koturn.Windows.Consoles.Internals
{
    /// <summary>
    /// Provides utility methods for window.
    /// </summary>
#if NET7_0_OR_GREATER
    internal static partial class WindowUtil
#else
    internal static class WindowUtil
#endif  // NET7_0_OR_GREATER
    {
        /// <summary>
        /// Sets the specified window's show state.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="cmdShow">Controls how the window is to be shown.</param>
        /// <returns>
        /// <para>If the window was previously visible, the return value is false.</para>
        /// <para>If the window was previously hidden, the return value is true.</para>
        /// </returns>
        public static bool ShowWindow(IntPtr hWnd, CmdShow cmdShow)
        {
            return SafeNativeMethods.ShowWindow(hWnd, cmdShow);
        }

        /// <summary>
        /// Get window menu (also known as the system menu or the control menu) for copying and modifying.
        /// </summary>
        /// <param name="hWnd">A handle to the window that will own a copy of the window menu.</param>
        /// <param name="doRevert">
        /// The action to be taken.
        /// If this parameter is false, <see cref="GetSystemMenu"/> returns a handle to the copy of the window menu currently in use.
        /// The copy is initially identical to the window menu, but it can be modified.
        /// If this parameter is true, <see cref="GetSystemMenu"/> resets the window menu back to the default state.
        /// The previous window menu, if any, is destroyed.</param>
        /// <returns>If the <paramref name="doRevert"/> parameter is false, the return value is a handle to a copy of the window menu.
        /// If the <paramref name="doRevert"/> parameter is true, the return value is <see cref="IntPtr.Zero"/>.</returns>
        public static IntPtr GetSystemMenu(IntPtr hWnd, bool doRevert)
        {
            var hMenu = SafeNativeMethods.GetSystemMenu(hWnd, doRevert);
            if (hMenu == IntPtr.Zero && !doRevert)
            {
                ThrowHelper.ThrowLastWin32Exception(nameof(SafeNativeMethods.GetSystemMenu) + " failed");
            }
            return hMenu;
        }

        /// <summary>
        /// Deletes a menu item or detaches a submenu from the specified menu by command.
        /// </summary>
        /// <param name="hMenu">A handle to the menu to be changed.</param>
        /// <param name="syscmd">The menu item to be deleted.</param>
        /// <exception cref="Win32Exception">Thrown when <see cref="SafeNativeMethods.RemoveMenu(IntPtr, uint, MenuFlags)"/> failed.</exception>
        public static void RemoveMenu(IntPtr hMenu, SysCommand syscmd)
        {
            if (!SafeNativeMethods.RemoveMenu(hMenu, (uint)syscmd, MenuFlags.ByCommand))
            {
                ThrowHelper.ThrowLastWin32Exception(nameof(SafeNativeMethods.RemoveMenu) + " failed");
            }
        }

        /// <summary>
        /// Deletes a menu item or detaches a submenu from the specified menu by position.
        /// </summary>
        /// <param name="hMenu">A handle to the menu to be changed.</param>
        /// <param name="position">The menu item to be deleted.</param>
        /// <exception cref="Win32Exception">Thrown when <see cref="SafeNativeMethods.RemoveMenu(IntPtr, uint, MenuFlags)"/> failed.</exception>
        public static void RemoveMenu(IntPtr hMenu, uint position)
        {
            if (!SafeNativeMethods.RemoveMenu(hMenu, position, MenuFlags.ByPosition))
            {
                ThrowHelper.ThrowLastWin32Exception(nameof(SafeNativeMethods.RemoveMenu) + "failed");
            }
        }

        /// <summary>
        /// Provides some P/Invoke methods.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
#if NET7_0_OR_GREATER
        internal static partial class SafeNativeMethods
#else
        internal static class SafeNativeMethods
#endif  // NET7_0_OR_GREATER
        {
            /// <summary>
            /// Sets the specified window's show state.
            /// </summary>
            /// <param name="hWnd">A handle to the window.</param>
            /// <param name="cmdShow">
            /// Controls how the window is to be shown.
            /// This parameter is ignored the first time an application calls <see cref="ShowWindow"/>, if the program that launched the application provides a STARTUPINFO structure.
            /// Otherwise, the first time <see cref="ShowWindow"/> is called, the value should be the value obtained by the WinMain function in its nCmdShow parameter.
            /// </param>
            /// <returns>
            /// <para>If the window was previously visible, the return value is false.</para>
            /// <para>If the window was previously hidden, the return value is true.</para>
            /// </returns>
            /// <remarks>
            /// <para><see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow"/></para>
            /// <para>To perform certain special effects when showing or hiding a window, use AnimateWindow.</para>
            /// <para>The first time an application calls <see cref="ShowWindow"/>, it should use the WinMain function's nCmdShow parameter as its nCmdShow parameter.
            /// Subsequent calls to <see cref="ShowWindow"/> must use one of the values in the given list, instead of the one specified by the WinMain function's nCmdShow parameter.</para>
            /// <para>As noted in the discussion of the nCmdShow parameter, the nCmdShow value is ignored in the first call to <see cref="ShowWindow"/>
            /// if the program that launched the application specifies startup information in the structure.
            /// In this case, <see cref="ShowWindow"/> uses the information specified in the STARTUPINFO structure to show the window.
            /// On subsequent calls, the application must call <see cref="ShowWindow"/> with nCmdShow set to SW_SHOWDEFAULT to use the startup information provided by the program that launched the application.
            /// This behavior is designed for the following situations:
            /// <list type="bullet">
            ///   <item>Applications create their main window by calling CreateWindow with the WS_VISIBLE flag set.</item>
            ///   <item>Applications create their main window by calling CreateWindow with the WS_VISIBLE flag cleared,
            ///   and later call <see cref="ShowWindow"/> with the <see cref="CmdShow.Show"/> flag set to make it visible.</item>
            /// </list>
            /// </para>
            /// </remarks>
#if NET7_0_OR_GREATER
            [LibraryImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool ShowWindow(IntPtr hWnd, CmdShow cmdShow);
#else
            [DllImport("user32.dll", EntryPoint = "ShowWindow", ExactSpelling = true, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindow(IntPtr hWnd, CmdShow cmdShow);
#endif  // NET7_0_OR_GREATER

            /// <summary>
            /// Enables the application to access the window menu (also known as the system menu or the control menu) for copying and modifying.
            /// </summary>
            /// <param name="hWnd">A handle to the window that will own a copy of the window menu.</param>
            /// <param name="doRevert">
            /// The action to be taken.
            /// If this parameter is false, <see cref="GetSystemMenu"/> returns a handle to the copy of the window menu currently in use.
            /// The copy is initially identical to the window menu, but it can be modified.
            /// If this parameter is true, <see cref="GetSystemMenu"/> resets the window menu back to the default state.
            /// The previous window menu, if any, is destroyed.</param>
            /// <returns>If the <paramref name="doRevert"/> parameter is false, the return value is a handle to a copy of the window menu.
            /// If the <paramref name="doRevert"/> parameter is true, the return value is <see cref="IntPtr.Zero"/>.</returns>
            /// <remarks>
            /// <para><seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmenu"/></para>
            /// <para>Any window that does not use the GetSystemMenu function to make its own copy of the window menu receives the standard window menu.</para>
            /// <para>The window menu initially contains items with various identifier values,
            /// such as <see cref="SysCommand.Close"/>, <see cref="SysCommand.Move"/>, and <see cref="SysCommand.Size"/>.</para>
            /// <para>Menu items on the window menu send WM_SYSCOMMAND messages.</para>
            /// <para>All predefined window menu items have identifier numbers greater than 0xF000.
            /// If an application adds commands to the window menu, it should use identifier numbers less than 0xF000.</para>
            /// <para>The system automatically grays items on the standard window menu, depending on the situation.
            /// The application can perform its own checking or graying by responding to the WM_INITMENU message that is sent before any menu is displayed.</para>
            /// </remarks>
#if NET7_0_OR_GREATER
            [LibraryImport("user32.dll", EntryPoint = "GetSystemMenu", SetLastError = true)]
            public static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.U1)] bool doRevert);
#else
            [DllImport("user32.dll", EntryPoint = "GetSystemMenu", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.U1)] bool doRevert);
#endif  // NET7_0_OR_GREATER

            /// <summary>
            /// Deletes a menu item or detaches a submenu from the specified menu.
            /// If the menu item opens a drop-down menu or submenu, <see cref="RemoveMenu(IntPtr, uint, MenuFlags)"/> does not destroy the menu or its handle,
            /// allowing the menu to be reused.
            /// Before this function is called, the GetSubMenu function should retrieve a handle to the drop-down menu or submenu.
            /// </summary>
            /// <param name="hMenu">A handle to the menu to be changed.</param>
            /// <param name="position">The menu item to be deleted, as determined by the <paramref name="flags"/> parameter.</param>
            /// <param name="flags">Indicates how the <paramref name="position"/> parameter is interpreted.
            /// This parameter must be one of the values of <see cref="MenuFlags"/>.</param>
            /// <returns>
            /// <para>If the function succeeds, the return value is true.</para>
            /// <para>If the function fails, the return value is false. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <para><see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-removemenu"/></para>
            /// <para>The application must call the DrawMenuBar function whenever a menu changes, whether the menu is in a displayed window.</para>
            /// </remarks>
#if NET7_0_OR_GREATER
            [LibraryImport("user32.dll", EntryPoint = "RemoveMenu", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool RemoveMenu(IntPtr hMenu, uint position, MenuFlags flags);
#else
            [DllImport("user32.dll", EntryPoint = "RemoveMenu", ExactSpelling = true, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RemoveMenu(IntPtr hMenu, uint position, MenuFlags flags);
#endif  // NET7_0_OR_GREATER
        }
    }
}
