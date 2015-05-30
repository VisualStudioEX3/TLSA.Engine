using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNAMathHelper = Microsoft.Xna.Framework.MathHelper;

namespace TLSA.Engine.Tools
{
    /// <summary>
    /// Libreria matematica de ayuda de TLSA.Engine.
    /// </summary>
    public static class MathTools
    {
        private static Random rnd = new Random();

        /// <summary>
        /// Generador de numeros aleatorios.
        /// </summary>
        /// <remarks>Instancia del objeto System.Random del motor.</remarks>
        public static Random RandomNumberGenerator { get { return rnd; } }

        /// <summary>
        /// Calcula el angulo entre dos puntos.
        /// </summary>
        /// <param name="A">Punto A.</param>
        /// <param name="B">Punto B.</param>
        /// <returns>Devuelve el angulo en grados en los dos puntos.</returns>
        public static float GetAngle(Vector2 A, Vector2 B)
        {
            float angle = XNAMathHelper.ToDegrees((float)Math.Atan2(B.Y - A.Y, B.X - A.X));
            return (angle < 0 ? angle + 360 : angle);
        }

        /// <summary>
        /// Desplaza un vector en un angulo.
        /// </summary>
        /// <param name="point">Vector a desplazar.</param>
        /// <param name="distance">Distancia en pixeles.</param>
        /// <param name="direction">Angulo que define la direccion.</param>
        /// <returns>Devuelve la nueva posicion del vector tras aplicar el desplazamiento.</returns>
        public static Vector2 Move(Vector2 point, int distance, float direction)
        {
            float rad = Microsoft.Xna.Framework.MathHelper.ToRadians(direction);

            // Forzamos rendodeo a mayor para asegurarnos de que el vector devuelto sea correcto con la 
            // posicion en pixeles y evitar fallos de precision por los decimales:
            float x = (float)Math.Round(point.X + distance * Math.Cos(rad), 0);
            float y = (float)Math.Round(point.Y + distance * Math.Sin(rad), 0);
            
            // Generamos el vector en base a los valores redondeados:
            return new Vector2(x, y);
        }

        /// <summary>
        /// Indica si un punto esta dentro de un rectangulo.
        /// </summary>
        /// <param name="Point">Punto a evaluar.</param>
        /// <param name="Rect">Rectangulo a evaluar.</param>
        /// <returns>Devuelve verdadero si el punto se encuentra dentro del rectangulo.</returns>
        public static bool PointInRect(Vector2 Point, Rectangle Rect)
        {
            return Rect.Contains(new Point((int)Point.X, (int)Point.Y)); 
        }

        /// <summary>
        /// Calcula la interseccion entre dos lineas.
        /// </summary>
        /// <param name="A">Inicio de la primera linea.</param>
        /// <param name="B">Final de la primera linea.</param>
        /// <param name="C">Inicio de la segunda linea.</param>
        /// <param name="D">Final de la segunda linea.</param>
        /// <returns>La coordenada de la interseccion o null si no hay interseccion.</returns>
        /// <remarks>Basado en el codigo de Marius Watz para Java: 
        /// http://workshop.evolutionzone.com/2007/09/10/code-2d-line-intersection/
        /// </remarks>
        public static Vector2? IntersectLines(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            float xD1, yD1, xD2, yD2, xD3, yD3;
            float dot, deg, len1, len2;
            float segmentLen1, segmentLen2;
            float ua, ub, div;

            // calculate differences  
            xD1 = B.X - A.X;
            xD2 = D.X - C.X;
            yD1 = B.Y - A.Y;
            yD2 = D.Y - C.Y;
            xD3 = A.X - C.X;
            yD3 = A.Y - C.Y;

            // calculate the lengths of the two lines  
            len1 = (float)Math.Sqrt(xD1 * xD1 + yD1 * yD1);
            len2 = (float)Math.Sqrt(xD2 * xD2 + yD2 * yD2);

            // calculate angle between the two lines.  
            dot = (xD1 * xD2 + yD1 * yD2); // dot product  
            deg = dot / (len1 * len2);

            // if abs(angle)==1 then the lines are parallell,  
            // so no intersection is possible  
            if (Math.Abs(deg) == 1) return null;

            // find intersection Pt between two lines  
            Vector2 pt = new Vector2(0, 0);
            div = yD2 * xD1 - xD2 * yD1;
            ua = (xD2 * yD3 - yD2 * xD3) / div;
            ub = (xD1 * yD3 - yD1 * xD3) / div;
            pt.X = A.X + ua * xD1;
            pt.Y = A.Y + ua * yD1;

            // calculate the combined length of the two segments  
            // between Pt-A and Pt-B  
            xD1 = pt.X - A.X;
            xD2 = pt.X - B.X;
            yD1 = pt.Y - A.Y;
            yD2 = pt.Y - B.Y;
            segmentLen1 = (float)(Math.Sqrt(xD1 * xD1 + yD1 * yD1) + Math.Sqrt(xD2 * xD2 + yD2 * yD2));

            // calculate the combined length of the two segments  
            // between Pt-C and Pt-D  
            xD1 = pt.X - C.X;
            xD2 = pt.X - D.X;
            yD1 = pt.Y - C.Y;
            yD2 = pt.Y - D.Y;
            segmentLen2 = (float)(Math.Sqrt(xD1 * xD1 + yD1 * yD1) + Math.Sqrt(xD2 * xD2 + yD2 * yD2));

            // if the lengths of both sets of segments are the same as  
            // the lenghts of the two lines the point is actually  
            // on the line segment.  

            // if the point isn't on the line, return null  
            if (Math.Abs(len1 - segmentLen1) > 0.01 || Math.Abs(len2 - segmentLen2) > 0.01)
                return null;

            // return the valid intersection  
            return pt;
        }  

        /// <summary>
        /// Calcula el porcentaje de un valor segun el rango indicado.
        /// </summary>
        /// <param name="Value">Valor a evaluar.</param>
        /// <param name="Range">Rango donde se calculara el porcentaje.</param>
        /// <returns>Devuelve el valor que representa el porcentaje del valor en el rango indicado.</returns>
        public static float Percent(float Value, float Range)
        {
            return Value / Range * 100;
        }
    }
}
