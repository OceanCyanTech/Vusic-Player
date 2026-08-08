using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.AppConfig
{
    public static class MediaControlSystem
    {
        public const int WM_HOTKEY = 0x0312;

        public const int HOTKEY_PLAY_PAUSE = 1001;
        public const int HOTKEY_NEXT = 1002;
        public const int HOTKEY_PREV = 1003;

        private const uint VK_MEDIA_NEXT_TRACK = 0xB0;
        private const uint VK_MEDIA_PREV_TRACK = 0xB1;
        private const uint VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const uint MOD_NOREPEAT = 0x4000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static void Register(IntPtr hwnd)
        {
            RegisterHotKey(hwnd, HOTKEY_PLAY_PAUSE, MOD_NOREPEAT, VK_MEDIA_PLAY_PAUSE);
            RegisterHotKey(hwnd, HOTKEY_NEXT, MOD_NOREPEAT, VK_MEDIA_NEXT_TRACK);
            RegisterHotKey(hwnd, HOTKEY_PREV, MOD_NOREPEAT, VK_MEDIA_PREV_TRACK);
        }

        public static void Unregister(IntPtr hwnd)
        {
            UnregisterHotKey(hwnd, HOTKEY_PLAY_PAUSE);
            UnregisterHotKey(hwnd, HOTKEY_NEXT);
            UnregisterHotKey(hwnd, HOTKEY_PREV);
        }
    }
}
