using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TLSA.Engine.Tools
{
    /// <summary>
    /// Funciones de ayuda.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Intercambia los valores entre dos variables.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de las variables.</typeparam>
        /// <param name="A">Variable A.</param>
        /// <param name="B">Variable B.</param>
        public static void Swap<T>(ref T A, ref T B)
        {
            T C = A; A = B; B = C;
        }

        /// <summary>
        /// Convierte una estructura Point en Vector2.
        /// </summary>
        /// <param name="point">Punto a convertir.</param>
        /// <returns>Estructura Vector2 con la informacion del punto.</returns>
        public static Vector2 PointToVector2(Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// Convierte una estructura Vector2 en Point.
        /// </summary>
        /// <param name="vector">Vector a convertir.</param>
        /// <returns>Estructura Point con la informacion del vector.</returns>
        public static Point Vector2ToPoint(Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }
    }
}
