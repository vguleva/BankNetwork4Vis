﻿namespace SimpleBankingModel
{
    public struct Edge
    {
        /// <summary>
        /// Number and type of source node
        /// </summary>
        public string Source;

        /// <summary>
        /// Number and type of target node
        /// </summary>
        public string Target;

        /// <summary>
        /// For banking system model corresponds to the size of lending.
        /// Made integral to avoid lots of small edges. If double, round it!
        /// </summary>
        public int Weight;

        /// <summary>
        /// For banking system model corresponds to the time life of an edge
        /// </summary>
        public int Maturity;

        /// <summary>
        /// The iteration when edge created
        /// </summary>
        internal int Created;

        /// <summary>
        /// The iteration when edge expires
        /// </summary>
        internal int Expires;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">ID of a source node</param>
        /// <param name="target">ID of a target node</param>
        /// <param name="weight">edge weight</param>
        /// <param name="maturity">edge living time</param>
        /// <param name="tim">iteration when the adge was created</param>
        public Edge(string source, string target, int weight, int maturity, int tim)
        {
            Source = source;
            Target = target;
            Weight = weight;
            Maturity = maturity;
            Created = tim;
            Expires = Created + Maturity;
        }
    }

}
