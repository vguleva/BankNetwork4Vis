using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankingModel.model
{
    partial class BankingSystem
    {
        /// <summary>
        /// Bank list
        /// </summary>
        internal List<Bank> Banks;
        /// <summary>
        ///  Customer list
        /// </summary>
        internal List<Customer> Customers;
        /// <summary>
        /// Interbank Network Edges
        /// </summary>
        internal List<Edge> IbNetwork;
        /// <summary>
        /// External network edges
        /// </summary>
        internal List<Edge> ENetwork;

        /// <summary>
        /// Current iteration number
        /// </summary>
        private static int curIt = 0;

        void Iteration()
        {
            NewEdgesENetwork();
            NewEdgesINetwork();
            DeleteExpiredEdges();
        }

        private void NewEdgesENetwork()
        {
            var bankChooser = new Random();
            var loanDepo = new Random();

            foreach (var customer in Customers)
            {
                var bankNum = bankChooser.Next(0, Banks.Count);
                var size = EdgeWeight;
                var maturity = Maturity;
                if (loanDepo.NextDouble() < LoanDepoShare)
                    ENetwork.Add(new Edge("b" + bankNum, customer.ID, size, maturity, curIt));
                else
                    ENetwork.Add(new Edge(customer.ID, "b" + bankNum, size, maturity, curIt));
            }
        }

        private void NewEdgesINetwork()
        {
            var bankChooser = new Random();
            foreach (var bank in Banks)
            {
                if (bank.NW > 0) continue;
                var bankNum = bankChooser.Next(0, Banks.Count);
                var size = EdgeWeight;
                var maturity = Maturity;
                IbNetwork.Add(new Edge(bank.ID, "b" + bankNum, size, maturity, curIt));
            }
        }

        private void DeleteExpiredEdges()
        {
            ENetwork.RemoveAll(x => x.Expires == curIt);
            IbNetwork.RemoveAll(x => x.Expires == curIt);
        }
        
    }
}
