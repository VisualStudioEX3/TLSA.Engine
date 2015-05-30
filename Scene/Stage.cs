using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

// XNA:
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// Referencias a TLSA.Engine:
using TLSA.Engine;
using TLSA.Engine.Graphics;
using TLSA.Engine.Graphics.Primitives;
using TLSA.Engine.Input;
using TLSA.Engine.Tools;
using TLSA.Engine.Tools.IO;
using TLSA.Engine.Tools.XML;

namespace TLSA.Engine.Scene
{
    /// <summary>
    /// Representa un Stage para gestionar los objetos de la escena.
    /// </summary>
    public class Stage : IDisposable
    {
        internal List<Entity> nodes;
        internal int killed = 0;

        public bool Enabled { get; set; }

        public bool Visible { get; set; }

        /// <summary>
        /// Lista de entidades de la escena:
        /// </summary>
        public ReadOnlyCollection<Entity> Nodes 
        { 
            get 
            { 
                return this.nodes.AsReadOnly(); 
            } 
        }

        /// <summary>
        /// Añade una nueva entidad a la escena.
        /// </summary>
        /// <param name="newEntity">Instancia de la entidad.</param>
        public void AddEntity(Entity newEntity)
        {
            this.AddEntity(newEntity, "");
        }

        /// <summary>
        /// Añade una nueva entidad a la escena.
        /// </summary>
        /// <param name="newEntity">Instancia de la entidad.</param>
        /// <param name="Name">Opcional. Nombre asignado a la entidad.
        /// Los nombres pueden repetirse en varias entidades.</param>
        public void AddEntity(Entity newEntity, string Name)
        {
            newEntity.Name = Name;
            newEntity.Father = this;
            newEntity.Initialize();
            this.nodes.Add(newEntity);
        }

        public Entity FindByName(string Name)
        {
            try
            {
                return this.nodes[nodes.IndexOf(nodes.OrderByDescending(e => e.CreationTime).Where(e => e.Name == Name).Last())];
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Elimina una entidad.
        /// </summary>
        /// <param name="Name">Nombre de la entidad. Si hubiera mas de una entidad con el mismo nombre se eliminara la ultima entidad agregada con dicho nombre.</param>
        public void RemoveEntity(string Name)
        {
            this.RemoveEntity(nodes.IndexOf(nodes.OrderByDescending(e => e.CreationTime).Where(e => e.Name == Name).Last()));
        }

        /// <summary>
        /// Elimina una entidad.
        /// </summary>
        /// <param name="Index">Indice de la entidad en la lista.</param>
        public void RemoveEntity(int Index)
        {
            nodes[Index].Terminate();
            this.nodes.RemoveAt(Index);
        }

        /// <summary>
        /// Elimina todas las entidades.
        /// </summary>
        /// <remarks>Libera una por una cada entidad que haya en la lista de nodos.</remarks>
        public void Clear()
        {
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                if (this.Nodes[i] != null)
				{
					this.Nodes[i].Terminate();
					this.nodes[i] = null;
				}
            }
            this.nodes.Clear();
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        public Stage()
        {
            this.nodes = new List<Entity>();
            this.Enabled = true;
            this.Visible = true;
        }

        /// <summary>
        /// Actualiza las entidades.
        /// </summary>
        /// <remarks>Al finalizar la actualizacion se eliminaran las entidades que esten muertas.</remarks>
        public void Update(GameTime gameTime)
        {
            if (this.Enabled)
			{
				// Ordenamos la lista por prioridad y obtenemos los elementos activos y muertos:
				Entity[] UpdateList = this.nodes.Where(e => e.Enabled == true || e.Killed == true).OrderByDescending(e => e.Priority).ToArray();
				for (int i = 0; i < UpdateList.Length; i++)
				{
					if (UpdateList[i].Enabled) // Si el elemento esta activo lo actualizamos:
					{
						UpdateList[i].Update(gameTime);
					}
					else if (UpdateList[i].Killed) // Si el elemento esta muerto destruimos su instancia:
					{
						UpdateList[i].Terminate();
						nodes.RemoveAt(nodes.IndexOf(UpdateList[i]));
						UpdateList[i] = null;
					}
				}

				// Eliminamos basura de la memoria en caso de que se hayan destruido entidades:
				if (this.killed > 0)
				{
					//GC.Collect();
					this.killed = 0;
				}
			}
        }        

        /// <summary>
        /// Dibuja todas las entidades de la escena.
        /// </summary>
        public void Draw(GameTime gameTime)
        {
            if (this.Visible)
			{
				Entity[] DrawList = this.nodes.Where(GetVisibles).OrderByDescending(e => e.ZOrder).ToArray();
				for (int i = 0; i < DrawList.Length; i++)
				{
					if (DrawList[i].Visible) DrawList[i].Draw(gameTime);
				}
			}
        }

        #region Delegados para realizar listados personalizados con FindAll() o con el metodo de extension Where<> en la lista Nodes
        // Filtra por entidades activas y muertas:
        private static bool GetEnabledAndDead(Entity e)
        {
            try
            {
                return e.Enabled || e.Killed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Filtra por entidades activas:
        private static bool GetEnabled(Entity e)
        {
            try
            {
                return e.Enabled && !e.Killed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Filtra por entidades muertas:
        private static bool GetDead(Entity e)
        {
            try
            {
                return e.Killed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Filtra por entidades visibles y que esten dentro del area de vision del render:
        private static bool GetVisibles(Entity e)
        {
            try
            {
                Rectangle eBounds = e.GetBounds();
                return e.Visible && (eBounds == Rectangle.Empty ? true : eBounds.Intersects(Manager.Graphics.ScreenBounds));
            }
            catch (Exception)
            {
                return false;
            }
        } 
        #endregion

        /// <summary>
        /// Destruye la instancia de la clase.
        /// </summary>
        /// <remarks>Destruye y libera una por una cada entidad que haya en la lista de nodos, y despues libera los recursos propios del Stage.</remarks>
        public void Dispose()
        {
            this.Clear();
        }
    }
}
