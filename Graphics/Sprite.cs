using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TLSA.Engine.Graphics
{
    /// <summary>
    /// Representa un objeto grafico que dibuja una textura o parte de ella aplicandole efectos y con capacidad para reproducir secuencias de animaciones.
    /// </summary>
    public class Sprite : TLSA.Engine.IDrawableAndUpdateable
    {
        private Texture2D texture;
        private Rectangle frame;
        private Vector2 drawOffset;

        // Campos publicos:
        private string textureName;
        /// <summary>
        /// Nombre del asset de la textura asociada al sprite.
        /// </summary>
        public string TextureName 
        {
            get { return this.textureName; }
            set
            {
                this.textureName = value;
                // Si el valor no es cadena vacia se establece la nueva textura:
                if (value != "") { this.texture = Manager.Graphics.Textures[this.textureName]; this.frame = texture.Bounds; } else this.texture = null;
            }
        }

        /// <summary>
        /// Textura asociada al sprite.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Texture2D Texture 
        { 
            get { return this.texture; } 
            set 
            { 
                this.texture = value;
                this.frame = texture.Bounds;
                this.textureName = ""; 
            } 
        }

        /// <summary>
        /// Lista de animaciones.
        /// </summary>
        public AnimationManager Animations { get; set; }

        private Vector2 location;
        /// <summary>
        /// Ubicacion del sprite en pantalla.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Vector2 Location
        {
            get
            {
                if (this.Fixed)
                    return this.location;
                else
                    return this.location + Manager.Graphics.OffSet;
            }
            set { this.location = value; }
        }

        /// <summary>
        /// Determina si se aplica el Offset o coordenada de desplazamiento de escena al objeto.
        /// </summary>
        public bool Fixed { get; set; }

        /// <summary>
        /// Color de tintado del sprite.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Color Color { get; set; }

        /// <summary>
        /// Scala del sprite.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public float Scale { get; set; }

        /// <summary>
        /// Angulo de rotacion del sprite.
        /// </summary>
        /// <remarks>Los angulos se definen en grados.</remarks>
        [System.Xml.Serialization.XmlIgnore]
        public float Angle { get; set; }

        /// <summary>
        /// Centro u origen del sprite.
        /// </summary>
        /// <remarks>Define el origen del sprite para la referencia a la posicion y la ubicacion del eje de rotacion.</remarks>
        [System.Xml.Serialization.XmlIgnore]
        public Vector2 OffSet { get; set; }

        /// <summary>
        /// Indica si el sprite estara centrado en su coordenada.
        /// </summary>
        /// <remarks>Fuerza a que la coordenada OffSet sea siempre el centro del sprite.</remarks>
        [System.Xml.Serialization.XmlIgnore]
        public bool Center { get; set; }

        /// <summary>
        /// Espejado del sprite.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public SpriteEffects Mirror { get; set; }

        /// <summary>
        /// Indica si el sprite esta visible.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool Visible { get; set; }

        /// <summary>
        /// Indica si el sprite esta activo.
        /// </summary>
        /// <remarks>La logica de actualizacion de sprite se aplica para gestionar las animaciones.</remarks>
        [System.Xml.Serialization.XmlIgnore]
        public bool Enabled { get; set; }

        /// <summary>
        /// Indica el filtro de mezclado utilizado en la operacion de dibujado.
        /// </summary>
        /// <remarks>Por defecto el filtro es AlphaBlending.</remarks>
        [System.Xml.Serialization.XmlIgnore]
        public BlendFilters BlendingEffect { get; set; }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        public Sprite()
        {
            this.TextureName = "";
            this.Animations = new AnimationManager();
            this.Location = new Vector2(); this.Color = Color.White; this.Scale = 1; this.Angle = 0f; this.OffSet = Vector2.Zero; this.Mirror = SpriteEffects.None;
            this.Visible = true; this.Enabled = true;
            this.BlendingEffect = BlendFilters.AlphaBlending;
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="AssetName">Nombre del recurso que representa la textura.</param>
        public Sprite(string AssetName)
            : this()
        {
            this.TextureName = AssetName;
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="Texture">Referencia directa a</param>
        public Sprite(Texture2D Texture)
            : this()
        {
            this.texture = Texture;
            this.frame = this.texture.Bounds;
        }

        /// <summary>
        /// Actualiza el estado del sprite y sus animaciones si las tuviera.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            if (this.Enabled) if (this.Animations.Secuences.Count > 0) { this.Animations.Update(gameTime); this.frame = this.Animations.Frame; } else this.frame = this.texture.Bounds;
        }
                
        /// <summary>
        /// Dibuja el sprite con los parametros establecidos.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            if (this.Center) drawOffset = new Vector2(this.frame.Width / 2, this.frame.Height / 2); else drawOffset = this.OffSet;
            if (this.Visible && this.texture != null)
            {
                Manager.Graphics.SetBlendFilter(this.BlendingEffect);
                Manager.Graphics.SpriteBatch.Draw(this.texture, this.Location, this.frame, this.Color, MathHelper.ToRadians(this.Angle), drawOffset, this.Scale, this.Mirror, 0f);
            }
        }

        /// <summary>
        /// Dibuja el sprite con los parametros establecidos.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="frame">Permite definir la region a dibujar obviando el valor establecido por el gestor de animaciones.</param>
        public void Draw(GameTime gameTime, Rectangle frame)
        {
            this.frame = frame; this.Draw(gameTime);
        }
    }
}