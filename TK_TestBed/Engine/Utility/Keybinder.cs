using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;

namespace TK_TestBed.Engine
{
    class Keybinder
    {
        // The delegate type. This one is from the library, in the System namespace
        public delegate void KeyListener(KeyModifiers modifiers);

        private KeyboardDevice keyboard;

        Dictionary<Key,List<KeyListener>> Listeners = new Dictionary<Key, List<KeyListener>>();

        public Keybinder(KeyboardDevice kbd)
        {
            kbd.KeyDown += OnKeyDown;
            keyboard = kbd;
        }

        public void SubscribeListener(Key key, KeyListener handler)
        {
            if (Listeners.ContainsKey(key))
            {
                Listeners[key].Add(handler);
            }
            else
            {
                var newList = new List<KeyListener>();
                newList.Add(handler);
                Listeners.Add(key, newList);
            }
        }

        // TODO: UnsubscribeListener()
        //

        private void OnKeyDown(object sender, KeyboardKeyEventArgs keyboardKeyEventArgs)
        {
            List<KeyListener> listeners;
            //KeyModifiers mods = keyboardKeyEventArgs.Modifiers; // note that this always returns 0, probably a bug in openTK
            KeyModifiers mods = new KeyModifiers();
            mods = mods | (keyboard[Key.AltLeft] || keyboard[Key.AltRight] ? KeyModifiers.Alt : 0);
            mods = mods | (keyboard[Key.ShiftLeft] || keyboard[Key.ShiftRight] ? KeyModifiers.Shift : 0);
            mods = mods | (keyboard[Key.ControlLeft] || keyboard[Key.ControlRight] ? KeyModifiers.Control : 0);
            Listeners.TryGetValue(keyboardKeyEventArgs.Key, out listeners);
            if (listeners != null)
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    listeners[i](mods);
                }
            }
        }
    }
}
