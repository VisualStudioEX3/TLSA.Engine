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
    /// Trazador de rayos.
    /// </summary>
    /// <remarks>Permite trazar una trayectoria desde un punto origen para obtener el punto de corte mas cercano con los objetos de la escena.</remarks>
    public class RayTracer
    {
        #region Estructuras internas
        /* Estructuras para gestionar las intersecciones, distancias y cuerpos 
           en la busqueda de intersecciones del trazador de rayos */
        private struct Line
        {
            public Vector2 a;
            public Vector2 b;
            public Body hitBody;

            public Line(Vector2 a, Vector2 b, Body hitBody)
            {
                this.a = a; this.b = b; this.hitBody = hitBody;
            }
        }

        private struct IntersectionPoint
        {
            public Vector2 p;
            public float distance;
            public Body hitBody;

            public IntersectionPoint(Vector2 p, float distance, Body hitBody)
            {
                this.p = p; this.distance = distance; this.hitBody = hitBody;
            }
        }
        #endregion

        #region Miembros privados
        private LineOverlay line; 
        #endregion

        #region Propiedades
        /// <summary>
        /// Origen del rayo.
        /// </summary>
        public Vector2 Source { get; set; }
        /// <summary>
        /// Cuerpo origen desde el que se traza el rayo.
        /// </summary>
        /// <remarks>Se utiliza para descartar el cuerpo en el trazado del rayo. 
        /// Si no hubiera cuerpo de origen esta propiedad se establece a Null.</remarks>
        public Body SourceBody { get; set; }
        /// <summary>
        /// Punto de impacto.
        /// </summary>
        /// <remarks>Devuelve las coordenadas de impacto del rayo.
        /// Si el rayo no impacta con ningun cuerpo devuelve el punto mas lejano a donde se traza el rayo.</remarks>
        public Vector2 Hit { get; internal set; }
        /// <summary>
        /// Direccion en la que se traza el rayo.
        /// </summary>
        public float Direction { get; set; }
        /// <summary>
        /// Alcance maximo que tendra el rayo.
        /// </summary>
        public int Radius { get; set; }
        /// <summary>
        /// Lista de coordenadas Z que se descartaran.
        /// </summary>
        /// <remarks>Este parametro se utiliza para realizar descartes de cuerpos en base a su coordenada Z.</remarks>
        public List<int> ZDiscardList { get; set; }
        /// <summary>
        /// Referencia al simulador al que esta asociado el trazador de rayos.
        /// </summary>
        public World World { get; internal set; }
        #endregion

        #region Constructores
        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="World">Referencia a la instancia del simulador al que se asociara el rayo.</param>
        /// <param name="Source">Punto de origen del rayo.</param>
        public RayTracer(World World)
        {
            this.World = World;
            this.Source = Vector2.Zero;
            this.line = new LineOverlay(); this.line.Color = Color.Red;
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="World">Referencia a la instancia del simulador al que se asociara el rayo.</param>
        /// <param name="Source">Punto de origen del rayo.</param>
        public RayTracer(World World, Vector2 Source)
            : this(World)
        {
            this.Source = Source;
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="World">Referencia a la instancia del simulador al que se asociara el rayo.</param>
        /// <param name="Source">Punto de origen del rayo.</param>
        /// <param name="Radius">Radio maximo de alcance del rayo.</param>
        public RayTracer(World World, Vector2 Source, int Radius)
            : this(World, Source)
        {
            this.Radius = Radius;
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="World">Referencia a la instancia del simulador al que se asociara el rayo.</param>
        /// <param name="Source">Punto de origen del rayo.</param>
        /// <param name="Radius">Radio maximo de alcance del rayo.</param>
        /// <param name="ZDiscardList">Lista de coordenadas Z que se descartaran.</param>
        public RayTracer(World World, Vector2 Source, int Radius, List<int> ZDiscardList)
            : this(World, Source, Radius)
        {
            this.ZDiscardList = ZDiscardList;
        }
        #endregion

        #region Metodos publicos
        /// <summary>
        /// Traza el rayo con los parametros establecidos.
        /// </summary>
        /// <returns>Devuelve el cuerpo con el que impacta.</returns>
        /// <remarks>La propiedad Hit devuelve el punto de impacto exacto del rayo.</remarks>
        public Body Trace()
        {
            // Recorremos los cuerpos que esten dentro del area definida por la diagonal del rayo:
            List<Line> lines = new List<Line>();
            foreach (Body b in this.GetBodiesInArea(this.ComputeRayArea()))
            {
                // Añadimos las lineas del cuerpo a la lista a evaluar:
                lines.Add(new Line(new Vector2(b.Bounds.Left, b.Bounds.Top), new Vector2(b.Bounds.Right, b.Bounds.Top), b));
                lines.Add(new Line(new Vector2(b.Bounds.Left, b.Bounds.Top), new Vector2(b.Bounds.Left, b.Bounds.Bottom), b));
                lines.Add(new Line(new Vector2(b.Bounds.Right, b.Bounds.Top), new Vector2(b.Bounds.Right, b.Bounds.Bottom), b));
                lines.Add(new Line(new Vector2(b.Bounds.Left, b.Bounds.Bottom), new Vector2(b.Bounds.Right, b.Bounds.Bottom), b));
            }

            // Obtenemos todos los puntos de corte que haya en el trazado del rayo:
            List<IntersectionPoint> points = new List<IntersectionPoint>(); Vector2? ret;
            foreach (Line line in lines)
            {
                ret = MathTools.IntersectLines(this.Source, this.Hit, line.a, line.b);
                if (ret != null)
                    points.Add(new IntersectionPoint((Vector2)ret, Vector2.Distance(this.Source, (Vector2)ret), line.hitBody));
            }

            // Si existen puntos de interseccion obtenemos el mas cercano:
            if (points.Count > 0)
            {
                IntersectionPoint near = points[0];
                foreach (IntersectionPoint p in points)
                    if (p.distance < near.distance) near = p;
                this.Hit = near.p;      // Marcamos el impacto con el punto de interseccion mas cercano.
                return near.hitBody;    // Devolvemos el cuerpo de impacto mas cercano.
            }
            else // Si no hay puntos de corte devolvemos null y mantenemos el punto mas lejano del rayo como impacto:
                return null;
        }

        /// <summary>
        /// Dibuja el trazado del rayo.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <remarks>Funcion de depuracion que representa el rayo en pantalla.</remarks>
        public void Draw(GameTime gameTime)
        {
            if (this.line.B != null)
            {
                this.line.A = this.Source;
                this.line.B = this.Hit;
                this.line.Draw(gameTime);
            }
        }
        #endregion

        #region Metodos privados para calcular las intersecciones con los cuerpos
        // Comprueba si un valor esta en la lista:
        private bool ZDiscard(int Z, List<int> ZList)
        {
            if (ZList != null)
                foreach (int value in ZList)
                    if (value == Z) return true;
            return false;
        }

        // Computa el area donde se proyecta el rayo:
        private Rectangle ComputeRayArea()
        {
            Vector2 a = this.Source;
            Vector2 b = MathTools.Move(this.Source, this.Radius, Math.Abs(this.Direction)); this.Hit = b;
            Vector2 swap = Vector2.Zero;

            if (a.X > b.X) { swap.X = b.X; b.X = a.X; a.X = swap.X; }
            if (a.Y > b.Y) { swap.Y = b.Y; b.Y = a.Y; a.Y = swap.Y; }

            /* En caso de que la direccion sea perpendicular, para evitar que el area de interseccion sea plana (sin volumen)
               segun la direccion incrementamos o decrementamos en 1 la altura o anchura del area: */
            switch ((int)this.Direction)
            {
                case 0: case 180: a.Y--; b.Y++; break;
                case 90: case 270: a.X--; b.X++; break;
            }

            return new Rectangle((int)a.X, (int)a.Y, (int)(b.X - a.X), (int)(b.Y - a.Y));
        }

        // Obtiene la lista de cuerpos activos que estan dentro del area del rayo:
        private List<Body> GetBodiesInArea(Rectangle RayArea)
        {
            List<Body> ret = new List<Body>();
            foreach (Body b in this.World.Bodies)
                if (b != this.SourceBody && b.Enabled && !b.Trigger && !ZDiscard(b.Z, this.ZDiscardList) && Rectangle.Intersect(RayArea, b.Bounds) != Rectangle.Empty)
                    ret.Add(b);
            return ret;
        }
        #endregion
    }
}