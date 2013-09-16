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
        /// Creates the song line for your personal Uzi.
        /// </summary>
        /// <param name="uziWeightInPounds">Weight in pounds of your Uzi.</param>
        /// <returns>Returns string that you can sing about your Uzi.</returns>
        public string CreateUziLine(decimal uziWeightInPounds)
        {
            if (uziWeightInPounds == 2240) return "My Uzi weighs a (UK) ton.";
            return uziWeightInPounds > 2240
                ? "My Uzi is heavier than a (UK) ton!."
                : "I don't want to talk about my Uzi.";
        }

        /// <summary>
        /// Gets wether PE are Cold Lampin.
        /// </summary>
        public bool IsColdLampin { get; private set; }
    }
}
