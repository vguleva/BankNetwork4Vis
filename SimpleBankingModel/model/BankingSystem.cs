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
        EventInt CurIt = new EventInt(0);// todo start it as a parameter
        internal List<Edge> AllEdgesOverModeling = new List<Edge>();

        internal BankingSystem(int banksNum, int custNum)
        {
            Banks = new List<Bank>();
            Customers = new List<Customer>();
            CurIt.SetValue(0);
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
            CurIt.Incremented += delegate()
            {
                foreach (var bank in Banks)
                    bank.UpdatePreviousBalanceSheetValues();
            };//UpdatePreviousBalanceSheets();}
            IbNetwork.OnAdd += delegate(Edge item)
            {
                Banks.First(x => x.ID == item.Source).IA_Plus(item.Weight);
                Banks.First(x => x.ID == item.Target).IL_Plus(item.Weight);
                AllEdgesOverModeling.Add(item);
            };
            ENetwork.OnAdd += delegate(Edge item) { 
                if (Regex.IsMatch(item.Source, @"b\d+") && Regex.IsMatch(item.Target, @"c\d+"))
                    Banks.First(x => x.ID == item.Source).EA_Plus(item.Weight);
                else if (Regex.IsMatch(item.Source, @"c\d+") && Regex.IsMatch(item.Target, @"b\d+"))
                    Banks.First(x => x.ID == item.Target).EL_Plus(item.Weight);
                AllEdgesOverModeling.Add(item);
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
        /// <param name="defaultMaturity"></param>
        internal void Iteration(Policy bankPolicy, Policy customerPolicy, int defaultMaturity)
        {
            CurIt.Plus();// write current values to previous
            NewEdgesENetwork(customerPolicy, defaultMaturity);
            NewEdgesINetwork(bankPolicy, defaultMaturity);
            DeleteExpiredEdges();
            // todo insolvent banks action, shock propagation
        }

        private void NewEdgesENetwork(Policy customerPolicy, int defaultMaturity)
        {
            var loanDepo = new Random();
            foreach (var customer in Customers)
            {
                int bankNum ; ChooseBank(customerPolicy," ",out bankNum);
                var size     = ChooseWeight();
                var maturity = ChooseMaturity(defaultMaturity);

                if (loanDepo.NextDouble() < LoanDepoShare)
                    ENetwork.Add(new Edge("b" + bankNum, customer.ID, size, maturity, CurIt.ToInt()));
                else
                    ENetwork.Add(new Edge(customer.ID, "b" + bankNum, size, maturity, CurIt.ToInt()));
            }
        }

        private void NewEdgesINetwork(Policy bankPolicy, int defaultMaturity)
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
                    var maturity = ChooseMaturity(defaultMaturity);
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
