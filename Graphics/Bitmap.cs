using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TLSA.Engine.Graphics
{
    public class Bitmap : TLSA.Engine.IDrawable 
    {
        private Texture2D texture;

        // Campos publicos:
        private string textureName;
        public string TextureName
        {
            get { return this.textureName; }
            set
            {
                this.textureName = value;
                // Si el valor no es cadena vacia se establece la nueva textura:
                if (value != "") 
                { 
                    this.texture = Manager.Graphics.Textures[this.textureName]; 
                    this.Frame = this.texture.Bounds; 
                    this.Size = new Vector2(this.texture.Bounds.Width, this.texture.Bounds.Height);
                } 
                else 
                    this.texture = null;
            }
        }
        [System.Xml.Serialization.XmlIgnore]
        public Texture2D Texture
        {
            get { return this.texture; }
            set
            {
                this.texture = value;
                this.Frame = this.texture.Bounds;
                this.Size = new Vector2(this.texture.Bounds.Width, this.texture.Bounds.Height);
                this.textureName = "";
            }
        }
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

        public Color Color { get; set; }
        public SpriteEffects Mirror { get; set; }
        public BlendFilters BlendingEffect { get; set; }
        public Rectangle? Frame { get; set; }
        public Vector2 Size { get; set; }
        public bool Visible { get; set; }
        

        public Bitmap()
        {
            this.TextureName = "";
            this.Location = new Vector2(); this.Color = Color.White; this.Mirror = SpriteEffects.None; this.Size = Vector2.Zero; this.Frame = null;
            this.Visible = true;
            this.Fixed = false;
            this.BlendingEffect = BlendFilters.AlphaBlending;
        }

        public Bitmap(string AssetName)
            : this()
        {
            this.TextureName = AssetName;
        }

        public void Draw(GameTime gameTime)
        {
            if (texture == null)
            {
                texture = Manager.Graphics.Textures[this.TextureName];
                if (this.Size == Vector2.Zero ) { this.Size = new Vector2(this.texture.Width, this.texture.Height); }
            }
            if (this.Visible)
            {
                Manager.Graphics.SetBlendFilter(this.BlendingEffect);
                Manager.Graphics.SpriteBatch.Draw(texture, new Rectangle((int)this.Location.X, (int)this.Location.Y, (int)Size.X, (int)Size.Y), Frame, Color, 0f, Vector2.Zero, this.Mirror, 0);
            }
        }
    }
}
