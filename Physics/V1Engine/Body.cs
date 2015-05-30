using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endif

using TLSA.Engine.Graphics;
using TLSA.Engine.Graphics.Primitives;
using TLSA.Engine.Input;
using TLSA.Engine.Physics.V1Engine;
using TLSA.Engine.Tools;
using TLSA.Engine.Tools.IO;
using TLSA.Engine.Tools.XML;

namespace TLSA.Engine.Physics.V1Engine
{
    public delegate void CollisionHandler(Body b, Vector2 force, float direction);
    public delegate void TriggerCollisionHandler(Body b);
    public delegate void HitHandler(object source, float force, float direction);

    public class Body : TLSA.Engine.IDrawable
    {
        public Guid Id { get; internal set; }

        private RectangleOverlay box;

        public World World { get; set; }
        public bool Enabled { get; set; }
        public bool Visible { get { return this.World.Visible; } set { } }

        public Rectangle Bounds { get; set; }

        internal Vector2 LT { get { return new Vector2(Bounds.Left, Bounds.Top); } }
        internal Vector2 LB { get { return new Vector2(Bounds.Left, Bounds.Bottom); } }
        internal Vector2 RT { get { return new Vector2(Bounds.Right, Bounds.Top); } }
        internal Vector2 RB { get { return new Vector2(Bounds.Right, Bounds.Bottom); } }

        internal Vector2 lastLocation;
        public Vector2 Location
        {
            get { return new Vector2(Bounds.Center.X, Bounds.Center.Y); }
            set
            {
                this.lastLocation = this.Location;                
                Rectangle r = this.Bounds;
                r.X = (int)value.X - (this.Bounds.Width / 2); r.Y = (int)value.Y - (this.Bounds.Height / 2);
                this.Bounds = r;
            }
        }
        
        public float Weight { get; set; }
        public bool Fixed { get; set; }
        public Vector2 Force { get; set; }
        public bool Trigger { get; set; }
        
        /// <summary>
        /// Coordenada Z.
        /// </summary>
        /// <remarks>Se utiliza para realizar descartes en las colisiones del simulador. 
        /// El valor de esta coordenada determina que solo respondera a colisiones con otros cuerpos que tengan el mismo valor de Z
        /// o que esten configurados para descartar el valor de Z.</remarks>
        public int Z { get; set; }
        
        /// <summary>
        /// Descarta la coordenada Z en las colisiones.
        /// </summary>
        /// <remarks>Colisiona con cualquier otro cuerpo sin importar el valor de su coordenada Z.</remarks>
        public bool ZDiscard { get; set; }
        public object Tag { get; set; }

        public float Direction
        {
            get
            {
                return MathTools.GetAngle(this.lastLocation, this.Location);
            }
        }

        public Vector2 DirectionVector
        {
            get
            {
                return this.Location - this.lastLocation;
            }
        }

        internal Rectangle Sensor
        {
            get { return new Rectangle((int)this.LB.X + 8, (int)this.LB.Y, this.Bounds.Width - 16, 1); }
        }

        public Vector2 Size 
        {
            get { return new Vector2(this.Bounds.Width, this.Bounds.Height); }
            set 
            {
                this.Bounds = new Rectangle(this.Bounds.X, this.Bounds.Y, (int)value.X, (int)value.Y);
            } 
        }

        /// <summary>
        /// Evento de colision.
        /// </summary>
        /// <remarks>Se produce cuando el cuerpo colisiona con otro.</remarks>
        public CollisionHandler OnCollision;

        /// <summary>
        /// Evento de colision con Triggers.
        /// </summary>
        /// <remarks>Se produce cuando el cuerpo colisiona con un Trigger.</remarks>
        public TriggerCollisionHandler OnTriggerCollision;

        /// <summary>
        /// Evento de impacto.
        /// </summary>
        /// <remarks>Se produce cuando un objeto o un rayo impacta sobre el cuerpo.</remarks>
        public HitHandler OnHit;

        public Body(World World)
        {
            this.Id = Guid.NewGuid();
            this.World = World;
            this.box = new RectangleOverlay();
            this.Enabled = true;
            this.Bounds = Rectangle.Empty;
            this.Weight = 0;
            this.Fixed = true;
            this.Force = Vector2.Zero;
            this.Trigger = false;
            this.Z = 0;
            this.ZDiscard = false;
        }

        public Body(World World, Rectangle Bounds, float Weight)
            : this(World)
        {
            this.Bounds = Bounds;
            this.Weight = Weight;
        }

        public Body(World World, Rectangle Bounds, float Weight, bool Fixed)
            : this(World, Bounds, Weight)
        {
            this.Fixed = Fixed;
        }

        public Body(World World, Rectangle Bounds, float Weight, bool Fixed, int Z)
            : this(World, Bounds, Weight, Fixed)
        {
            this.Z = Z;
        }

        public Body(World World, Rectangle Bounds, float Weight, bool Fixed, bool ZDiscard)
            : this(World, Bounds, Weight, Fixed)
        {
            this.ZDiscard = ZDiscard;
        }

        public void Draw(GameTime gameTime)
        {
            if (this.Visible)
            {
                // Codigo de logica especifico para la depuracion:
                box.Location = new Vector2(this.Bounds.Left, this.Bounds.Top);
                box.Size = new Vector2(this.Bounds.Width, this.Bounds.Height);

                if (this.Trigger && this.Enabled)           // Amarillo.
                {
                    this.box.ForeColor = new Color(255, 255, 0, 255);
                    this.box.BackColor = new Color(255, 255, 0, 128);
                }
                else if (this.Fixed && this.Enabled)        // Azul.
                {
                    this.box.ForeColor = new Color(0, 0, 255, 255);
                    this.box.BackColor = new Color(0, 0, 255, 128);
                }
                else                
                {
                    if (this.Enabled)       // Verde.
                    {
                        this.box.ForeColor = new Color(0, 255, 0, 255);
                        this.box.BackColor = new Color(0, 255, 0, 128);
                    }
                    else                    // Rojo.
                    {
                        this.box.ForeColor = new Color(255, 0, 0, 255);
                        this.box.BackColor = new Color(255, 0, 0, 128);
                    }
                }

                // Dibujamos la caja:
                box.Draw(gameTime);
            }
        }

        /// <summary>
        /// Aplica fuerza instantanea al cuerpo.
        /// </summary>
        /// <param name="Force">Fuerza en X e Y.</param>
        /// <remarks>La fuerza se aplica directamente sobreescribiendo los valores de fuerza actuales.</remarks>
        public void ApplyForce(Vector2 Force)
        {
            this.Force = Force;
        }

        /// <summary>
        /// Aplica fuerza instantanea en una direccion.
        /// </summary>
        /// <param name="Force">Valor de fuerza a aplicar.</param>
        /// <param name="Direction">Angulo que define la direccion en la que se aplicara la fuerza.</param>
        public void ApplyForce(float Force, float Direction)
        {
            // Calculamos el punto destino al rotar el punto origen en el angulo y radio/distancia indicado:
            this.ApplyForce(this.Location - MathTools.Move(this.Location, (int)Force, Direction));
        }

        /// <summary>
        /// Mueve el cuerpo en una direccion.
        /// </summary>
        /// <param name="Distance">Distancia en pixeles que recorrera el cuerpo.</param>
        /// <param name="Direction">Angulo que define la direccion en la que se movera.</param>
        public void Move(int Distance, float Direction)
        {
            this.Location = MathTools.Move(this.Location, Distance, Direction);
        }

        public override string ToString()
        {
            return "TLSA.Engine.Physics.V1Engine.Body { Location: (" + this.Location.ToString() + ") Bounds: (" + this.Bounds.ToString() + ") Direction: " + this.Direction.ToString() + " }";
        }
    }
}