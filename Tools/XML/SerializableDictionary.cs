/// Codigo basado en el original de John Saunders de su articulo "One Way to Serialize Dictionaries"
/// http://johnwsaundersiii.spaces.live.com/blog/cns!600A2BE4A82EA0A6!699.entry

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace TLSA.Engine.Tools.XML
{
    /// <summary>
    /// Clase Diccionario que soporta serializacion a XML.
    /// </summary>
    /// <typeparam name="K">Tipo de dato de la clave.</typeparam>
    /// <typeparam name="V">Tipo de dato del valor.</typeparam>
    /// <remarks>Los serializadores XML de .NET (el InmediateXMLSerializer de XNA no se ve afectado) no puede serializar clases que implementen la interfaz IDictionary. Esta clase implementa las funcionalidades de un diccionario (IDictionary) pero con capacidad para ser serializado a XML.</remarks>
    public class SerializableDictionary<K, V>
    {
        #region Construction and Initialization
        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="original">Diccionario (IDictionary) que se usara para inicializar la clase.</param>
        public SerializableDictionary(IDictionary<K, V> original)
        {
            this.Dictionary = original;
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        public SerializableDictionary()
        {
            this.Dictionary = new Dictionary<K,V>();
        }
        #endregion

        #region Interfaz similar a IDictionary
        /// <summary>
        /// Añade un elemento al diccionario.
        /// </summary>
        /// <param name="Key">Clave del elemento.</param>
        /// <param name="Value">Valor del elemento.</param>
        public void Add(K Key, V Value)
        {
            this.Dictionary.Add(Key, Value);
        }

        /// <summary>
        /// Añade un elemento al diccionario.
        /// </summary>
        /// <param name="item">Elemento a añadir.</param>
        public void Add(SerializableKeyValuePair item)
        {
            this.Dictionary.Add(item.ToKeyValuePair());
        }

        /// <summary>
        /// Elimina un elemento del diccionario.
        /// </summary>
        /// <param name="Key">Clave del elemento.</param>
        public void Remove(K Key)
        {
            this.Dictionary.Remove(Key);
        }

        /// <summary>
        /// Elimina un elemento del diccionario.
        /// </summary>
        /// <param name="item">Elemento a eliminar.</param>
        public void Remove(SerializableKeyValuePair item)
        {
            this.Dictionary.Remove(item.ToKeyValuePair());
        }

        /// <summary>
        /// Devuelve la lista de valores del diccionario.
        /// </summary>
        [XmlIgnore]
        public ReadOnlyCollection<V> Values { get { RebuildInternalDictionary(); return this.Dictionary.Values.ToList<V>().AsReadOnly(); } }

        /// <summary>
        /// Devuelve la lista de claves del diccionario.
        /// </summary>
        [XmlIgnore]
        public ReadOnlyCollection<K> Keys { get { RebuildInternalDictionary(); return this.Dictionary.Keys.ToList<K>().AsReadOnly(); } }

        /// <summary>
        /// Propiedad por defecto del diccionario.
        /// </summary>
        /// <param name="key">Clave del elemento.</param>
        /// <returns>Devuelve el elemento del diccionario con la clave indicada.</returns>
        /// <remarks>Esta propiedad permite acceder directamente a la lista de elementos pasando como parametro la clave, de igual forma a como se haria con un Dictionary(IDictionary).</remarks>
        [XmlIgnore]
        public V this[K key] { get { RebuildInternalDictionary(); return this.Dictionary[key]; } }

        /// <summary>
        /// Numero de elementos del diccionario.
        /// </summary>
        [XmlIgnore]
        public int Count { get { RebuildInternalDictionary(); return this.Dictionary.Count; } }
        #endregion

        /// <summary>
        /// Reconstruye el diccionario interno si la lista tiene datos.
        /// </summary>
        /// <remarks>Este metodo se implementa para los casos en que la instancia de la clase ha sido generada a 
        /// partir de deseriaizacion ya que el diccionario interno no se reconstruye automaticamente.</remarks>
        private void RebuildInternalDictionary()
        {
            if (this.Dictionary.Count == 0 && this._list != null && this._list.Count > 0)
            {
                foreach (SerializableKeyValuePair kvp in this._list)
                    this.Dictionary.Add(kvp.ToKeyValuePair());
            }
        }

        #region The Proxy List
    /// <summary>
    /// Acceso al diccionario (IDictionary) interno.
    /// </summary>
    [XmlIgnore]
    public IDictionary<K, V> Dictionary { get; internal set; }

    /// <summary>
    /// Holds the keys and values
    /// </summary>
    public class SerializableKeyValuePair
    {
        public K Key { get; set; }
        public V Value { get; set; }
        /// <summary>
        /// Convertir clase a KeyValuePair.
        /// </summary>
        /// <returns>Devuelve una instancia de KeyValuePair para ser usada con el diccionario (IDictionary) interno de la clase.</returns>
        internal KeyValuePair<K, V> ToKeyValuePair()
        {
            return new KeyValuePair<K, V>(this.Key, this.Value);
        }
    }

    // This field will store the deserialized list
    private Collection<SerializableKeyValuePair> _list;    

    /// <remarks>
    /// XmlElementAttribute is used to prevent extra nesting level. It's
    /// not necessary.
    /// </remarks>
    [XmlElement]
    public Collection<SerializableKeyValuePair> KeysAndValues
    {
        get
        {
            if (_list == null)
            {
                _list = new Collection<SerializableKeyValuePair>();
            }

            // On deserialization, Original will be null, just return what we have
            if (Dictionary == null)
            {
                return _list;
            }

            // If Original was present, add each of its elements to the list
            if (this.Dictionary.Count > 0) _list.Clear();
            foreach (var pair in Dictionary)
            {
                _list.Add(new SerializableKeyValuePair { Key = pair.Key, Value = pair.Value });
            }

            return _list;
        }
    }
    #endregion

        /// <summary>
        /// Convenience method to return a dictionary from this proxy instance
        /// </summary>
        /// <returns></returns>
        public Dictionary<K, V> ToDictionary()
        {
            return KeysAndValues.ToDictionary(key => key.Key, value => value.Value);
        }
    }
}