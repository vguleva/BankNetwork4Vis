using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SimpleBankingModel.classes;

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
        /// Interbank Network Edges.
        /// Both source and target vertexes are banks
        /// </summary>
        internal EventList<Edge> IbNetwork = new EventList<Edge>();
        /// <summary>
        /// External network edges.
        /// One of vertexes is customer
        /// </summary>
        internal EventList<Edge> ENetwork = new EventList<Edge>();

        /// <summary>
        /// Current iteration number
        /// </summary>
        private static EventInt _curIt = new EventInt(0);// todo start it as a parameter

        internal BankingSystem(int banksNum, int custNum)
        {
            Banks = new List<Bank>();
            Customers = new List<Customer>();

            for(var i = 0; i < custNum; i++)
                Customers.Add(new Customer(i));
            for(var i=0; i < banksNum; i++)
                Banks.Add(new Bank(i));
            
            Initialize();
        }

        /// <summary>
        /// Subscribe on events
        /// </summary>
        void Initialize()
        {
            _curIt.Incremented += delegate()
            {
                foreach (var bank in Banks)
                    bank.UpdatePreviousBalanceSheetValues();
            };//UpdatePreviousBalanceSheets();}
            IbNetwork.OnAdd += delegate(Edge item)
            {
                Banks.First(x => x.ID == item.Source).IA_Plus(item.Weight);
                Banks.First(x => x.ID == item.Target).IL_Plus(item.Weight);
            };
            ENetwork.OnAdd += delegate(Edge item) { 
                if (Regex.IsMatch(item.Source, @"b\d+") && Regex.IsMatch(item.Target, @"c\d+"))
                    Banks.First(x => x.ID == item.Source).EA_Plus(item.Weight);
                else if (Regex.IsMatch(item.Source, @"c\d+") && Regex.IsMatch(item.Target, @"b\d+"))
                    Banks.First(x => x.ID == item.Target).EL_Plus(item.Weight);
            };
            IbNetwork.OnRemove += delegate(Edge item)
            {
                Banks.First(x => x.ID == item.Source).IA_Minus(item.Weight);
                Banks.First(x => x.ID == item.Target).IL_Minus(item.Weight);
            };
            ENetwork.OnRemove += delegate(Edge item)
            {
                if (Regex.IsMatch(item.Source, @"b\d+") && Regex.IsMatch(item.Target, @"c\d+"))
                    Banks.First(x => x.ID == item.Source).EA_Minus(item.Weight);
                else if (Regex.IsMatch(item.Source, @"c\d+") && Regex.IsMatch(item.Target, @"b\d+"))
                    Banks.First(x => x.ID == item.Target).EL_Minus(item.Weight);
            };
        }

        internal void Iteration()
        {
            NewEdgesENetwork();
            NewEdgesINetwork();
            DeleteExpiredEdges();
            // todo insolvent banks action, shock propagation
            _curIt.Plus();
        }

        private void NewEdgesENetwork()
        {
            var bankChooser = new Random();
            var loanDepo = new Random();

            foreach (var customer in Customers)
            {
                var bankNum  = ChooseBank();
                var size     = ChooseWeight();
                var maturity = ChooseMaturity();

                if (loanDepo.NextDouble() < LoanDepoShare)
                    ENetwork.Add(new Edge("b" + bankNum, customer.ID, size, maturity, _curIt.ToInt()));
                else
                    ENetwork.Add(new Edge(customer.ID, "b" + bankNum, size, maturity, _curIt.ToInt()));
            }
        }

        private void NewEdgesINetwork()
        {
            var bankChooser = new Random();
            foreach (var bank in Banks)
            {
                if (bank.NW > 0) continue;
                for (var i = 0; i < -bank.NW; i++)
                {
                    var bankNum = ChooseBank();
                    var size = ChooseWeight(); // TODO size=-NW
                    var maturity = ChooseMaturity();
                    IbNetwork.Add(new Edge(bank.ID, "b" + bankNum, size, maturity, _curIt.ToInt()));
                }
            }
        }

        private void DeleteExpiredEdges()
        {
            ENetwork.RemoveAll(x => x.Expires == _curIt.ToInt());
            IbNetwork.RemoveAll(x => x.Expires == _curIt.ToInt());
        }

        private void UpdatePreviousBalanceSheets()
        {
            foreach (var bank in Banks)
                bank.UpdatePreviousBalanceSheetValues();
        }
    }
}
