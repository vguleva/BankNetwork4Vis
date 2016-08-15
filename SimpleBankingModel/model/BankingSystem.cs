﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleBankingModel.classes;
using SimpleBankingModel.interfaces;

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
        readonly EventInt _curIt = new EventInt(0);// todo start it as a parameter
        internal List<Edge> AllEdgesOverSimulation = new List<Edge>();

        /// <summary>
        /// Set number of current iteration to ZERO, 
        /// and add new customers and banks.
        /// Initialize the system (event subscription).
        /// Empty init graph 
        /// </summary>
        /// <param name="bankNum"></param>
        /// <param name="custNum"></param>
        internal BankingSystem(int bankNum, int custNum)
        {
            Initialize();

            Banks = new List<Bank>();
            Customers = new List<Customer>();
            _curIt.SetValue(0);
            for(var i = 0; i < custNum; i++)
                Customers.Add(new Customer(i));
            for(var i=0; i < bankNum; i++)
                Banks.Add(new Bank(i));
        }

        /// <summary>
        /// Constructs banking system with banks, customers, and certain initial topology.
        /// For the given graph type and model parameters generates list of edges with NetworkX software,
        /// process them, and add to interbank network edges with weight of 1, maturity of 3, creation date of 0,
        /// and marked for banks source and target nodes.
        /// </summary>
        /// <param name="bankNum"></param>
        /// <param name="custNum"></param>
        /// <param name="graphType">Graph of sertain type and input constructor parameters</param>
        internal BankingSystem(int bankNum, int custNum, IGraph graphType)
        {
            Initialize();

            Banks = new List<Bank>();
            Customers = new List<Customer>();
            _curIt.SetValue(0);
            for (var i = 0; i < custNum; i++)
                Customers.Add(new Customer(i));
            for (var i = 0; i < bankNum; i++)
                Banks.Add(new Bank(i));

            var graphEdges = graphType.Generate();
            foreach (var edge in graphEdges)
            {
                IbNetwork.Add(edge);
            }
            //IbNetwork.AddRange();
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
            _curIt.Incremented += delegate
            {
                foreach (var bank in Banks)
                    bank.UpdatePreviousBalanceSheetValues();
            };//UpdatePreviousBalanceSheets();}
            IbNetwork.OnAdd += delegate(Edge item)
            {
                if (item.Source[0] != 'b' || item.Target[0]!='b')
                    throw new Exception("Unable to add B2C-edge to IB network");
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
            _curIt.Plus();// save current values of bank balance sheets to previous
            NewEdgesENetwork(customerPolicy);
            NewEdgesINetwork(bankPolicy);
            DeleteExpiredEdges();
        }

        internal void Iteration(Policy bankPolicy, Policy customerPolicy, 
              IComparer<Edge> rewiringComparatorA, IComparer<Edge> rewiringComparatorL)
        {
            Iteration(bankPolicy, customerPolicy);
            foreach (var bank in Banks)
            {
                if (bank.NW < 0) DeleteNode(bank.ID, rewiringComparatorA, rewiringComparatorL);
            }
        }

        private void NewEdgesENetwork(Policy customerPolicy)
        {
            var loanDepo = new Random();
            foreach (var customer in Customers)
            {
                int bankNum ; ChooseBank(customerPolicy," ",out bankNum);
                var size     = ChooseWeight();
                var maturity = ChooseMaturity();

                ENetwork.Add(loanDepo.NextDouble() < LoanDepoShare
                    ? new Edge("b" + bankNum, customer.ID, size, maturity, _curIt.ToInt())
                    : new Edge(customer.ID, "b" + bankNum, size, maturity, _curIt.ToInt()));
            }
        }

        private void NewEdgesINetwork(Policy bankPolicy)
        {
            foreach (var bank in Banks)
            {
                if (bank.NW > 0) continue;
                while(bank.NW < 0)    
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
                    IbNetwork.Add(new Edge(bank.ID, "b" + bankNum, size, maturity, _curIt.ToInt()));
                }
            }
        }

        private void DeleteExpiredEdges()
        {
            ENetwork.RemoveAll(x => x.Expires == _curIt.ToInt());
            IbNetwork.RemoveAll(x => x.Expires == _curIt.ToInt());
        }

        /// <summary>
        /// The deletion of bank having this ID according to this policy of edges rewiring.
        /// Assets and liabilities are sorted according to the corresponding rules,
        /// after which src and trg are linked according to queue.
        /// New edges take minimum expire date and minimum weight. The rest of edge weight is for the further counter-party
        /// </summary>
        /// <param name="nodeId">The ID of a bank-node for deletion</param>
        /// <param name="rewiringComparatorA">The method of eliminated bank assets sorting</param>
        /// <param name="rewiringComparatorL">The method of eliminated bank liabilities sorting</param>
        public void DeleteNode(string nodeId, IComparer<Edge> rewiringComparatorA, IComparer<Edge> rewiringComparatorL)
        {
            // form lists of assets and liabilities for an excluded bank
            var assets = new List<Edge>();
            assets.AddRange(ENetwork.Where (x => x.Source == nodeId));
            assets.AddRange(IbNetwork.Where(x => x.Source == nodeId));
            ENetwork.RemoveAll (x => x.Source == nodeId); // remove cur assets and liabilities from ib- and e- networks
            IbNetwork.RemoveAll(x => x.Source == nodeId);
            var liabilities = new List<Edge>();
            liabilities.AddRange(ENetwork.Where (x=>x.Target==nodeId));
            liabilities.AddRange(IbNetwork.Where(x=>x.Target==nodeId));
            ENetwork.RemoveAll (x => x.Target == nodeId); // remove cur assets and liabilities from ib- and e- networks
            IbNetwork.RemoveAll(x => x.Target == nodeId);

            // sort formed list according to POLICY
            assets.Sort(rewiringComparatorA);      //TODO
            liabilities.Sort(rewiringComparatorL); //TODO
            // add result edges to the system
            while (assets.Count > 0 && liabilities.Count > 0)
            {
                var newSource = liabilities[0].Source;
                var newTarget = assets[0].Target;
                var newWeight = Math.Min(assets[0].Weight, liabilities[0].Weight);
                var newExpires = Math.Min(assets[0].Expires, liabilities[0].Expires);
                var newMaturity = newExpires - _curIt.ToInt();
                var newEdge = new Edge(newSource, newTarget, newWeight, newMaturity, _curIt.ToInt());
                
                if (newSource[0] == 'b' && newTarget[0] == 'b')
                    IbNetwork.Add(newEdge);
                else
                    ENetwork.Add(newEdge);
                assets[0].Weight=assets[0].Weight - newWeight;
                liabilities[0].Weight=liabilities[0].Weight - newWeight;

                if (assets[0].Weight == 0)      
                    assets.RemoveAt(0);
                if (liabilities[0].Weight == 0) 
                    liabilities.RemoveAt(0);
            }
        }

        public void InfusionFund(string nodeId, int infusionSize)
        {
            throw new NotImplementedException();
        }

        private void ChooseBank(Policy bankPolicy, string bankId, out int bankNum)
        {
            if(bankPolicy==Policy.R)
                bankNum = ChooseBank();
            else if (bankPolicy == Policy.P)
                bankNum = ChooseBank_PreferentiallyAssets();
            else bankNum = ChooseBank_AssortativeAssets(bankId);
        }
        
    }
}
