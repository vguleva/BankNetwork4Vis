using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        EventInt CurIt = new EventInt(0);// todo start it as a parameter
        internal List<Edge> AllEdgesOverSimulation = new List<Edge>();

        /// <summary>
        /// Set number of current iteration to ZERO, 
        /// and add new customers and banks.
        /// Initialize the system (event subscription)
        /// </summary>
        /// <param name="bankNum"></param>
        /// <param name="custNum"></param>
        internal BankingSystem(int bankNum, int custNum)
        {
            Initialize();

            Banks = new List<Bank>();
            Customers = new List<Customer>();
            CurIt.SetValue(0);
            for(var i = 0; i < custNum; i++)
                Customers.Add(new Customer(i));
            for(var i=0; i < bankNum; i++)
                Banks.Add(new Bank(i));
        }

        /// <summary>
        /// Constructs banking system with banks, customers, and certain initial topology
        /// </summary>
        /// <param name="bankNum"></param>
        /// <param name="custNum"></param>
        /// <param name="ibNetworkEdges"></param>
        internal BankingSystem(int bankNum, int custNum, List<Edge> ibNetworkEdges):base()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The Subscription on events.
        /// Bank prev balance sheet update after each iteration,
        /// ia and il update after ib-network addition and removal of links,
        /// ea and el update after e-network changes,
        /// allEdgesOverSimulation update after the change of each list.
        /// </summary>
        void Initialize()
        {
            CurIt.Incremented += delegate
            {
                foreach (var bank in Banks)
                    bank.UpdatePreviousBalanceSheetValues();
            };//UpdatePreviousBalanceSheets();}
            IbNetwork.OnAdd += delegate(Edge item)
            {
                Banks.First(x => x.ID == item.Source).IA_Plus(item.Weight);
                Banks.First(x => x.ID == item.Target).IL_Plus(item.Weight);
                AllEdgesOverSimulation.Add(item);
            };
            ENetwork.OnAdd += delegate(Edge item) { 
                if (Regex.IsMatch(item.Source, @"b\d+") && Regex.IsMatch(item.Target, @"c\d+"))
                    Banks.First(x => x.ID == item.Source).EA_Plus(item.Weight);
                else if (Regex.IsMatch(item.Source, @"c\d+") && Regex.IsMatch(item.Target, @"b\d+"))
                    Banks.First(x => x.ID == item.Target).EL_Plus(item.Weight);
                AllEdgesOverSimulation.Add(item);
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

        /// <summary>
        /// Increment iteration num. Add new links to IB network, Ext network, delete expired edges
        /// </summary>
        /// <param name="bankPolicy"></param>
        /// <param name="customerPolicy"></param>
        internal void Iteration(Policy bankPolicy, Policy customerPolicy)
        {
            CurIt.Plus();// save current values of bank balance sheets to previous
            NewEdgesENetwork(customerPolicy);
            NewEdgesINetwork(bankPolicy);
            DeleteExpiredEdges();
            // todo insolvent banks action, shock propagation
            // update files with edges over simulation and bank data
        }

        private void NewEdgesENetwork(Policy customerPolicy)
        {
            var loanDepo = new Random();
            foreach (var customer in Customers)
            {
                int bankNum ; ChooseBank(customerPolicy," ",out bankNum);
                var size     = ChooseWeight();
                var maturity = ChooseMaturity();

                if (loanDepo.NextDouble() < LoanDepoShare)
                    ENetwork.Add(new Edge("b" + bankNum, customer.ID, size, maturity, CurIt.ToInt()));
                else
                    ENetwork.Add(new Edge(customer.ID, "b" + bankNum, size, maturity, CurIt.ToInt()));
            }
        }

        private void NewEdgesINetwork(Policy bankPolicy)
        {
            foreach (var bank in Banks)
            {
                if (bank.NW > 0) continue;
                //var assetRequired = bank.NW;
                //for (var i = 0; i <= -2*assetRequired; i++)
                while(bank.NW < 0)    // todo ?? NW<=0
                {
                    int bankNum;
                    ChooseBank(bankPolicy, bank.ID, out bankNum);
                    var cnt = 0;
                    while ("b" + bankNum == bank.ID)
                        if (cnt < 4)
                        {
                            ChooseBank(bankPolicy, bank.ID, out bankNum);
                            cnt++;
                        }
                        else ChooseBank(Policy.R, bank.ID, out bankNum);


                    var size = ChooseWeight(); // TODO size=-NW
                    var maturity = ChooseMaturity();
                    IbNetwork.Add(new Edge(bank.ID, "b" + bankNum, size, maturity, CurIt.ToInt()));
                }
            }
        }

        private void DeleteExpiredEdges()
        {
            ENetwork.RemoveAll(x => x.Expires == CurIt.ToInt());
            IbNetwork.RemoveAll(x => x.Expires == CurIt.ToInt());
        }

        private void UpdatePreviousBalanceSheets()
        {
            foreach (var bank in Banks)
                bank.UpdatePreviousBalanceSheetValues();
        }

        private void ChooseBank(Policy bankPolicy, string bankID, out int bankNum)
        {
            if(bankPolicy==Policy.R)
                bankNum = ChooseBank();
            else if (bankPolicy == Policy.P)
                bankNum = ChooseBank_PreferentiallyAssets();
            else bankNum = ChooseBank_AssortativeAssets(bankID);
            //switch (bankPolicy)
            //{
            //    case Policy.Random:
            //        bankNum = ChooseBank();
            //        break;
            //    case Policy.Preferential:
            //        bankNum = ChooseBankPreferentiallyAssets();
            //        break;
            //    case Policy.Assortative:
            //        bankNum = ChooseBank_AssortativeAssets(bankID);
            //        break;
            //}
        }
        
    }
}
