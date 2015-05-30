using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TLSA.Engine.Graphics
{
    /// <summary>
    /// Render Target.
    /// </summary>
    /// <remarks>Crea un objeto grafico sobre el que dibujar las operaciones graficas y guardar en una textura.</remarks>
    public class RenderTarget
    {
        internal RenderTarget2D renderTarget;

        /// <summary>
        /// Indica si el Render Target esta abierto.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Devuelve el tamaño que tiene el Render Target.
        /// </summary>
        public Vector2 Size { get; private set; }

        /// <summary>
        /// Accede al contenido del RenderTarget para utilizarse como textura en un Sprite.
        /// </summary>
        /// <returns>Devuelve la instancia del RenderTarget2D como Textura.</returns>
        /// <remarks>Esta propiedad no preserva el contenido de la textura en caso de que se pierda el dispositivo grafico.
        /// Para crear una copia permanente del contenido de la textura usar la funcion GetTexture().</remarks>
        public Texture2D Texture { get { return this.renderTarget; } }

        #region Constructores
        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <remarks>Inicializa el render target con el tamaño del buffer de pantalla.</remarks>
        public RenderTarget() 
            : this(Manager.Graphics.CurrentDisplayMode) 
        { 
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="Width">Ancho en pixeles del render target.</param>
        /// <param name="Height">Alto en pixeles del render target.</param>
        public RenderTarget(int Width, int Height)
        {
            this.renderTarget = new RenderTarget2D(Manager.GameInstance.GraphicsDevice, Width, Height);
            this.Size = new Vector2(this.renderTarget.Width, this.renderTarget.Height);
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="Size">Tamaño en pixeles del render target.</param>
        public RenderTarget(Vector2 Size) 
            : this((int)Size.X, (int)Size.Y) 
        { 
        }

        /// <summary>
        /// Establece el fondo del RenderTarget a transparente.
        /// </summary>
        private void SetAlphaBackground()
        {
            Color[] buffer = new Color[this.renderTarget.Width * this.renderTarget.Height];
            for (int i = 0; i < buffer.Length; i++) buffer[i] = Color.Transparent;
            this.renderTarget.SetData<Color>(buffer);
        }
        #endregion

        /// <summary>
        /// Abre el render target para dibujar en el.
        /// </summary>
        public void Begin()
        {
            Manager.Graphics.End();
            Manager.GameInstance.GraphicsDevice.SetRenderTarget(this.renderTarget);
            Manager.Graphics.Begin();
            this.IsOpen = true;
        }

        /// <summary>
        /// Cierra el render target.
        /// </summary>
        public void End()
        {
            Manager.Graphics.End();
            Manager.GameInstance.GraphicsDevice.SetRenderTarget(null);
            Manager.Graphics.Begin();
            this.IsOpen = false;
        }

        /// <summary>
        /// Vuelca el contenido del RenderTarget en una nueva textura.
        /// </summary>
        /// <returns>Devuelve la instancia de la textura.</returns>
        public Texture2D SaveToTexture()
        {
            Texture2D tex = new Texture2D(Manager.GameInstance.GraphicsDevice, this.renderTarget.Width, this.renderTarget.Height);
            Color[] buffer = new Color[this.renderTarget.Width * this.renderTarget.Height];
            this.renderTarget.GetData<Color>(buffer);
            tex.SetData<Color>(buffer);
            return tex;
        }
    }
}
