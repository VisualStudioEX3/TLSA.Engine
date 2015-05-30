using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace TLSA.Engine.Tools.XML
{
    /// <summary>
    /// Clase con funciones auxiliares.
    /// </summary>
    public static class Serializers
    {
        #region Serializadores genericos de System.XML con System.IO
        /// <summary>
        /// Serializa un objeto a un archivo XML.
        /// </summary>
        /// <param name="data">Objeto a serializar a XML.</param>
        /// <param name="filename">Archivo XML a generar.</param>
        public static void SerializeToFile(object data, string filename)
        {
            XmlSerializer mySerializer = new XmlSerializer(data.GetType());
            StreamWriter myWriter = new StreamWriter(filename);
            mySerializer.Serialize(myWriter, data);
            myWriter.Close();
        }

        /// <summary>
        /// Deserializa un archivo XML a objeto.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a deserializar.</typeparam>
        /// <param name="filename">Archivo XML a deserializar.</param>
        /// <returns>Devuelve el contenido del XML en el formato indicado.</returns>
        /// <remarks>Esta funcion permite deserializar un archivo XML desde cualquier ubicacion salvo el Content Manager.</remarks>
        public static T DeserializeFromFile<T>(string filename)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            FileStream myFileStream = new FileStream(filename, FileMode.Open);
            T ret = (T)mySerializer.Deserialize(myFileStream);
            myFileStream.Close();
            return ret;
        }

        /// <summary>
        /// Deserializa un archivo XML a objeto.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a deserializar.</typeparam>
        /// <param name="filename">Archivo XML a deserializar.</param>
        /// <returns>Devuelve el contenido del XML en el formato indicado.</returns>
        /// <remarks>Esta funcion permite deserializar archivos XML desde el Title Storage.</remarks>
        public static T DeserializeFromTitleStorage<T>(string filename)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            Stream stream = TitleContainer.OpenStream(filename);
            T ret = (T)mySerializer.Deserialize(stream);
            stream.Close();
            return ret;
        }
        #endregion
    }
}