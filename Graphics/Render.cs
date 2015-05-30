using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MSSystem = System;

namespace TLSA.Engine.Graphics
{
    /// <summary>
    /// Modos de video predefinidos:
    /// </summary>
    public enum PrefDisplayModes { _480p, _720p, _1080p }

    /// <summary>
    /// Objeto que simplica la gestion del render para operaciones 2D.
    /// </summary>
    public class Render
    {
        #region Miembros internos
        internal SpriteBatch SpriteBatch;
        internal Texture2D DummyTexture;
        internal BlendFilters currentBlendFilter;
        
        internal Effect shader;
        internal string shaderAsset = "";

        private RenderTarget2D renderTarget;

        private Color[] screenBuffer;
        private Texture2D screenTexture;
        private bool initializeMembers = false;
        #endregion

        #region Propiedades
        /// <summary>
        /// Color que se utilizara para borrar la escena.
        /// </summary>
        public Color ClearColor { get; set; }

        /// <summary>
        /// Lista de texturas cargadas en memoria.
        /// </summary>
        public Dictionary<string, Texture2D> Textures { get; internal set; }

        /// <summary>
        /// Lista de fuentes de texto cargadas en memoria.
        /// </summary>
        public Dictionary<string, SpriteFont> Fonts { get; internal set; }

        /// <summary>
        /// Dimensiones de la escena.
        /// </summary>
        public Rectangle ScreenBounds { get; internal set; }

        /// <summary>
        /// Dimensiones del area segura de la pantalla.
        /// </summary>
        /// <remarks>En XBox360, las televisiones no muestran completamente el area de resolucion. 
        /// Esta estructura devuelve el area segura definida por la propia television que este conectada como salida.
        /// Utilice esta area para ubicar de forma segura elementos criticos como marcadores, el jugador, el avatar o el HUD.</remarks>
        public Rectangle ScreenSafeArea
        {
            get { return Manager.GraphicsDevice.Viewport.TitleSafeArea; }
        }

        /// <summary>
        /// Indica si Windows muestra el cursor del raton.
        /// </summary>
        public bool IsMouseVisible
        {
            get { return Manager.GameInstance.IsMouseVisible; }
            set { Manager.GameInstance.IsMouseVisible = value; }
        }

        /// <summary>
        /// Devuelve el modo de video actual.
        /// </summary>
        public Vector2 CurrentDisplayMode
        {
            get { return new Vector2(Manager.GraphicsDeviceManager.PreferredBackBufferWidth, Manager.GraphicsDeviceManager.PreferredBackBufferHeight); }
        }

        /// <summary>
        /// Indica si se realiza la espera de sincronizacion vertical.
        /// </summary>
        public bool SynchronizeWithVerticalRetrace
        {
            get { return Manager.GraphicsDeviceManager.SynchronizeWithVerticalRetrace; }
            set { Manager.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = value; }
        }

        /// <summary>
        /// Coordenada de desplazamiento de la escena.
        /// </summary>
        /// <remarks>Esta coordenada afecta a cualquier objeto grafico no fijo sumando su posicion al offset.</remarks>
        public Vector2 OffSet { get; set; }

        /// <summary>
        /// Indica si el shader, si se aplico alguno, esta activo.
        /// </summary>
        public bool IsShaderActive { get; internal set; }
        #endregion

        #region Metodos y funciones publicas
        /// <summary>
        /// Inicializa la clase.
        /// </summary>
        /// <remarks>Se inicializa desde la clase Manager.</remarks>
        internal Render()
        {
            // Creamos una textura base de 1x1 de color blanco que sera utilizada por las operaciones de dibujo de primitivas:
            DummyTexture = new Texture2D(Manager.GameInstance.GraphicsDevice, 1, 1);
            DummyTexture.SetData(new Color[] { Color.White });

            ClearColor = Color.CornflowerBlue;
            SpriteBatch = new SpriteBatch(Manager.GraphicsDevice);

            Textures = new Dictionary<string, Texture2D>();
            Fonts = new Dictionary<string, SpriteFont>();

            OffSet = Vector2.Zero;

            BlendFactory.Initialize();
            currentBlendFilter = BlendFilters.AlphaBlending;            

            IsShaderActive = false;
        }

        /// <summary>
        /// Carga una textura en el ContentManager y la agrega a la lista.
        /// </summary>
        /// <param name="assetName">Nombre del recurso de la textura en el Content Project.</param>
        /// <returns>Devuelve la instancia de la textura cargada.</returns>
        /// <remarks>Si el recurso que intenta cargar ya existe se omitira su carga y se devolvera la instancia del recurso ya cargado en memoria.</remarks>
        public Texture2D LoadTexture(string assetName)
        {
            if (!Textures.Keys.Contains(assetName))
            {
                Texture2D tex = Manager.Content.Load<Texture2D>(assetName);
                Textures.Add(assetName, tex);
                return tex;
            }
            else
                return Textures[assetName];
        }

        /// <summary>
        /// Carga una fuente de texto en el ContentManager y la agrega a la lista.
        /// </summary>
        /// <param name="assetName">Nombre del recurso de la fuente de texto en el Content Project.</param>
        /// <returns>Devuelve la instancia de la fuente cargada.</returns>
        /// <remarks>Si el recurso que intenta cargar ya existe se omitira su carga y se devolvera la instancia del recurso ya cargado en memoria.</remarks>
        public SpriteFont LoadFont(string assetName)
        {
            if (!Fonts.Keys.Contains(assetName))
            {
                SpriteFont font = Manager.Content.Load<SpriteFont>(assetName);
                Fonts.Add(assetName, font);
                return font;
            }
            else
                return Fonts[assetName];
        }

        /// <summary>
        /// Cambia el modo de video actual.
        /// </summary>
        /// <param name="Width">Ancho.</param>
        /// <param name="Height">Alto.</param>
        /// <param name="VSync">Indica si se aplica espera de refresco vertical.</param>
        public void SetDisplayMode(int Width, int Height)
        {
            Manager.GraphicsDeviceManager.PreferredBackBufferWidth = Width;
            Manager.GraphicsDeviceManager.PreferredBackBufferHeight = Height;
            Manager.GraphicsDeviceManager.ApplyChanges();
            
            ScreenBounds = new Rectangle(0, 0, Width, Height);

            renderTarget = new RenderTarget2D(Manager.GraphicsDevice, Width, Height);            
        }

        /// <summary>
        /// Cambia el modo de video actual.
        /// </summary>
        /// <param name="mode">Perfil predefinido de resolucion.</param>
        public void SetDisplayMode(PrefDisplayModes mode)
        {
            switch (mode)
            {
                case PrefDisplayModes._720p: SetDisplayMode(1280, 720); break;
                case PrefDisplayModes._1080p: SetDisplayMode(1920, 1080); break;
                default: SetDisplayMode(640, 480); break;
            }
        }

        /// <summary>
        /// Alterna entre modo a pantalla completa y modo ventana.
        /// </summary>
        public void ToggleFullScreen()
        {
            Manager.GraphicsDeviceManager.ToggleFullScreen();
        }

        /// <summary>
        /// Carga un shader en memoria.
        /// </summary>
        /// <param name="assetName">Nombre del shader en el content Manager.</param>
        public void LoadShader(string assetName)
        {
            shader = Manager.Content.Load<Effect>(assetName);
            shaderAsset = assetName;
        }

        /// <summary>
        /// Activa el shader cargado en memoria.
        /// </summary>
        public void BeginShader()
        {
            IsShaderActive = true && shader != null;
        }

        /// <summary>
        /// Desactiva el shader.
        /// </summary>
        public void EndShader()
        {
            IsShaderActive = false;
        }

        internal void SetBlendFilter(BlendFilters filter)
        {
            if (filter != this.currentBlendFilter)
            {
                SpriteBatch.End();

                SpriteBatch.Begin(0, BlendFactory.GetBlendState(filter));
                currentBlendFilter = filter;
            }
        }

        /// <summary>
        /// Inicia el render para dibujar.
        /// </summary>
        public void Begin()
        {                        
            Manager.GameInstance.GraphicsDevice.Clear(ClearColor);
            Manager.GraphicsDevice.SetRenderTarget(renderTarget);
            SpriteBatch.Begin(0, BlendFactory.GetBlendState(currentBlendFilter));
            Manager.Graphics.ClearScreen();
        }

        /// <summary>
        /// Termina el render para volcar la informacion en pantalla.
        /// </summary>
        public void End()
        {
            SpriteBatch.End();
            Manager.GraphicsDevice.SetRenderTarget(null);
            
            // Si el shader esta activado...
            if (IsShaderActive)
            {
                // Si no estuviera cargado el shader se vuelve a cargar:
                if (shader == null || shader.IsDisposed) shader = Manager.Content.Load<Effect>(shaderAsset);

                // Dibujamos la textura con el shader activado:                
                SpriteBatch.Begin(0, BlendState.Opaque, null, null, null, shader);
                Manager.Graphics.ClearScreen();
                SpriteBatch.Draw(renderTarget, this.ScreenBounds, Color.White);
                SpriteBatch.End();
            }
            else
            {
                SpriteBatch.Begin();
                Manager.Graphics.ClearScreen();
                SpriteBatch.Draw(renderTarget, this.ScreenBounds, Color.White);
                SpriteBatch.End();
            }
        }

        /// <summary>
        /// Borra el contenido de la pantalla.
        /// </summary>
        public void ClearScreen()
        {
            this.ClearScreen(this.ClearColor);
        }

        /// <summary>
        /// Borra el contenido de la pantalla.
        /// </summary>
        /// <param name="color">Color de borrado.</param>
        public void ClearScreen(Color color)
        {
            Manager.Graphics.SpriteBatch.Draw(this.DummyTexture, this.ScreenBounds, color);
        }
        #endregion
    }
}