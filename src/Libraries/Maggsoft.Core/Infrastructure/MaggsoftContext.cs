using System.Runtime.CompilerServices;

namespace Maggsoft.Core.Infrastructure
{
    /// <summary>
    /// Provides access to the singleton instance of the Nop engine.
    /// </summary>
    public class MaggsoftContext
    {
        #region Methods

        /// <summary>
        /// Create a static instance of the Nop engine.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IMaggsoft Create()
        {
            //create NopEngine as engine
            return Singleton<IMaggsoft>.Instance ?? (Singleton<IMaggsoft>.Instance = new MaggsoftEngine());
        }

        /// <summary>
        /// Sets the static engine instance to the supplied engine. Use this method to supply your own engine implementation.
        /// </summary>
        /// <param name="engine">The engine to use.</param>
        /// <remarks>Only use this method if you know what you're doing.</remarks>
        public static void Replace(IMaggsoft engine)
        {
            Singleton<IMaggsoft>.Instance = engine;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton Nop engine used to access Nop services.
        /// </summary>
        public static IMaggsoft Current
        {
            get
            {
                if (Singleton<IMaggsoft>.Instance == null)
                {
                    Create();
                }

                return Singleton<IMaggsoft>.Instance;
            }
        }

        #endregion
    }
}
