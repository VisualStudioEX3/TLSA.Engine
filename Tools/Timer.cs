using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TLSA.Engine.Tools
{
    /// <summary>
    /// Cronometro independiente de tiempo.
    /// </summary>
    public class Timer
    {
        private int delta;

        public Timer()
        {
            this.Reset();
        }

        /// <summary>
        /// Devuelve los milisegundos transcurridos desdel ultimo reinicio o desde que se creo el cronometro.
        /// </summary>
        public int Value { get { return ((int)DateTime.Now.Ticks - delta) / 10000; } }

        /// <summary>
        /// Reinicia el cronometro a 0.
        /// </summary>
        public void Reset()
        {
            delta = (int)System.DateTime.Now.Ticks;
        }
    }
}
