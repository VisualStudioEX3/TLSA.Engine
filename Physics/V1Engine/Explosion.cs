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
    /// Explosion.
    /// </summary>
    /// <remarks>Permite definir un emisor de fuerza con un radio y un valor de fuerza definido que afectara a los objetos que tengan visibilidad directa con el emisor (no afectara a objetos que tengan obstaculos por medio).</remarks>
    public class Explosion
    {
        public Vector2 Location { get; set; }
        public int Radius { get; set; }
        public float Force { get; set; }
        public World World { get; internal set; }

        public Explosion(World World)
        {
            this.World = World;
        }

        public Explosion(World World, Vector2 Location, int Radius, float Force)
            : this(World)
        {
            this.Location = Location;
            this.Radius = Radius;
            this.Force = Force;
        }

        /// <summary>
        ///  Aplica la explosion con los parametros definidos.
        /// </summary>
        /// <remarks>Cada cuerpo que reciba el impacto de la fuerza de la explosion invocara el evento OnHit.</remarks>
        public void Explode()
        {
            int distance;
            float direction, response;

            // AABB de la explision:
            Rectangle AABB = new Rectangle((int)(this.Location.X - this.Radius), (int)(this.Location.Y - this.Radius), (int)(this.Location.X + this.Radius), (int)(this.Location.Y + this.Radius));

            /* Creamos un rayo para trazar trayectorias desde el epicentro 
               a los posibles objetivos dentro del area de la onda expansiva: */
            RayTracer r = new RayTracer(this.World);

            foreach (Body b in this.World.Bodies)
            {
                if (b.Enabled && !b.Fixed && !b.Trigger)
                {
                    // Comprobamos pues si esta dentro del radio de la explosion:
                    distance = (int)Vector2.Distance(this.Location, b.Location);
                    if (distance <= this.Radius)
                    {
                        /* Comprobamos que no haya ningun obstaculo. Trazamos un segmento desde la explosion hacia el centro del objeto,
                           y comprobamos que el objeto mas cercano es el analizado: */
                        /*if (r.TraceSegment(this.Location, b.Location, null, null) == b
                            || Rectangle.Intersect(AABB, b.Bounds) != Rectangle.Empty)*/
                        {
                            /* Si esta dentro del radio obtenemos el angulo desde el centro de la explision al centro del objeto
                               y hallamos el porcentaje de la fuerza que aplicaremos por la onda expansiva: */
                            direction = MathTools.GetAngle(this.Location, b.Location);

                            // Obtenemos la fuerza de respuesta segun distancia hacia el epicentro de la explosion:
                            if (this.Force > 0)
                            {
                                response = MathTools.Percent(distance - this.Radius, this.Force) * -1;

                                // Aplicamos la fuerza al cuerpo:
                                b.ApplyForce(response, direction);
                            }
                            else response = 0;

                            // Invocamos el evento OnHit:
                            if (b.OnHit != null) b.OnHit(this, response, direction);
                        }
                    }
                }
            }
        }
    }
}