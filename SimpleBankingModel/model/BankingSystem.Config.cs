using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankingModel.model
{
    partial class BankingSystem
    {
        #region Customer parameters (how customer create edges)
        /// <summary>
        /// Customer probabilistic parameter of requesting a loan.
        /// If random is less than Share then loan, 
        /// else it is deposit.
        /// </summary>
        const double LoanDepoShare = .5;
        #endregion

        #region Bank parameters (how bank create edges)
        #endregion

        #region Model simplification: edges default 
        // todo  embed as edge parameters by default, using config
        /// <summary>
        /// The way of choosing an edge weight
        /// </summary>
        private const int EdgeWeight = 1;
        /// <summary>
        /// The way of choosing an edge lifetime
        /// </summary>
        private const int Maturity = 1;
        #endregion
    }
}
