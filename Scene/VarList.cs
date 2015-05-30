using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TLSA.Engine.Scene
{
    /// <summary>
    /// Lista de variables.
    /// </summary>
    /// <remarks>La lista permite almacenar variables para almacenar distintos tipos de datos.
    /// La idea es que la lista sea accesible de forma global para ser visible desde cualquier componente.</remarks>
    public class VarList
    {
        private Dictionary<string, object> vars;

        internal VarList()
        {
            vars = new Dictionary<string, object>();
        }

        /// <summary>
        /// Crea una nueva variable en la lista.
        /// </summary>
        /// <param name="name">Nombre de la variable.</param>
        /// <param name="value">Valor de la variable.</param>
        public void Create(string name, object value)
        {
            vars.Add(name, value);
        }

        /// <summary>
        /// Elimina una variable de la lista.
        /// </summary>
        /// <param name="name">Nombre de la variable.</param>
        public void Delete(string name)
        {
            vars.Remove(name);
        }

        /// <summary>
        /// Devuelve o establece el valor de una variable.
        /// </summary>
        /// <param name="name">Nombre de la variable.</param>
        /// <returns>Devuelve el valor de la variable.</returns>
        public object this[string name]
        {
            get { return vars[name]; }
            set { vars[name] = value; }
        }

        /// <summary>
        /// Elimina toda las variables de la lista.
        /// </summary>
        public void Clear()
        {
            vars.Clear();
        }
    }
}