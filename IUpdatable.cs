namespace TLSA.Engine
{
    /// <summary>
    /// Define un objeto actualizable.
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Indica si el objeto esta activado.
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// Actualiza el objeto.
        /// </summary>
        /// <param name="gameTime"></param>
        void Update(Microsoft.Xna.Framework.GameTime gameTime);
    }
}