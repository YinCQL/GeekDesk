﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


/// <summary>
/// 热键注册
/// </summary>
namespace GeekDesk.Util
{

    class Hotkey
    {
        #region 系统api
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, HotkeyModifiers fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        #endregion

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="window">持有快捷键窗口</param>
        /// <param name="fsModifiers">组合键</param>
        /// <param name="key">快捷键</param>
        /// <param name="callBack">回调函数</param>
        public static int Regist(IntPtr windowHandle, HotkeyModifiers fsModifiers, Key key, HotKeyCallBackHanlder callBack)
        {
            HwndSource hs = HwndSource.FromHwnd(windowHandle);
            hs.AddHook(WndProc);

            int id = keyid++;
            int vk = KeyInterop.VirtualKeyFromKey(key);
            keymap.Add(id, callBack);
            if (!RegisterHotKey(windowHandle, id, fsModifiers, (uint)vk)) throw new Exception("RegisterHotKey Failed");
            return id;
        }

        /// <summary>
        /// 快捷键消息处理
        /// </summary>
        static IntPtr WndProc(IntPtr windowHandle, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (keymap.TryGetValue(id, out var callback))
                {
                    callback();
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        /// <param name="hWnd">持有快捷键窗口的句柄</param>
        /// <param name="callBack">回调函数</param>
        public static void UnRegist(IntPtr windowHandle, HotKeyCallBackHanlder callBack)
        {
            List<int> list = new List<int>(keymap.Keys);
            for (int i=0; i < list.Count; i++)
            {
                if (keymap[list[i]] == callBack)
                {
                    HwndSource hs = HwndSource.FromHwnd(windowHandle);
                    hs.RemoveHook(WndProc);
                    UnregisterHotKey(windowHandle, list[i]);
                    keymap.Remove(list[i]);
                }
            }
        }

        const int WM_HOTKEY = 0x312;
        static int keyid = 10;
        public static Dictionary<int, HotKeyCallBackHanlder> keymap = new Dictionary<int, HotKeyCallBackHanlder>();

        public delegate void HotKeyCallBackHanlder();
    }

    public enum HotkeyModifiers
    {
        MOD_ALT = 0x1,
        MOD_CONTROL = 0x2,
        MOD_SHIFT = 0x4,
        MOD_WIN = 0x8
    }

}