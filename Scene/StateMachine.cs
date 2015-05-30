using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace TLSA.Engine.Scene
{
    /// <summary>
    /// Define un estado de la maquina de estados.
    /// </summary>
    /// <remarks>Herede esta clase para definir los estados de una maquina de estados.
    /// Esta clase le permite programar toda la logica y dibujado de escena definiendo el codigo concerniente a un estado de su juego, por ejemplo, un menu.</remarks>
    public class StateComponent
    {
        internal bool initialized = false;

        /// <summary>
        /// Codigo de inicializacion del estado.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Actualiza la logica del estado.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <remarks>Sobreescriba este metodo para programar su propio codigo de logica.</remarks>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Dibuja los objetos dentro del estado.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <remarks>Sobreescriba este metodo para programar su propio codigo de dibujado.</remarks>
        public virtual void Draw(GameTime gameTime)
        {
        }

        /// <summary>
        /// Destruye los objetos del estado.
        /// </summary>
        /// <remarks>Sobreescriba este metodo para programa su propio codigo para liberar los recursos definidos por el estado.
        /// Este metodo sera llamado por la maquina de estados, a la que este asociado, cuando cambie de estado si el parametro Persistent esta desactivado.</remarks>
        public virtual void Terminate()
        {
        }
    }

    /// <summary>
    /// Maquina de estados.
    /// </summary>
    /// <remarks>Permite definir diferentes secciones del juego mediante estados en los que podra programar el codigo de su juego.</remarks>
    public class StateMachine : IDisposable
    {
        /// <summary>
        /// Lista de estados.
        /// </summary>
        public Dictionary<string, StateComponent> States { get; internal set; }

        /// <summary>
        /// Nombre del estado actual.
        /// </summary>
        public String CurrerntState { get; internal set; }

        public StateMachine()
        {
            this.States = new Dictionary<string, StateComponent>();
            this.CurrerntState = "";
        }

        /// <summary>
        /// Cambia el estado de la maquina.
        /// </summary>
        /// <param name="state">Nombre del estado.</param>
        /// <remarks>Al cambiar de estado se descargara cualquier contenido que haya cargado en el Content.</remarks>
        public void ChangeState(string state)
        {
            // Procedemos a destruir sus recursos:
            if (this.CurrerntState != "")
            {
                this.States[this.CurrerntState].Terminate();
                this.States[this.CurrerntState].initialized = false;
                Manager.Scene.Clear();
                Manager.UnloadContent();
            }

            // Guardamos el estado establecido:
            this.CurrerntState = state;
            
            // Inicializamos el estado:
            this.States[state].Initialize();
            this.States[state].initialized = true;
        }

        /// <summary>
        /// Actualiza la logica del estado actual.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (this.CurrerntState != "" && this.States[this.CurrerntState].initialized) this.States[this.CurrerntState].Update(gameTime);
        }

        /// <summary>
        /// Dibuja los objetos del estado actual.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            if (this.States[this.CurrerntState].initialized) this.States[this.CurrerntState].Draw(gameTime);
        }

        /// <summary>
        /// Destruye la instancia de la maquina de estados y libera todos los recursos.
        /// </summary>
        public void Dispose()
        {
            foreach (StateComponent state in this.States.Values)
                state.Terminate();
        }
    }
}