using System;

namespace Hapi.Example
{
    /// <summary>
    /// Class representing Public Enenmy.
    /// </summary>
    public class PublicEnemy
    {
        /// <summary>
        /// Creates a <see cref="PublicEnemy"/>.
        /// </summary>
        public PublicEnemy()
        {
            IsColdLampin = false;
        }

        /// <summary>
        /// Asks Flavor Flav what time it is.
        /// </summary>
        /// <returns></returns>
        public DateTime WhatTimeIsItFlav()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Gets wether PE are Cold Lampin.
        /// </summary>
        public bool IsColdLampin { get; private set; }
    }
}
