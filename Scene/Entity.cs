using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TLSA.Engine.Scene
{
    /// <summary>
    /// Representa una entidad de la escena.
    /// </summary>
    /// <remarks>Este objeto se utiliza para gestionar automaticamente la actualizacion de logica y dibujo de un objeto en la escena.
    /// Herede esta clase en sus objetos del juego para poder integrarlos en el Stage.</remarks>
    public class Entity : IComparable, TLSA.Engine.IDrawableAndUpdateable
    {
        // Referencia al Stage al que pertenece:
        internal Stage Father { get; set; }

        // Tiempo de creacion del objeto:
        internal DateTime CreationTime { get; private set; }
                
        #region Propiedades
        /// <summary>
        /// Nombre asignado a la entidad en el Stage.
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Determina si el objeto esta activo.
        /// </summary>
        public virtual bool Enabled { get; set; }
        /// <summary>
        /// Determina el orden de actualizacion en la lista.
        /// </summary>
        /// <remarks>Mientras mayor sea el valor antes se actualizara el objeto en la escena.</remarks>
        public virtual int Priority { get; set; }
        /// <summary>
        /// Posicion del objeto en la escena.
        /// </summary>
        public virtual Vector2 Location { get; set; }
        /// <summary>
        /// Orden de dibujado en la escena.
        /// </summary>
        /// <remarks>Mientras mayor sea el valor antes se dibujara el objeto en la escena.</remarks>
        public virtual int ZOrder { get; set; }
        /// <summary>
        /// Determina si el objeto se dibujara.
        /// </summary>
        public virtual bool Visible { get; set; }
        /// <summary>
        /// Determina el area que ocupa la entidad.
        /// </summary>
        public virtual Rectangle Bounds { get; set; }
        /// <summary>
        /// Etiqueta que identifica la entidad o al grupo al que pertenece.
        /// </summary>
        /// <remarks>Utilice esta propiedad para enviar mensajes a la entidad o grupo de entidades con la misma etiqueta.</remarks>
        public virtual string Tag { get; set; }
        #endregion

        public Entity()
        {
            this.CreationTime = DateTime.Now;

            this.Bounds = Rectangle.Empty;
            this.Enabled = true;
            this.Location = Vector2.Zero;
            this.Priority = 0;
            this.Tag = string.Empty;
            this.Visible = true;
            this.ZOrder = 0;
        }

        /// <summary>
        /// Indica si la entidad ha sido inicializada.
        /// </summary>
        public bool IsInitialized { get; internal set; }

        #region Metodos y funciones publicas        
        /// <summary>
        /// Inicializa la entidad.
        /// </summary>
        /// <remarks>Este codigo sera ejecutado al agregar la entidad en el Stage.
        /// Sobrecargue este metodo para escribir su propio codigo de inicializacion. Se debe mantener la llamada al metodo base (base.Initialize()) para una correcta inicializacion.</remarks>
        public virtual void Initialize()
        {
            this.IsInitialized = true;
        }

        /// <summary>
        /// Termina la entidad.
        /// </summary>
        /// <remarks>Este metodo sera llamado por el Stage si se llamo al metodo Kill() de la entidad.
        /// Utilice este metodo para definir el codigo donde liberar recursos utilizados por la entidad y la clase que la hereda.</remarks>
        public virtual void Terminate() 
        {
            this.Bounds = Rectangle.Empty;
            this.Enabled = false;
            this.Location = Vector2.Zero;
            this.Priority = 0;
            this.Tag = string.Empty;
            this.Visible = false;
            this.ZOrder = 0;

            this.IsInitialized = false;
        }

        /// <summary>
        /// Devuelve las dimensiones y ubicacion del area rectangular que ocupa la entidad visualmente.
        /// </summary>
        /// <returns>Rectangulo que representa el area visual de la entidad en el espacio de coordenadas.</returns>
        /// <remarks>Si se sobrecarga la funcion para que devuelva el area real de visualizacion de la entidad los Stage puede utilizar esta informacion para
        /// optimizar la lista de entidades a dibujar descartando las que no esten en el area visual de la pantalla.
        /// Por defecto esta funcion, si no esta sobrecargada, devolvera un rectagulo vacio.</remarks>
        public virtual Rectangle GetBounds()
        {
            return this.Bounds;
        }

        /// <summary>
        /// Actualiza los estados del objeto.
        /// </summary>
        /// <remarks>Este metodo se debe sobrecargar para escribir el codigo de logica del objeto.</remarks>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Dibuja el objeto.
        /// </summary>
        /// <remarks>Este metodo se debera sobrecargar para programar el codigo de dibujo del objeto.</remarks>
        public virtual void Draw(GameTime gameTime)
        {
        }

        /// <summary>
        /// Evento receptor de mensajes.
        /// </summary>
        /// <param name="message">Mensaje recibido.</param>
        /// <param name="values">Lista de valores recibidos si lo hubiera.</param>
        /// <remarks>Este metodo se implementa como evento para recibir los mensajes que se envien desde la cola de mensajes al que este asociado el Stage.</remarks>
        public virtual void ReciveMessage(string message, params object[] values)
        {
        }

        internal bool Killed;
        /// <summary>
        /// Mata la entidad.
        /// </summary>
        /// <remarks>A llamar este metodo el Stage llamara al metodo Dispose del objeto al finalizar el proceso de actualizacion de todas las entidades.
        /// Este metodo facilita la tarea de eliminar la instancia del objeto desde el propio objeto, por ejemplo desde un evento que implemente la clase que herede la clase Entidad.</remarks>
        public void Kill()
        {
            this.Killed = true;
            this.Enabled = false;
            this.Visible = false;
            this.Father.killed++;
        }
        #endregion

        #region Codigo para realizar la ordenacion de la entidad en el Stage
        /// <summary>
        /// Metodo por defecto de la interfaz IComprable.
        /// </summary>
        /// <param name="obj">Objeto que se comparara con la instancia.</param>
        /// <returns>Devuelve -1 si es mayor, 0 si es igual o 1 si es menor.</returns>
        /// <remarks>El metodo por defecto implementa la comparacion por el campo Priority.</remarks>
        public int CompareTo(object obj)
        {
            Entity e = (Entity)obj;
            if (this.Priority > e.Priority) return -1; else if (this.Priority == e.Priority) return 0; else return 1;
        }

        /// <summary>
        /// Comparador para ordenar por la propiedad ZOrder.
        /// </summary>
        private class SortByZOrderHelper : IComparer<Entity>
        {
            int IComparer<Entity>.Compare(Entity a, Entity b)
            {
                if (a.ZOrder > b.ZOrder) return -1; else if (a.ZOrder == b.ZOrder) return 0; else return 1;
            }
        }

        /// <summary>
        /// Generra una instancia de IComparer para ordenar una coleccion generica por el valor de la propiedad ZOrder.
        /// </summary>
        /// <returns>Devuelve una instancia de IComparer.</returns>
        /// <remarks>Esta funcion solo la utiliza el Stage.</remarks>
        public static IComparer<Entity> SortByZOrder()
        {
            return new SortByZOrderHelper();
        }

        /// <summary>
        /// Comparador para ordenar por la propiedad Priority.
        /// </summary>
        private class SortByPriorityHelper : IComparer<Entity>
        {
            int IComparer<Entity>.Compare(Entity a, Entity b)
            {
                if (a.Priority > b.Priority) return -1; else if (a.Priority == b.Priority) return 0; else return 1;
            }
        }

        /// <summary>
        /// Generra una instancia de IComparer para ordenar una coleccion generica por el valor de la propiedad Priority.
        /// </summary>
        /// <returns>Devuelve una instancia de IComparer.</returns>
        /// <remarks>Esta funcion solo la utiliza el Stage.</remarks>
        public static IComparer<Entity> SortByPriority()
        {
            return new SortByPriorityHelper();
        }

        private class SortByCreationTimeHelper : IComparer<Entity>
        {
            int IComparer<Entity>.Compare(Entity a, Entity b)
            {
                if (a.CreationTime > b.CreationTime) return -1; else if (a.CreationTime == b.CreationTime) return 0; else return 1;
            }
        }

        public static IComparer<Entity> SortByCreationTime()
        {
            return new SortByCreationTimeHelper();
        }
        #endregion                
    }
}
