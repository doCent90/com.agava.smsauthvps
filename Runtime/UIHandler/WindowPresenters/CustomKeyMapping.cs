using UnityEngine;

namespace Agava.Wink
{
    public static class CustomKeyMapping
    {
        public static string GetKey(KeyCode code) => code switch
        {
            KeyCode.Keypad0 => "0",
            KeyCode.Keypad1 => "1",
            KeyCode.Keypad2 => "2",
            KeyCode.Keypad3 => "3",
            KeyCode.Keypad4 => "4",
            KeyCode.Keypad5 => "5",
            KeyCode.Keypad6 => "6",
            KeyCode.Keypad7 => "7",
            KeyCode.Keypad8 => "8",
            KeyCode.Keypad9 => "9",
            _ => null,
        };
    }
}
