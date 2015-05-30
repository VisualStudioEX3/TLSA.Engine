using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TLSA.Engine.Scene
{
    /// <summary>
    /// Estructura de un mensaje.
    /// </summary>
    struct Message
    {
        public string tag;      // Etiqueta del/los objetivo/s
        public string message;  // Mensaje a enviar.
        public object[] value;  // Valor asociado al mensaje.

        public Message(string tag, string message, object[] value)
        {
            this.tag = tag; this.message = message; this.value = value;
        }
    }

    /// <summary>
    /// Gestor de mensajes.
    /// </summary>
    /// <remarks>Este gestor de mensajes permite comunicar las entidades de un Stage mendiante mensajes.</remarks>
    public class MessageQueue
    {
        private Queue<Message> current, prev;
        private Stage scene;

        public MessageQueue(Stage scene)
        {
            this.scene = scene;
            prev = new Queue<Message>();
        }

        /// <summary>
        /// Envia un mensaje a la cola.
        /// </summary>
        /// <param name="tag">Etiqueta a quien va dirigido el mensaje.</param>
        /// <param name="message">Mensaje a enviar.</param>
        /// <param name="values">Lista opcional de valores del mensaje.</param>
        /// <remarks>Solo las entidades activas recibiran mensajes.</remarks>
        public void SendMessage(string tag, string message, params object[] values)
        {
            prev.Enqueue(new Message(tag, message, values));
        }
        
        /// <summary>
        /// Procesa toda la cola de mensajes.
        /// </summary>
        public void Process()
        {
			Message read;
			current = prev;
			while (current.Count > 0)
			{
				read = current.Dequeue(); // Obtenemos el siguiente mensaje de la cola.
				tag = read.tag; // Establecemos la etiqueta a buscar.
				if (tag == "")  // Si la etiqueta esta vacia se envia el mensaje a todas las entidades:
					entities = scene.nodes.ToArray();
				else    // En caso contrario se filtra la lista por la etiqueta:
					entities = scene.nodes.Where(GetByTag).ToArray(); // Obtenemos la lista con todas las entidades que tienen la etiqueta.

				// Invocamos el evento de la entidad:
				for (int i = 0; i < entities.Length; i++)
					entities[i].ReciveMessage(read.message, read.value);
			}
        }

        #region Delegado y codigo para las busquedas de entidades por su etiqueta
        private static Entity[] entities;
        private static string tag;
        private static bool GetByTag(Entity target)
        {
            try
            {
                return (target.Tag == tag && target.Enabled);
            }
            catch (Exception)
            {
                return false;
            }
        } 
        #endregion
    }
}
