using System;
using System.Runtime.InteropServices;

namespace Platform.Sandbox
{
    /// <summary>
    /// Представляет тест, который позволит определить необходимо ли указывать расширение файла при подключении библиотеки.
    /// TODO: Проверить на пользовательских DLL
    /// TODO: Далее протестировать на CoreCLR, Mono
    /// </summary>
    public class DllImportTest
    {
        // Use DllImport to import the Win32 MessageBox function.
        [DllImport("user32", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        public static void Test()
        {
            // Call the MessageBox function using platform invoke.
            MessageBox(new IntPtr(0), "Hello World!", "Hello Dialog", 0);
        }
    }
}
