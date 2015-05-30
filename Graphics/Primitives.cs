using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TLSA.Engine.Graphics;
using XNAMathHelper = Microsoft.Xna.Framework.MathHelper;
using TLSAMathHelper = TLSA.Engine.Tools.MathTools;

namespace TLSA.Engine.Graphics.Primitives
{
    /// <summary>
    /// Dibuja una linea.
    /// </summary>
    public class LineOverlay : TLSA.Engine.IDrawable 
    {
        private float angle;
        private Rectangle rectLine;
        private int borderWidth;
        private Vector2 a, b;

        public bool Visible { get; set; }
        public int ZOrder { get; set; }

        public Vector2 A
        {
            get { if (this.Fixed) return this.a; else return this.a + Manager.Graphics.OffSet; }
            set { this.a = value; this.angle = XNAMathHelper.ToRadians(TLSAMathHelper.GetAngle(this.a, this.b)); UpdateRectangle(); }
        }
        public Vector2 B
        {
            get { if (this.Fixed) return this.b; else return this.b + Manager.Graphics.OffSet; }
            set { this.b = value; this.angle = XNAMathHelper.ToRadians(TLSAMathHelper.GetAngle(this.a, this.b)); UpdateRectangle(); }
        }

        public bool Fixed { get; set; }
        public BlendFilters BlendingEffect { get; set; }

        private void UpdateRectangle()
        {
            this.rectLine = new Rectangle((int)a.X - this.borderWidth / 2, (int)a.Y - this.borderWidth / 2, (int)Vector2.Distance(a, b), this.BorderWidth);
        }
        public Color Color { get; set; }
        public int BorderWidth
        {
            get { return this.borderWidth; }
            set { this.borderWidth = value; if (this.borderWidth < 1) this.borderWidth = 1; UpdateRectangle(); }
        }        

        #region Constructores de la clase
        public LineOverlay()
        {
            this.A = Vector2.Zero;
            this.B = Vector2.Zero;
            this.Color = Color.White;
            this.BorderWidth = 1;
            this.Fixed = false;
            this.BlendingEffect = BlendFilters.AlphaBlending;
            this.Visible = true;
        }

        public LineOverlay(Vector2 A, Vector2 B)
            : this()
        {
            this.A = A; this.B = B;
        }

        public LineOverlay(Vector2 A, Vector2 B, Color Color)
            : this(A, B)
        {
            this.Color = Color;
        }

        public LineOverlay(Vector2 A, Vector2 B, Color Color, int BorderWidth)
            : this(A, B, Color)
        {
            this.BorderWidth = BorderWidth;
        }
        #endregion

        public void Draw(GameTime gameTime)
        {
            if (this.Visible)
            {
                Manager.Graphics.SetBlendFilter(this.BlendingEffect);

                // Dibuja el sprite con la distancia en pixeles y el angulo entre los dos puntos y con la anchura definida para representar la linea:
                Manager.Graphics.SpriteBatch.Draw(Manager.Graphics.DummyTexture, this.rectLine, null, Color, this.angle, Vector2.Zero, SpriteEffects.None, 0); 
            }
        }
    }

    /// <summary>
    /// Dibuja un rectangulo con borde.
    /// </summary>
    public class RectangleOverlay : TLSA.Engine.IDrawable 
    {
        private LineOverlay left, top, right, bottom;

        public bool Visible { get; set; }

        public Color BackColor { get; set; }

        public Color ForeColor
        {
            get { return this.left.Color; }
            set
            {
                this.left.Color = value;
                this.top.Color = value;
                this.right.Color = value;
                this.bottom.Color = value;
            }
        }

        private int borderWith;
        public int BorderWidth
        {
            get { return this.borderWith; }
            set { this.borderWith = value; this.UpdateRectangle(); }
        }

        private Vector2 location;
        public Vector2 Location
        {
            get { if (this.Fixed) return this.location; else return this.location + Manager.Graphics.OffSet; }
            set { this.location = value; this.UpdateRectangle(); }
        }

        public bool Fixed 
        {
            get { return left != null ? left.Fixed : false; }
            set
            {
                if (left != null)
                {
                    left.Fixed = value;
                    top.Fixed = value;
                    right.Fixed = value;
                    bottom.Fixed = value;
                }
            }
        }
        public BlendFilters BlendingEffect { get; set; }

        private Vector2 size;
        public Vector2 Size
        {
            get { return this.size; }
            set { this.size = value; this.UpdateRectangle(); }
        }

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)this.Location.X, (int)this.Location.Y, (int)this.size.X, (int)this.size.Y);
            }
            set
            {
                this.location = new Vector2(value.X, value.Y);
                this.size = new Vector2(value.Width, value.Height);
                this.UpdateRectangle();
            }
        }

        private Vector2 backRect_offSet = Vector2.Zero;
        private Vector2 foreRect_offSet = Vector2.Zero;
        private Rectangle backRect;

        public RectangleOverlay()
        {
            this.left = new LineOverlay();
            this.top = new LineOverlay();
            this.right = new LineOverlay();
            this.bottom = new LineOverlay();

            this.Location = new Vector2();
            this.Size = new Vector2();
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.BorderWidth = 1;
           
            this.Visible = true;

            this.BlendingEffect = BlendFilters.AlphaBlending;
        }

        public RectangleOverlay(Vector2 Location, Vector2 Size)
            : this()
        {
            this.Location = Location; this.Size = Size;
        }

        public RectangleOverlay(Rectangle Rect)
            : this(new Vector2(Rect.X, Rect.Y), new Vector2(Rect.Width, Rect.Height))
        {
        }

        public void UpdateRectangle()
        {            
            // Rectangulo de fondo:
            this.backRect = new Rectangle((int)this.Location.X + this.borderWith, (int)this.Location.Y + this.borderWith, (int)this.Size.X - this.borderWith, (int)this.Size.Y - this.borderWith);

            // Rectangulo que representa el borde:
            this.left.A = new Vector2(this.backRect.Left, this.backRect.Top);
            this.left.B = new Vector2(this.backRect.Left, this.backRect.Bottom);
            this.left.BorderWidth = this.borderWith;

            this.top.A = new Vector2(this.backRect.Left, this.backRect.Top);
            this.top.B = new Vector2(this.backRect.Right, this.backRect.Top);
            this.top.BorderWidth = this.borderWith;

            this.right.A = new Vector2(this.backRect.Right, this.backRect.Top);
            this.right.B = new Vector2(this.backRect.Right, this.backRect.Bottom);
            this.right.BorderWidth = this.borderWith;

            this.bottom.A = new Vector2(this.backRect.Left, this.backRect.Bottom);
            this.bottom.B = new Vector2(this.backRect.Right, this.backRect.Bottom);
            this.bottom.BorderWidth = this.borderWith;
        }

        public void Draw(GameTime gameTime)
        {
            if (this.Visible)
            {
                Manager.Graphics.SetBlendFilter(this.BlendingEffect);

                // Dibujamos el rectangulo de fondo:
                if (this.BackColor != Color.Transparent) Manager.Graphics.SpriteBatch.Draw(Manager.Graphics.DummyTexture, this.backRect, null, this.BackColor, 0, Vector2.Zero, SpriteEffects.None, 0);

                // Dibujamos los bordes:
                this.left.Draw(gameTime); this.top.Draw(gameTime); this.right.Draw(gameTime); this.bottom.Draw(gameTime); 
            }
        }
    }
}