using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// XNA:
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// TLSA.Engine:
using TLSA.Engine;
using TLSA.Engine.Scene;
using TLSA.Engine.Graphics;
using TLSA.Engine.Graphics.Primitives;
using TLSA.Engine.Input;
using TLSA.Engine.Physics.V1Engine;
using TLSA.Engine.Tools;
using TLSA.Engine.Tools.IO;
using TLSA.Engine.Tools.XML;

namespace TLSA.Engine
{
    /// <summary>
    /// Punto de acceso global del motor.
    /// </summary>
    /// <remarks>Desde aqui se accede a la gestion de la mayor parte del motor.</remarks>
    public static class Manager
    {
        private static FPSCounter fps;

        /// <summary>
        /// Instancia de la clase principal del juego en XNA.
        /// </summary>
        /// /// <remarks>Esta propiedad se implementa para la comunicacion de los modulos del motor con XNA.</remarks>
        public static Game GameInstance { get; internal set; }
        /// <summary>
        /// Instancia del dispositivo grafico de XNA.
        /// </summary>
        /// /// <remarks>Esta propiedad se implementa para la comunicacion de los modulos del motor con XNA.</remarks>
        public static GraphicsDevice GraphicsDevice { get { return GameInstance.GraphicsDevice; } }
        /// <summary>
        /// Instancia del administrador de contenidos de XNA:
        /// </summary>
        /// /// <remarks>Esta propiedad se implementa para la comunicacion de los modulos del motor con XNA.</remarks>
        public static ContentManager Content { get { return GameInstance.Content; } }
        /// <summary>
        /// Instancia del administrador del dispositivo grafico de XNA.
        /// </summary>
        /// <remarks>Esta propiedad se implementa para la comunicacion de los modulos del motor con XNA.</remarks>
        public static GraphicsDeviceManager GraphicsDeviceManager { get; internal set; }

        /// <summary>
        /// Acceso al motor grafico.
        /// </summary>
        public static Render Graphics { get; internal set; }

        /// <summary>
        /// Instancia principal del Stage.
        /// </summary>
        /// <remarks>Utilice este Stage instanciado como gestor principal de la escena de su juego.</remarks>
        public static Stage Scene { get; internal set; }

        /// <summary>
        /// Instancia principal de la maquina de estados.
        /// </summary>
        /// <remarks>Utilice esta maquina de estados instanciado como gestor principal de estados de su juego.</remarks>
        public static StateMachine GameStates { get; internal set; }

        /// <summary>
        /// Instancia principal del mapa de input.
        /// </summary>
        /// <remarks>Esta instancia esta pensada para ser usada en la interaccion de menus y demas interfaces de usuario.</remarks>
        public static InputMap UIInput { get; set; }

        /// <summary>
        /// Instancia principal de la cola de mensaje.
        /// </summary>
        /// <remarks>Esta instancia esta pensada para ser usada con las entidades de la instancia principal del Stage.</remarks>
        public static MessageQueue Messages { get; internal set; }

        /// <summary>
        /// Instancia generica del motor de fisicas.
        /// </summary>
        /// <remarks>Esta instancia ofrece un simulador de fisicas preparado para el uso en la instancia principal de Stage. 
        /// Esta basado en el V1Engine.</remarks>
        public static World PhysicEngine { get; internal set; }

        /// <summary>
        /// Lista global de variables.
        /// </summary>
        /// <remarks>Utilice esta lista para definir variables que pueden usarse desde cualquier entidad o seccion del juego.</remarks>
        public static VarList Vars { get; internal set; }

        /// <summary>
        /// Devuelve el numero de fotogramas por segundo que ha tardado el completarse el ciclo de la escena en actualizarse y dibujarse.
        /// </summary>
        public static int FPS { get { return fps.GetFPS(); } }

        /// <summary>
        /// Permite acceder a los perfiles de usuario que hayan iniciado sesion y gestionar varias funcionalidades del componente de servicios de jugador de XNA.
        /// </summary>
        public static TLSA.Engine.Tools.GamerServices.Sessions GamerServices { get; internal set;  }

        /// <summary>
        /// Instancia global de la variable de tiempo de juego de XNA:
        /// </summary>
        public static GameTime GameTime { get; internal set; }

        /// <summary>
        /// Inicializa TLSA.Engine.
        /// </summary>
        /// <param name="gameInstance">Instancia de la clase principal del juego en XNA.</param>
        /// <param name="graphicsInstance">Instancia del administrador del dispositivo grafico de XNA.</param>
        public static void Initialize(Game gameInstance, GraphicsDeviceManager graphicsInstance)
        {
            fps = new FPSCounter();

            GameInstance = gameInstance;
            GamerServices = new Tools.GamerServices.Sessions();
            GraphicsDeviceManager = graphicsInstance;
            Graphics = new Render();
            Scene = new Stage();
            GameStates = new StateMachine();
            UIInput = new InputMap(PlayerIndex.One, InputType.KeyboardMouse);
            Messages = new MessageQueue(Scene);
            PhysicEngine = new World(Vector2.Zero, Graphics.ScreenBounds);
            Vars = new VarList();
            GameTime = new GameTime();
        }

        /// <summary>
        /// Termina TLSA.Engine.
        /// </summary>
        /// <remarks>Libera todos las instancias del motor y termina la ejecucion del juego.</remarks>
        public static void Terminate()
        {
            Scene.Dispose();
            GameStates.Dispose();
            PhysicEngine.Dispose();
            GameInstance.Exit();            
            UnloadContent();
        }

        /// <summary>
        /// Descarga todos los recursos cargados en el content por el motor:
        /// </summary>
        public static void UnloadContent()
        {
            Content.Unload();
            Graphics.Textures.Clear();
            Graphics.Fonts.Clear();
        }

        /// <summary>
        /// Actualiza los estados del motor.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <remarks>Actualiza los estados del mapa de input de la interfaz de usuario, el estado actual del juego y el contenido del grafo de escena.</remarks>
        public static void Update(GameTime gameTime)
        {
            GameTime = gameTime;

            UIInput.Update(gameTime);       // Actualizamos el input.
            GameStates.Update(gameTime);    // Actualizamos los estados.
            Messages.Process();             // Procesamos la cola de mensajes.
            PhysicEngine.Update(gameTime);  // Actualizamos las fisicas, si estan activas.
            Scene.Update(gameTime);         // Actualizamos las entidades de la escena.
        }

        /// <summary>
        /// Dibuja la escena actual.
        /// </summary>
        /// <param name="gameTime">Gestiona el dibujado del render grafico, el estado actual del juego y el contenido del grafo de escena.</param>
        public static void Draw(GameTime gameTime)
        {
            GameTime = gameTime;

            Graphics.Begin();
            {
                GameStates.Draw(gameTime);
                Scene.Draw(gameTime);
                PhysicEngine.Draw(gameTime);
            }
            Graphics.End();
            fps.Update(gameTime);           // Actualizamos la cuenta de fotogramas por segundo al finalizar las operaciones de dibujado.
        }
    }
}