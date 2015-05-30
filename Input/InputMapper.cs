using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TLSA.Engine.Input
{
    /// <summary>
    /// Botones del raton.
    /// </summary>
    public enum MouseButtons
    {
        LeftButton, RightButton, MiddleButton
    }

    /// <summary>
    /// Dispositivos de entrada disponibles.
    /// </summary>
    public enum InputType
    {
        KeyboardMouse, GamePad
    }

    /// <summary>
    /// Accion del mapa de input.
    /// </summary>
    public class InputAction
    {
        public string Name;                         // Nombre de la accion.
        public Nullable<Keys> Key;                  // Tecla del teclado asociada.
        public Nullable<MouseButtons> MouseButton;  // Boton del raton.
        public Nullable<Buttons> Button;            // Boton del gamepad asociado.

        internal bool Hit;                          // Indica si se ha hecho pulsacion.

        public InputAction()
        {
            this.Hit = false;
        }

        public InputAction(Nullable<Keys> key, Nullable<MouseButtons> mouseButton, Nullable<Buttons> button) : this ()
        {
            this.Key = key; this.MouseButton = mouseButton; this.Button = button;
        }
    }

    /// <summary>
    /// Mapa de input.
    /// </summary>
    /// <remarks>Define un mapa de acciones para la entrada de valores unificando dispositivos y acciones por su nombre.</remarks>
    public class InputMap : TLSA.Engine.IUpdateable 
    {
        //private int MAX_ITEMS = 16;
        private float LEFT_VALUE = -0.5f;
        private float RIGHT_VALUE = 0.5f;
        private float DOWN_VALUE = -0.5f;
        private float UP_VALUE = 0.5f;

        private Vector2 vibrators = Vector2.Zero;   // Valores de los vibradores del gamepad.

        public bool Enabled { get; set; }

        /// <summary>
        /// Lista de acciones del mapa.
        /// </summary>
        public Dictionary<string, InputAction> Actions { get; set; }

        /// <summary>
        /// Jugador al que esta asociado el mapa.
        /// </summary>
        public PlayerIndex Player { get; set; }

        private KeyboardState keybState;
        private MouseState mouseState;
        private GamePadState gamepadState;

        /// <summary>
        /// Selecciona el dispositivo del que se leera la entrada de valores.
        /// </summary>
        public InputType SelectedDevice { get; set; }

        /// <summary>
        /// Indica si el dispostivo esta conectado.
        /// </summary>
        /// <remarks>Devuelve el estado del gamepad. En caso de tratarse de otro tipo de dispostivo como el teclado, raton o touchscreen devolvera siempre verdadero.</remarks>
        public bool IsConnected
        {
            get
            {
#if XBOX
                return gamepadState.IsConnected;
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="player">Jugador al que esta asociado el mapa.</param>
        /// <param name="SelectedDevice">Dispositivo seleccionado del que se leera la entrada de valores.</param>
        public InputMap(PlayerIndex player, InputType selectedDevice)
        {
            this.Player = player;
            this.Actions = new Dictionary<string, InputAction>();
            this.SelectedDevice = selectedDevice;
            this.Enabled = true;
        }

        /// <summary>
        /// Actualiza los estados de lectura de los dispositivos.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            if (this.Enabled)
            {
#if WINDOWS
                keybState = Keyboard.GetState(); 
                mouseState = Mouse.GetState();
#endif
                gamepadState = GamePad.GetState(Player, GamePadDeadZone.None);
            }
        }

        /// <summary>
        /// Comprueba si algun boton o tecla asociada a una accion esta pulsado.
        /// </summary>
        /// <returns>Devuelve verdadero si se esta presionando alguna tecla o boton.</returns>
        public bool PressAny()
        {
#if WINDOWS
            if (this.SelectedDevice == InputType.KeyboardMouse)
            {
                for (int i = 0; i < 255; i++)
                    if (keybState.IsKeyDown((Keys)i)) return true;

                return (mouseState.LeftButton == ButtonState.Pressed ||
                        mouseState.MiddleButton == ButtonState.Pressed ||
                        mouseState.RightButton == ButtonState.Pressed ||
                        mouseState.XButton1 == ButtonState.Pressed ||
                        mouseState.XButton2 == ButtonState.Pressed);
            }
#elif XBOX
            if (this.SelectedDevice == InputType.GamePad)
            {
                for (int i = 1; i <= 1073741824 && i != -2147483648; i = i * 2)
                    if (gamepadState.IsButtonDown((Buttons)i)) 
                        return true;
            }
#endif
            return false;   
        }


        /// <summary>
        /// Comprueba si el boton o tecla asociada a una accion esta pulsado.
        /// </summary>
        /// <param name="action">Nombre de la accion.</param>
        /// <returns>Devuelve verdadero si se esta presionando la tecla o boton.</returns>
        public bool Press(string action)
        {
            switch (this.SelectedDevice)
            {
#if WINDOWS
                case InputType.KeyboardMouse:
                    bool ret = false;
                    if (this.Actions[action].Key != null) ret =  keybState.IsKeyDown(this.Actions[action].Key.Value);
                    if (this.Actions[action].MouseButton != null)
                    {
                        switch (this.Actions[action].MouseButton)
                        {
                            case MouseButtons.LeftButton: ret = mouseState.LeftButton == ButtonState.Pressed; break;
                            case MouseButtons.RightButton: ret = mouseState.RightButton == ButtonState.Pressed; break;
                            case MouseButtons.MiddleButton: ret = mouseState.MiddleButton == ButtonState.Pressed; break;
                            default: ret = false; break;
                        }
                    }
                    return ret;
#endif
                case InputType.GamePad:
                    if (this.Actions[action].Button.Value == Buttons.LeftThumbstickLeft)
                    {
                        return (gamepadState.ThumbSticks.Left.X <= LEFT_VALUE && gamepadState.IsButtonDown(Buttons.LeftThumbstickLeft));
                    }
                    else if (this.Actions[action].Button.Value == Buttons.LeftThumbstickRight)
                    {
                        return (gamepadState.ThumbSticks.Left.X >= RIGHT_VALUE && gamepadState.IsButtonDown(Buttons.LeftThumbstickRight));
                    }
                    else if (this.Actions[action].Button.Value == Buttons.LeftThumbstickDown)
                    {
                        return (gamepadState.ThumbSticks.Left.Y <= DOWN_VALUE && gamepadState.IsButtonDown(Buttons.LeftThumbstickDown));
                    }
                    else if (this.Actions[action].Button.Value == Buttons.LeftThumbstickUp)
                    {
                        return (gamepadState.ThumbSticks.Left.Y >= UP_VALUE && gamepadState.IsButtonDown(Buttons.LeftThumbstickUp));
                    }
                    else
                        return gamepadState.IsButtonDown(this.Actions[action].Button.Value);
                default: return false;
            }
        }

        /// <summary>
        /// Comprueba si el boton o tecla asociada a una accion no esta pulsado.
        /// </summary>
        /// <param name="action">Nombre de la accion.</param>
        /// <returns>Devuelve verdadero si no se esta presionando la tecla o boton.</returns>
        public bool Release(string action)
        {
            switch (this.SelectedDevice)
            {
#if WINDOWS
                case InputType.KeyboardMouse:
                    bool ret = false;
                    if (this.Actions[action].Key != null) ret = keybState.IsKeyUp(this.Actions[action].Key.Value);
                    if (this.Actions[action].MouseButton != null)
                    {
                        switch (this.Actions[action].MouseButton)
                        {
                            case MouseButtons.LeftButton: ret = mouseState.LeftButton == ButtonState.Released; break;
                            case MouseButtons.RightButton: ret = mouseState.RightButton == ButtonState.Released; break;
                            case MouseButtons.MiddleButton: ret = mouseState.MiddleButton == ButtonState.Released; break;
                            default: ret = false; break;
                        }
                    }
                    return ret; 
#endif
                case InputType.GamePad: return gamepadState.IsButtonUp(this.Actions[action].Button.Value);
                default: return false;
            }
        }

        /// <summary>
        /// Comprueba si el boton o tecla asociada ha sido pulsado.
        /// </summary>
        /// <param name="action">Nombre de la accion.</param>
        /// <returns>Devuelve verdadero en cuanto se detecte la pulsacion y falso mientras continue pulsado.</returns>
        public bool Hit(string action)
        {
            if (Press(action) && !this.Actions[action].Hit)
            {
                this.Actions[action].Hit = true;
                return true;
            }
            else if (Press(action) && this.Actions[action].Hit)
            {
                return false;
            }
            else if (!Press(action) && this.Actions[action].Hit)
            {
                this.Actions[action].Hit = false;
            }
            return false;
        }

#if WINDOWS
        /// <summary>
        /// Devuelve el estado de las teclas Shift.
        /// </summary>
        /// <returns>Devuelve verdadero si alguna de las dos teclas (derecha o izquierda) esta pulsada.</returns>
        public bool IsShiftKeyPressed
        {
            get
            {
                return (this.SelectedDevice == InputType.KeyboardMouse)
                    && (keybState.IsKeyDown(Keys.LeftShift) || keybState.IsKeyDown(Keys.RightShift));
            }
        }

        /// <summary>
        /// Devuelve el estado de las teclas Control.
        /// </summary>
        /// <returns>Devuelve verdadero si alguna de las dos teclas (derecha o izquierda) esta pulsada.</returns>
        public bool IsControlKeyPressed
        {
            get
            {
                return (this.SelectedDevice == InputType.KeyboardMouse)
                    && (keybState.IsKeyDown(Keys.LeftControl) || keybState.IsKeyDown(Keys.RightControl));
            }
        }

        /// <summary>
        /// Devuelve el estado de las teclas Alt.
        /// </summary>
        /// <returns>Devuelve verdadero si alguna de las dos teclas (derecha o izquierda) esta pulsada.</returns>
        public bool IsAltKeyPressed
        {
            get
            {
                return (this.SelectedDevice == InputType.KeyboardMouse)
                    && (keybState.IsKeyDown(Keys.LeftAlt) || keybState.IsKeyDown(Keys.RightAlt));
            }
        } 
#endif

        // Devuelve las coordenadas del eje principal (raton o stick izquierdo del gamepad):
        public Vector2 PrimaryAxis()
        {
            if (this.SelectedDevice == InputType.KeyboardMouse)
            {
#if WINDOWS
                return new Vector2((float)mouseState.X, (float)mouseState.Y);
#else
                return Vector2.Zero;
#endif
            }
            else if (this.SelectedDevice == InputType.GamePad)
            {
                return gamepadState.ThumbSticks.Left;
            }
            return Vector2.Zero;
        }

        // Devuelve las coordenadas del eje secundario (solo stick derecho del gamepad):
        public Vector2 SecondaryAxis()
        {
            if (this.SelectedDevice == InputType.GamePad)
            {
                return gamepadState.ThumbSticks.Left;
            }
            return Vector2.Zero;
        }

#if XBOX
        /// <summary>
        /// Devuelve o establece la potencia de los vibradores del gamepad.
        /// </summary>
        public Vector2 Vibrators
        {
            get
            {
                if (this.SelectedDevice == InputType.GamePad && this.IsConnected)
                {
                    return this.vibrators;
                }
                else
                    return Vector2.Zero;
            }
            set
            {
                if (this.SelectedDevice == InputType.GamePad && this.IsConnected)
                {
                    this.vibrators = value;
                    GamePad.SetVibration(this.Player, value.X, value.Y);
                }
            }
        }  
#endif
    }
}
