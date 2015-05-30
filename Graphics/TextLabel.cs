using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TLSA.Engine.Graphics
{
    /// <summary>
    /// Representa un objeto que contiene una cadena de texto que se puede dibujar y aplicar transformaciones igual que con un Sprite.
    /// </summary>
    public class TextLabel : TLSA.Engine.IDrawable
    {
        private SpriteFont font;

        private Vector2 location;
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
        public bool Fixed { get; set; }
        private string fontName = "";
        public string FontName 
        {
            get { return this.fontName; }
            set
            {
                this.fontName = value;
                this.font = Manager.Graphics.Fonts[value];
            }
        }
        private string text = "";
        public string Text 
        {
            get { return this.text; }
            set 
            {
                this.text = value;
                this.size = this.font.MeasureString(this.Text);
            }
        }
        public Color Color { get; set; }
        public float Scale { get; set; }
        public float Angle { get; set; }
        public Vector2 OffSet { get; set; }
        public bool Center { get; set; }
        public SpriteEffects Mirror { get; set; }
        public BlendFilters BlendingEffect { get; set; }
        public bool Visible { get; set; }
        public int ZOrder { get; set; }

        private Vector2 size;
        

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        public TextLabel()
        {
            this.Location = new Vector2(); this.Color = Color.White; this.Scale = 1; this.Angle = 0f; this.OffSet = Vector2.Zero; this.Mirror = SpriteEffects.None;
            this.Visible = true;
            this.BlendingEffect = BlendFilters.TextSmothRendering;
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="AssetName">Nombre del recurso que representa la fuente de texto.</param>
        public TextLabel(string AssetName)
            : this()
        {
            this.FontName = AssetName;            
        }

        /// <summary>
        /// Dibuja la cadena de texto aplicando los parametros establecidos.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            if (this.Center) this.OffSet = this.size / 2;
            if (this.Visible)
            {
                Manager.Graphics.SetBlendFilter(this.BlendingEffect);
                Manager.Graphics.SpriteBatch.DrawString(this.font, this.Text, this.Location, this.Color, MathHelper.ToRadians(this.Angle), this.OffSet, this.Scale, this.Mirror, 0);
            }
        }

        /// <summary>
        /// Devuelve el tamaño que tendra el area de la cadena de texto al dibujarse.
        /// </summary>
        public Vector2 Size
        {
            get { return this.size; }
        }

        /// <summary>
        /// Devuelve el area que ocupara el texto al dibujarse:
        /// </summary>
        public Rectangle Bounds 
        { 
            get 
            {
                Rectangle bounds = new Rectangle((int)this.Location.X, (int)this.Location.Y, (int)this.Size.X, (int)this.Size.Y);
                if (this.Center)
                {
                    bounds.X -= (int)(this.size.X / 2); bounds.Y -= (int)(this.size.Y / 2);
                }
                return bounds;
            } 
        }
    }
}
