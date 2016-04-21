namespace SimpleBankingModel.model
{
    class Bank
    {
        /// <summary>
        /// Structure: "b0".
        /// As different types of nodes to be hold in a network
        /// </summary>
        internal string ID; // todo event for fulfilling when edge is added to list
        // values of balance sheet are integral as edges' weights are integral
        /// <summary>
        /// Interbank Assets
        /// </summary>
        private int IA;
        /// <summary>
        /// Interbank Liabilities
        /// </summary>
        private int IL;
        /// <summary>
        /// External Assets
        /// </summary>
        private int EA;
        /// <summary>
        /// External Liabilities
        /// </summary>
        private int EL;

        /// <summary>
        /// Net Worth
        /// </summary>
        internal int NW
        {
            get { return EA + IA - EL - IL; }
        }
    }
}
