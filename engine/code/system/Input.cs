using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public static class Input
    {
        static KeyboardState keyboardState;
        static KeyboardState lastKeyboardState;

        static MouseState mouseState;
        static MouseState lastMouseState;

        //static GamePadState[] gamePadStates;
        //static GamePadState[] lastGamePadStates;

        public static Point mousePosition;

        static Input()
        {
            Update(0, 0);
            Update(0, 0);
        }

        public static void Update(float delta, float total)
        {
            lastKeyboardState = keyboardState;
            lastMouseState = mouseState;

            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            //for (int i = 0; i < 4; i++)
            //{
            //    lastGamePadStates[i] = gamePadStates[i];

            //    gamePadStates[i] = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.Circular);
            //}

            mousePosition = new Point(mouseState.X, mouseState.Y);

            UpdateTextInput(delta, total);
        }




        public static bool KeyPressed(Keys key) { return keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key); }
        public static bool KeyHeld(Keys key) { return keyboardState.IsKeyDown(key); }
        public static bool KeyReleased(Keys key) { return keyboardState.IsKeyUp(key) && lastKeyboardState.IsKeyDown(key); }

        public static bool KeysPressed(params Keys[] keys)
        {
            bool pressed = false;

            foreach (Keys key in keys)
            {
                if (keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key))
                    pressed = true;
            }

            return pressed;
        }

        public static bool KeysHeld(params Keys[] keys)
        {
            bool pressed = false;

            foreach (Keys key in keys)
            {
                if (keyboardState.IsKeyDown(key))
                    pressed = true;
            }

            return pressed;
        }

        public static bool KeysReleased(params Keys[] keys)
        {
            bool pressed = false;

            foreach (Keys key in keys)
            {
                if (keyboardState.IsKeyUp(key) && lastKeyboardState.IsKeyDown(key))
                    pressed = true;
            }

            return pressed;
        }


        public static bool LMB_Pressed() { return mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released; }
        public static bool LMB_Held() { return mouseState.LeftButton == ButtonState.Pressed; }
        public static bool LMB_Released() { return mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed; }

        public static bool MMB_Pressed() { return mouseState.MiddleButton == ButtonState.Pressed && lastMouseState.MiddleButton == ButtonState.Released; }
        public static bool MMB_Held() { return mouseState.MiddleButton == ButtonState.Pressed; }
        public static bool MMB_Released() { return mouseState.MiddleButton == ButtonState.Released && lastMouseState.MiddleButton == ButtonState.Pressed; }

        public static bool RMB_Pressed() { return mouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released; }
        public static bool RMB_Held() { return mouseState.RightButton == ButtonState.Pressed; }
        public static bool RMB_Released() { return mouseState.RightButton == ButtonState.Released && lastMouseState.RightButton == ButtonState.Pressed; }

        public static void SetMousePosition(int x, int y)
        {
            Mouse.SetPosition(x, y);

            // this is dumb but mandatory
            mouseState = new MouseState(x, y,
                mouseState.ScrollWheelValue,
                mouseState.LeftButton,
                mouseState.MiddleButton,
                mouseState.RightButton,
                mouseState.XButton1,
                mouseState.XButton2);
        }


        #region Text Input

        static StringBuilder inputBuffer;

        //Action OnInputConfirm;  // enter
        //Action OnInputCancel;   // escape

        // int = position, bool true = exact position, bool false = position difference
        //static Action<int, bool> caretPositionUpdated;
        //static Func<int> caretQuery;
        static int caretPosition;

        public static int CarotPosition
        {
            get
            {
                return caretPosition;
            }

            set
            {
                caretPosition = value;
            }
        }

        // thinking of a better way to do this
        static float backspaceHoldTime = 0f;
        static float backspaceThreshold = 0.04f;
        static float backspaceDelay = 0.4f;

        static float leftHoldTime = 0f;
        static float leftThreshold = 0.04f;
        static float leftDelay = 0.4f;

        static float rightHoldTime = 0f;
        static float rightThreshold = 0.04f;
        static float rightDelay = 0.4f;

        static float timeSinceModified = 0.0f;

        public static float TimeSinceInputBufferModified
        {
            get
            {
                return timeSinceModified;
            }
        }

        // preferably do not run inputs elsewhere while active
        public static bool IsInputActive
        {
            get
            {
                return inputBuffer != null;
            }
        }

        public static void BindInputBuffer(StringBuilder _inputBuffer)
        {
            inputBuffer = _inputBuffer;
            timeSinceModified = 0f;
            backspaceHoldTime = 0f;
            caretPosition = 0;
        }

        public static StringBuilder GetInputBuffer()
        {
            return inputBuffer;
        }

        static void UpdateTextInput(float delta, float total)
        {
            if (!IsInputActive)
                return;

            timeSinceModified += delta;

            // moved out of TextInputWidget so all forms of text input support pasting
            if (KeyHeld(Keys.LeftControl) && KeyPressed(Keys.V))
            {
                //string clipboardText = System.Windows.Forms.Clipboard.GetText();
                //if (inputBuffer.Length + clipboardText.Length > inputBuffer.Capacity)
                //    clipboardText = clipboardText.Substring(0, inputBuffer.Capacity - inputBuffer.Length);

                //inputBuffer.Insert(caretPosition, clipboardText);
                //caretPosition += clipboardText.Length;
                // return so that pressed keys do not interfere
                return;
            }

            if (KeyHeld(Keys.LeftControl) && KeyPressed(Keys.C))
            {
                //System.Windows.Forms.Clipboard.SetText(inputBuffer.ToString());
                // return so that pressed keys do not interfere
                return;
            }

            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            Keys[] lastPressedKeys = lastKeyboardState.GetPressedKeys();

            for (int i = 0; i < pressedKeys.Length; i++)
            {
                bool newKey = true;

                for (int j = 0; j < lastPressedKeys.Length; j++)
                {
                    if (pressedKeys[i] == lastPressedKeys[j])
                    {
                        // contains key
                        newKey = false;
                    }
                }

                // this key is new
                if (newKey)
                {
                    Keys key = pressedKeys[i];

                    ProcessInputKey(key);
                }
            }

            if (KeyHeld(Keys.Back))
            {
                backspaceHoldTime += delta;

                // while loop so slow framerates/update rates doesn't slow down repeat
                while (backspaceHoldTime > backspaceDelay + backspaceThreshold)
                {
                    backspaceHoldTime -= backspaceThreshold;

                    if (inputBuffer.Length > 0 && caretPosition > 0)
                    {
                        caretPosition--;
                        inputBuffer.Remove(caretPosition, 1);
                        timeSinceModified = 0f;
                    }
                }
            }
            else
            {
                backspaceHoldTime = 0f;
            }

            if (KeyHeld(Keys.Right))
            {
                rightHoldTime += delta;

                // while loop so slow framerates/update rates doesn't slow down repeat
                while (rightHoldTime > rightDelay + rightThreshold)
                {
                    rightHoldTime -= rightThreshold;
                    if (caretPosition < inputBuffer.Length)
                    {
                        caretPosition++;
                        timeSinceModified = 0f;
                    }
                }
            }
            else
            {
                rightHoldTime = 0f;
            }

            if (KeyHeld(Keys.Left))
            {
                leftHoldTime += delta;

                // while loop so slow framerates/update rates doesn't slow down repeat
                while (leftHoldTime > leftDelay + leftThreshold)
                {
                    leftHoldTime -= leftThreshold;
                    if (caretPosition > 0)
                    {
                        caretPosition--;
                        timeSinceModified = 0f;
                    }
                }
            }
            else
            {
                leftHoldTime = 0f;
            }
        }

        static void ProcessInputKey(Keys key)
        {
            string pressedSet = null;

            // oh no
            switch (key)
            {
                case Keys.D1: pressedSet = "1!"; break;
                case Keys.D2: pressedSet = "2@"; break;
                case Keys.D3: pressedSet = "3#"; break;
                case Keys.D4: pressedSet = "4$"; break;
                case Keys.D5: pressedSet = "5%"; break;
                case Keys.D6: pressedSet = "6^"; break;
                case Keys.D7: pressedSet = "7&"; break;
                case Keys.D8: pressedSet = "8*"; break;
                case Keys.D9: pressedSet = "9("; break;
                case Keys.D0: pressedSet = "0)"; break;

                case Keys.A: pressedSet = "aA"; break;
                case Keys.B: pressedSet = "bB"; break;
                case Keys.C: pressedSet = "cC"; break;
                case Keys.D: pressedSet = "dD"; break;
                case Keys.E: pressedSet = "eE"; break;
                case Keys.F: pressedSet = "fF"; break;
                case Keys.G: pressedSet = "gG"; break;
                case Keys.H: pressedSet = "hH"; break;
                case Keys.I: pressedSet = "iI"; break;
                case Keys.J: pressedSet = "jJ"; break;
                case Keys.K: pressedSet = "kK"; break;
                case Keys.L: pressedSet = "lL"; break;
                case Keys.M: pressedSet = "mM"; break;
                case Keys.N: pressedSet = "nN"; break;
                case Keys.O: pressedSet = "oO"; break;
                case Keys.P: pressedSet = "pP"; break;
                case Keys.Q: pressedSet = "qQ"; break;
                case Keys.R: pressedSet = "rR"; break;
                case Keys.S: pressedSet = "sS"; break;
                case Keys.T: pressedSet = "tT"; break;
                case Keys.U: pressedSet = "uU"; break;
                case Keys.V: pressedSet = "vV"; break;
                case Keys.W: pressedSet = "wW"; break;
                case Keys.X: pressedSet = "xX"; break;
                case Keys.Y: pressedSet = "yY"; break;
                case Keys.Z: pressedSet = "zZ"; break;

                case Keys.Space: pressedSet = "  "; break;

                case Keys.OemTilde: pressedSet = "`~"; break;
                case Keys.OemBackslash: pressedSet = @"\|"; break;
                case Keys.OemPipe: pressedSet = @"\|"; break;
                case Keys.OemCloseBrackets: pressedSet = "]}"; break;
                case Keys.OemOpenBrackets: pressedSet = "[{"; break;
                case Keys.OemComma: pressedSet = ",<"; break;
                case Keys.OemPeriod: pressedSet = ".>"; break;
                case Keys.OemQuestion: pressedSet = "/?"; break;
                case Keys.OemSemicolon: pressedSet = ";:"; break;
                case Keys.OemPlus: pressedSet = "=+"; break;
                case Keys.OemQuotes: pressedSet = "\'\""; break;
                case Keys.OemMinus: pressedSet = "-_"; break;
            }

            if (pressedSet != null)
            {
                int offset = 0;
                if (KeyHeld(Keys.LeftShift) || KeyHeld(Keys.RightShift))
                    offset = 1;

                if (inputBuffer.Length < inputBuffer.Capacity)
                {
                    inputBuffer.Insert(caretPosition, pressedSet[offset]);
                    caretPosition++;
                    timeSinceModified = 0f;
                }
            }

            if (KeyPressed(Keys.Back))
            {
                if (inputBuffer.Length > 0 && caretPosition > 0)
                {
                    caretPosition--;
                    inputBuffer.Remove(caretPosition, 1);
                    timeSinceModified = 0f;
                }
            }

            if (KeyPressed(Keys.Delete))
            {
                if (caretPosition < inputBuffer.Length)
                {
                    inputBuffer.Remove(caretPosition, 1);
                    timeSinceModified = 0f;
                }
            }

            if (KeyPressed(Keys.Left))
            {
                if (caretPosition > 0)
                {
                    caretPosition--;
                    timeSinceModified = 0f;
                }
            }

            if (KeyPressed(Keys.Right))
            {
                if (caretPosition < inputBuffer.Length)
                {
                    caretPosition++;
                    timeSinceModified = 0f;
                }
            }
        }

        public static void ClearInputBuffer()
        {
            inputBuffer.Clear();
            caretPosition = 0;
        }

        public static void UnbindInputBuffer()
        {
            inputBuffer = null;
            caretPosition = 0;
        }

        #endregion Text Input
    }
}
