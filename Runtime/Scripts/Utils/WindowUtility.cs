#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace IVLab.MinVR3
{
	public static class WindowUtility {

		// http://unitydevelopers.blogspot.com/2015/04/set-size-and-position-of-windows.html
		// http://answers.unity3d.com/questions/148723/how-can-i-change-the-title-of-the-standalone-playe.html
		// http://matt.benic.us/post/88468666204/using-win32-api-to-get-specific-window-instance-in
		// http://answers.unity3d.com/questions/936814/choose-screen-with-command-line-arguments.html
		// https://gist.github.com/mattbenic/908483ad0bedbc62ab17
		// Unity3D does not have a built-in function to set the position of an window
#region Content for setting x,y position and width/height
		private const string UnityWindowClassName = "UnityWndClass";

		[DllImport("kernel32.dll")]
		static extern uint GetCurrentThreadId();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int wFlags);

		[DllImport("user32.dll", EntryPoint = "SetWindowText")]
		static extern bool SetWindowText(IntPtr hwnd, System.String lpString);

		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);
#endregion



		// https://forum.unity.com/threads/solved-how-to-remove-the-title-bar-of-a-game.729437/#post-4867424
#region Content for hiding title bar
        const int SWP_HIDEWINDOW = 0x80; //hide window flag.
        const int SWP_SHOWWINDOW = 0x40; //show window flag.
        const int SWP_NOMOVE = 0x0002; //don't move the window flag.
        const int SWP_NOSIZE = 0x0001; //don't resize the window flag.
        const uint WS_SIZEBOX = 0x00040000;
        const int GWL_STYLE = -16;
        const int WS_BORDER = 0x00800000; //window with border
        const int WS_DLGFRAME = 0x00400000; //window with double border but no title
        const int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar
        const int WS_SYSMENU = 0x00080000;      //window with no borders etc.
        const int WS_MAXIMIZEBOX = 0x00010000;
        const int WS_MINIMIZEBOX = 0x00020000;  //window with minimizebox
     
        [DllImport("user32.dll")]
        static extern System.IntPtr GetActiveWindow();
     
        [DllImport("user32.dll")]
        static extern int FindWindow(string lpClassName, string lpWindowName);
     
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(
            System.IntPtr hWnd, // window handle
            System.IntPtr hWndInsertAfter, // placement order of the window
            short X, // x position
            short Y, // y position
            short cx, // width
            short cy, // height
            uint uFlags // window flags.
        );
     
        [DllImport("user32.dll")]
        static extern System.IntPtr SetWindowLong(
             System.IntPtr hWnd, // window handle
             int nIndex,
             uint dwNewLong
        );
     
        [DllImport("user32.dll")]
        static extern System.IntPtr GetWindowLong(
            System.IntPtr hWnd,
            int nIndex
        );
     
        private static System.IntPtr HWND_TOP = new System.IntPtr(0);
        private static System.IntPtr HWND_TOPMOST = new System.IntPtr(-1);
        private static System.IntPtr HWND_NOTOPMOST = new System.IntPtr(-2);
#endregion

  
#region Methods for setting window position/width/height
		public static void SetPosition(int x, int y, int resX = 0, int resY = 0) 
		{
			// 1) get the current window handle.
			IntPtr windowHandle = WindowUtility.GetWindowHandle();

			// 2) offset and positio the window, if we got something.
			if (windowHandle != IntPtr.Zero) {
				SetWindowPos(windowHandle, 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);
			}
		}

		public static void SetWindowTitle(string title) {
			// 1) get the current window handle.
			IntPtr windowHandle = WindowUtility.GetWindowHandle();
			// 2) set the window title, if we got something.
			if (windowHandle != IntPtr.Zero) {
				SetWindowText(windowHandle, title);
			}
		}

		/// <summary>
		/// Gets the current window handle.
		/// </summary>
		/// <returns>The window handle.</returns>
		private static IntPtr GetWindowHandle() {
			IntPtr windowHandle = IntPtr.Zero;

			// enumerates all nonchild windows associated with the current thread.
			uint threadId = GetCurrentThreadId();
			EnumThreadWindows(threadId, (hWnd, lParam) =>
				{
					// retrieves the name of the class to which the specified window belongs.
					var classText = new StringBuilder(UnityWindowClassName.Length + 1);
					GetClassName(hWnd, classText, classText.Capacity);
					// compare to see if this is what we are looking for
					if (classText.ToString() == UnityWindowClassName)
					{
						windowHandle = hWnd;
						return false;
					}
					return true;
				}, IntPtr.Zero);

			//
			return windowHandle;
		}
#endregion
     
#region Methods for showing/hiding window borders
        public static void ShowWindowBorders(bool value){
			var hWnd = GetActiveWindow();
            if(Application.isEditor) return; //We don't want to hide the toolbar from our editor!
     
            int style = GetWindowLong(hWnd, GWL_STYLE).ToInt32(); //gets current style
     
            if(value){
                SetWindowLong(hWnd, GWL_STYLE, (uint)(style | WS_CAPTION | WS_SIZEBOX)); //Adds caption and the sizebox back.
                SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW); //Make the window normal.
            } else {
                SetWindowLong(hWnd, GWL_STYLE, (uint)(style & ~(WS_CAPTION | WS_SIZEBOX))); //removes caption and the sizebox from current style.
                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW); //Make the window render above toolbar.
            }
        }
#endregion

	}
}
#endif