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
    /// <summary>
    /// Simulador de fisicas.
    /// </summary>
    /// <remarks>Representa un area donde se aplican fisicas basicas de colision entre cajas (cuerpos).</remarks>
    public class World : TLSA.Engine.IDrawableAndUpdateable, IDisposable
    {
        public Vector2 Gravity { get; set; }
        public Rectangle WorkArea { get; set; }
        public List<Body> Bodies { get; set; }
        public bool Enabled { get; set; }
        public bool Visible { get; set; }

        public World(Vector2 Gravity, Rectangle WorkArea)
        {
            this.Gravity = Gravity;
            this.WorkArea = WorkArea;
            this.Bodies = new List<Body>();
            this.Enabled = true;
            this.Visible = false;
        }

        #region Predicados para realizar busquedas en la lista de cuerpos
        private bool GetActives(Body body)
        {
            return body.Enabled && this.WorkArea.Intersects(body.Bounds);
        }

        private Body checkBody;
        private bool GetCollide(Body body)
        {
            return body.Id != checkBody.Id && (checkBody.Bounds.Intersects(body.Bounds));
        } 
        #endregion

        // Detecta si el sensor de suelo ha colisionado con algun cuerpo:
        private bool SensorCollide(Body[] bodies, Body body)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                if (body != bodies[i] && !bodies[i].Trigger && body.Sensor.Intersects(bodies[i].Bounds)) return true;
            }
            return false;
        }

        /// <summary>
        /// Actualiza los estados de los cuerpos.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (this.Enabled)
            {
                Body[] collisions;
                Body[] actives = this.Bodies.Where(GetActives).ToArray();

                // Recorremos la lista de cuerpos activos que han actualizado su posicion:
                for (int i = 0; i < actives.Length; i++)
                {
                    // Aplicamos fuerza y gravedad:
                    if (!actives[i].Fixed && !actives[i].Trigger)
                    {
                        if (!SensorCollide(actives, actives[i]))
                            actives[i].Location += Vector2.Multiply(this.Gravity, (actives[i].Weight < 0 ? actives[i].Weight * -1 : actives[i].Weight));

                        actives[i].Location += actives[i].Force;
                    }

                    // Vamos reduciendo la fuerza en cada ciclo:
                    Vector2 delta = actives[i].Force;
                    if (delta.X > 0) delta.X -= 0.5f; else if (delta.X < 0) delta.X += 0.5f;
                    if (delta.Y > 0) delta.Y -= 0.5f; else if (delta.Y < 0) delta.Y += 0.5f;                    
                    actives[i].Force = delta;

                    // Obtenemos la lista de cuerpos con los que ha colisionado:
                    checkBody = actives[i];
                    collisions = actives.Where(GetCollide).ToArray();

                    // Recorremos la lista de cuerpos con los que ha colisionado y aplicamos la correccion de posicion adecuada:
                    for (int j = 0; j < collisions.Length; j++)
                    {
                        // Invocamos el evento de colision correspondiente:
                        if (collisions[j].Trigger && actives[i].OnTriggerCollision != null)  // Si es un Trigger:
                            actives[i].OnTriggerCollision(collisions[j]);
                        else if (actives[i].OnCollision != null)                 // El resto de cuerpos:
                            actives[i].OnCollision(collisions[j], collisions[j].Force, collisions[j].Direction);

                        ResponseCollision(actives[i], collisions[j]);
                    }
                } 
            }
        }

        // Aplica la respuesta a la colision entre los dos cuerpos:
        private void ResponseCollision(Body a, Body b)
        {
            if (a != b)
            {
                Vector2 delta = a.Location;

                // Obtenemos la interseccion entre los dos cuerpos:
                Rectangle intersection = Rectangle.Intersect(a.Bounds, b.Bounds);

                // Si los dos cuerpos se intersectan evaluamos respuesta:
                if (intersection != Rectangle.Empty)
                {
                    // Si no es un trigger y coinciden las coordenadas Z (o si B descarta Z) se aplica respuesta en la colision:
                    if (!b.Trigger && (b.ZDiscard || a.Z == b.Z))
                    {
                        if (intersection.Height < intersection.Width)
                        {
                            if (intersection.Top == a.Bounds.Top)
                                delta.Y += intersection.Height;
                            else if (intersection.Bottom == a.Bounds.Bottom)
                                delta.Y -= intersection.Height;
                        }
                        else if (intersection.Height > intersection.Width)
                        {
                            if (intersection.Left == a.Bounds.Left)
                                delta.X += intersection.Width;
                            else if (intersection.Right == a.Bounds.Right)
                                delta.X -= intersection.Width;
                        }
                        else
                        {
                            if (MathTools.PointInRect(a.LT, b.Bounds))
                            {
                                delta.X += intersection.Width;
                                delta.Y += intersection.Height;
                            }
                            if (MathTools.PointInRect(a.RT, b.Bounds))
                            {
                                delta.X -= intersection.Width;
                                delta.Y += intersection.Height;
                            }
                            if (MathTools.PointInRect(a.LB, b.Bounds))
                            {
                                delta.X += intersection.Width;
                                delta.Y -= intersection.Height;
                            }
                            if (MathTools.PointInRect(a.RB, b.Bounds))
                            {
                                delta.X -= intersection.Width;
                                delta.Y -= intersection.Height;
                            }
                        }

                        // Aplicamos la correccion de posicion:
                        if (!a.Fixed && !a.Trigger) a.Location = new Vector2(delta.X, delta.Y);

                        if (a.Sensor.Intersects(b.Bounds)) a.Force = new Vector2(a.Force.X, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Dibuja todos los cuerpos fisicos de la escena.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            if (this.Visible)
                foreach (Body b in this.Bodies)
                    b.Draw(gameTime);
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                this.Bodies.Clear();
                this.disposed = true;
                GC.Collect();
            }
        }
    }
}