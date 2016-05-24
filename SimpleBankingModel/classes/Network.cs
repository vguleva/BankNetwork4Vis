using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Specialized;
using System.Data.Odbc;
//using Financial_Network_Simulation.math;
//using Financial_Network_Simulation.structs;
//using IronPython.Modules;

namespace SimpleBankingModel.classes
{
    static class Network
    {
        static double AverageDegree(IList<Edge> edges) { }
        static double AverageClustering() { }
        static double AverageShortestPath() { }
        static double[] LaplacianSpectrum() { }

        static double Entropy(IEnumerable<double> serie)
        {
            return -serie.Sum(x => x * Math.Log(x));
        }
    }
    
    
    class Network_
    {
        
        /*************************************************************************
         *  NETWORK STRUCTURE FEATURES
         *************************************************************************
         */

        #region Energy

        internal double Energy1
        {
// for reaction for NW
            get
            {
                var res = .0;
                if (_previousAFs == null)
                {
                    SavePreviousState();
                    return 0;
                }
                foreach (var bank in Banks.Where(b=>!DeadList.Contains(b.Id)))
                {
                    // add value of change node state if 
                    if (bank.NetWorth < 0 && _previousNWs[bank.Id] >= 0)
                        res += Math.Abs(bank.NetWorth - _previousNWs[bank.Id]);
                    foreach (var rec in _prevLiaListDistincted[bank.Id])
                    {
                        if (bank.NetWorth < 0 &&
                            bank.NetWorth + Banks[rec.BankAssignment].NetWorth /*_previousNWs[rec.BankAssignment]*/< 0)
                            res += bank.NetWorth + _previousNWs[rec.BankAssignment];
                    }
                }
                //SavePreviousState();
                return res;
            }
        }

        internal double Energy1norm
        {
// for reaction for NW
            get { return Energy1/(Banks.Count - DeadList.Count); }
        }

        internal double Energy1bare
        {
// for reaction for NW
            get
            {
                var res = .0;
                if (_previousAFs == null)
                {
                    SavePreviousState();
                    return 0;
                }
                foreach (var bank in Banks)
                {
                    //res += Math.Abs(bank.NetWorth - previousNWs[bank.Id]);
                    foreach (var rec in _prevLiaListDistincted[bank.Id])
                    {
                        if (bank.NetWorth < 0 &&
                            bank.NetWorth + Banks[rec.BankAssignment].NetWorth /*_previousNWs[rec.BankAssignment]*/< 0)
                            res += bank.NetWorth + _previousNWs[rec.BankAssignment];
                    }
                }
                return res;
            }
        }

        internal double Energy2 // for reaction for AF
        {
            get
            {
                var res = .0;
                if (_previousAFs == null)
                {
                    SavePreviousState();
                    return 0;
                }
                foreach (var bank in Banks)
                {
                    if (bank.AvailableFunds < 0 && _previousAFs[bank.Id] >= 0)
                        res += Math.Abs(bank.AvailableFunds - _previousAFs[bank.Id]);
                    foreach (var rec in _prevAssListDistincted[bank.Id])
                    {
                        if (bank.AvailableFunds < 0 &&
                            bank.AvailableFunds + Banks[rec.BankAssignment].AvailableFunds
                                /* _previousAFs[rec.BankAssignment]*/< 0)
                            res += bank.AvailableFunds + _previousAFs[rec.BankAssignment];
                    }
                }
                return res;
            }
        }

        internal double Energy2norm // for reaction for AF
        {
            get { return Energy2/(Banks.Count - DeadList.Count); }
        }

        internal double Energy2bare // for reaction for AF
        {
            get
            {
                var res = .0;
                if (_previousAFs == null)
                {
                    SavePreviousState();
                    return 0;
                }
                foreach (var bank in Banks)
                {
                    //res += Math.Abs(bank.AvailableFunds - previousAFs[bank.Id]);
                    foreach (var rec in _prevAssListDistincted[bank.Id])
                    {
                        if (bank.AvailableFunds < 0 &&
                            bank.AvailableFunds + Banks[rec.BankAssignment].AvailableFunds
                                /*_previousAFs[rec.BankAssignment]*/< 0)
                            res += bank.AvailableFunds + _previousAFs[rec.BankAssignment];
                    }
                }
                return res;
            }
        }

        internal double Entropy
        {
            get
            {
                var sumWithDegrees =
                    Banks.Where(bank1 => (bank1.IntAssList.Count + bank1.IntLiaList.Count) > 0).Sum(
                        bank1 =>
                            Banks.Where(
                                bank2 =>
                                    (bank2.IntAssList.Count + bank2.IntLiaList.Count) > 0 &&
                                    bank2.Neighbours.Contains(bank1.Id)).Sum(
                                        bank2 =>
                                            1.0/
                                            ((bank1.IntAssList.Count + bank1.IntLiaList.Count)* // bank1 degree
                                             (bank2.IntAssList.Count + bank2.IntLiaList.Count)))); // bank2 degree
                return 1 - 1/NumberOfNodes - sumWithDegrees/(NumberOfNodes*NumberOfNodes);
            }
        }
        /*
        internal double Entropy1
        {
            get
            {
                
            }
        }
         */

        /// <summary>
        /// Number of edges
        /// </summary>
        internal int Energy
        {
            get
            {
                var curNumEdges = Banks.Sum(bank => bank.IntAssList.Count());
                return curNumEdges;
            }
        }


        internal double Temperature // todo test: * 1000
        {
            get
            {
                return (Entropy - _prevEntropy)*1000/(Energy - _prevNumEdges);
                if (_previousDegrees == null)
                {
                    SavePreviousState();
                    return 0;
                }
                var deltaE = Energy - _prevNumEdges + 0.001;

                var sigma = .0;
                foreach (var bank1 in Banks)
                {
                    foreach (var bank2 in Banks)
                    {
                        if (bank1.Neighbours.Contains(bank2.Id) && bank1.IntAssList.Count + bank1.IntLiaList.Count > 0 &&
                            bank2.IntAssList.Count + bank2.IntLiaList.Count > 0 && _previousDegrees[bank1.Id] > 0 &&
                            _previousDegrees[bank2.Id] > 0)
                        {
                            sigma += 1/
                                     ((bank1.IntAssList.Count + bank1.IntLiaList.Count)*
                                      (bank2.IntAssList.Count + bank2.IntLiaList.Count)) -
                                     1/(_previousDegrees[bank1.Id]*_previousDegrees[bank2.Id]);
                        }
                    }
                }
                return sigma/deltaE;
            }
        }

        #endregion

        internal double AverageDegree
        {
            get
            {
                var degreeList = new List<double>();
                for (var i = 0; i < NumberOfNodes; i++)
                    if (!DeadList.Contains(i))
                        degreeList.Add(GraphBidirected.Count(x => x.Source == i));
                return degreeList.Sum()/degreeList.Count;
            }
        }

        internal double AverageWeightedDegree_ { get; set; }
        
        private IEnumerable<Edge> GraphBidirected
        {
            get
            {
                var result = new HashSet<Edge>();
                foreach (var bank in Banks)
                {
                    foreach (var investmentInfo in bank.IntAssList)
                    {
                        result.Add(new Edge(Math.Min(bank.Id, investmentInfo.BankAssignment),
                            Math.Max(bank.Id, investmentInfo.BankAssignment)));
                        result.Add(new Edge(Math.Max(bank.Id, investmentInfo.BankAssignment),
                            Math.Min(bank.Id, investmentInfo.BankAssignment)));
                    }
                    foreach (var investmentInfo in bank.IntLiaList)
                    {
                        result.Add(new Edge(Math.Min(bank.Id, investmentInfo.BankAssignment),
                            Math.Max(bank.Id, investmentInfo.BankAssignment)));
                        result.Add(new Edge(Math.Max(bank.Id, investmentInfo.BankAssignment),
                            Math.Min(bank.Id, investmentInfo.BankAssignment)));
                    }

                }
                return result;
            }

        }

        private HashSet<Edge> Graph
        {
            get
            {
                var result = new HashSet<Edge>();
                foreach (var bank in Banks)
                {
                    foreach (var investmentInfo in bank.IntAssList)
                        result.Add(new Edge(Math.Min(bank.Id, investmentInfo.BankAssignment),
                            Math.Max(bank.Id, investmentInfo.BankAssignment)));
                    foreach (var investmentInfo in bank.IntLiaList)
                        result.Add(new Edge(Math.Min(bank.Id, investmentInfo.BankAssignment),
                            Math.Max(bank.Id, investmentInfo.BankAssignment)));
                }
                return result;
            }
        }

        /* **************************************************************************
         * NETWORK NODE STATES DYNAMIC FEATURES
         * **************************************************************************/
           #region
        /// <summary>
        /// Number of iterations until first bankruptcy as expected.
        /// Taken as distance between current bank state and "infected" state, divided by velocity of state change.
        /// Velocity is defined as the difference of current and previous state. 
        /// In more general case, velocity is change of state per iteration. Negative velocity correspondes to shift to "infected" state.
        /// State is represented by bank's AvailebleFunds.
        /// Returns -1, if all agents has positive velocities
        /// </summary>
        internal double ExpectedIterationWhenFirstBankruptcyAF
        {
            get
            {
                try
                {
                    return
                        Banks.Where(
                            b =>
                                !DeadList.Contains(b.Id) && b.AvailableFunds > 0 &&
                                b.AvailableFunds < _previousAFs[b.Id])
                            .Min(b => Math.Abs(b.AvailableFunds/(b.AvailableFunds - _previousAFs[b.Id])));
                }
                catch (InvalidOperationException e)
                {
                    Log.Error(e.StackTrace);
                    return -1;
                }
            }
        }

        /// <summary>
        /// Number of iterations until first bankruptcy as expected.
        /// Taken as distance between current bank state and "infected" state, divided by velocity of state change.
        /// Velocity is defined as the difference of current and previous state. 
        /// In more general case, velocity is change of state per iteration. Negative velocity correspondes to shift to "infected" state.
        /// State is represented by bank's NetWorth
        /// </summary>
        internal double ExpectedIterationWhenFirstBankruptcyNW
        {
            get
            {
                try
                {
                    return
                        Banks.Where(b => !DeadList.Contains(b.Id) && b.NetWorth > 0 && b.NetWorth < _previousNWs[b.Id])
                            .Min(b => Math.Abs(b.NetWorth/(b.NetWorth - _previousNWs[b.Id])));
                }
                catch (InvalidOperationException e)
                {
                    Log.Error(e.StackTrace);
                    return -1;
                }
            }
        }

        internal double ExpectedIterationWhenFirstBankruptcy
        {
            get
            {
                try
                {
                    if (ExpectedIterationWhenFirstBankruptcyAF == -1) return ExpectedIterationWhenFirstBankruptcyNW;
                    if (ExpectedIterationWhenFirstBankruptcyNW == -1) return ExpectedIterationWhenFirstBankruptcyAF;
                    return Math.Min(ExpectedIterationWhenFirstBankruptcyAF, ExpectedIterationWhenFirstBankruptcyNW);
                }
                catch (InvalidOperationException e)
                {
                    Log.Error(e.StackTrace);
                    return -1;
                }
            }
        }

        private readonly double _permissibleBankruptsShare;

        /// <summary>
        /// Expected iteration when number of bankrupts will be _permissibleBankruptsShare.
        /// Distances before bankruptcies are sorted, the appropriate is returned.
        /// State is represented by bank's AF
        /// </summary>
        internal double ExpectedIterationWhenShareOfBankruptsAF
        {
            get
            {
                if (
                    Banks.Count(
                        b => !DeadList.Contains(b.Id) && b.AvailableFunds > 0 && b.AvailableFunds < _previousAFs[b.Id]) <
                    _permissibleBankruptsShare*NumberOfNodes)
                    return -1;
                var sortedDistances =
                    Banks.Where(
                        b => !DeadList.Contains(b.Id) && b.AvailableFunds > 0 && b.AvailableFunds < _previousAFs[b.Id])
                        .Select(b => Math.Abs(b.AvailableFunds/(b.AvailableFunds - _previousAFs[b.Id]))).ToList();
                sortedDistances.Sort();
                // sortedDistances.Reverse();
                for (var i = 1; i < _permissibleBankruptsShare*NumberOfNodes; i++)
                    sortedDistances.RemoveAt(0);
                return sortedDistances[0];
            }
        }

        /// <summary>
        /// Expected iteration when number of bankrupts will be _permissibleBankruptsShare.
        /// Distances before bankruptcies are sorted, the appropriate is returned.
        /// State is represented by bank's NW
        /// </summary>
        internal double ExpectedIterationWhenShareOfBankruptsNW
        {
            get
            {
                if (Banks.Count(b => !DeadList.Contains(b.Id) && b.NetWorth > 0 && b.NetWorth < _previousNWs[b.Id]) <
                    _permissibleBankruptsShare*NumberOfNodes)
                    return -1;
                var sortedDistances =
                    Banks.Where(b => !DeadList.Contains(b.Id) && b.NetWorth > 0 && b.NetWorth < _previousNWs[b.Id])
                        .Select(b => Math.Abs(b.NetWorth/(b.NetWorth - _previousNWs[b.Id]))).ToList();
                sortedDistances.Sort();
                // sortedDistances.Reverse();
                for (var i = 1; i < _permissibleBankruptsShare*NumberOfNodes; i++)
                    sortedDistances.RemoveAt(0);
                return sortedDistances[0];
            }
        }

        internal double ExpectedIterationWhenShareOfBankrupts{
            get
            {
                // check that there are going to be the expected number of bankrupts
                if (Banks.Count(b => !DeadList.Contains(b.Id) && 
                        (
                            b.NetWorth > 0 && b.NetWorth < _previousNWs[b.Id]
                            || 
                            b.AvailableFunds > 0 && b.AvailableFunds < _previousAFs[b.Id]
                        )
                    ) < _permissibleBankruptsShare * NumberOfNodes)
                    return -1;
                var sortedDistances =
                    Banks.Where(b => !DeadList.Contains(b.Id) && 
                                     (
                                         b.NetWorth > 0 && b.NetWorth < _previousNWs[b.Id]
                                         ||
                                         b.AvailableFunds > 0 && b.AvailableFunds < _previousAFs[b.Id]
                                     )
                        )
                        .Select(b => Math.Min
                                     (
                                         Math.Abs(b.NetWorth / (b.NetWorth - _previousNWs[b.Id])), 
                                         Math.Abs(b.AvailableFunds/ (b.AvailableFunds - _previousAFs[b.Id]))
                                     )
                        )
                        .ToList(); // add distances for banks having both AF & NW decreased
                sortedDistances.Sort();
                for (var i = 1; i < _permissibleBankruptsShare * NumberOfNodes; i++)
                    sortedDistances.RemoveAt(0);
                return sortedDistances[0];
            }
        }
    

        /// <summary>
        /// Expected number of iterations after first bankruptcy before futher share of bankruptcies.
        /// State of node is represented by AF. 
        /// </summary>
        internal double ExpectedWindowAfterFirstBankruptcyAF{
            get { return ExpectedIterationWhenShareOfBankruptsAF - ExpectedIterationWhenFirstBankruptcyAF; }
        }

        /// <summary>
        /// Expected number of iterations after first bankruptcy before futher share of bankruptcies.
        /// State of node is represented by NW. 
        /// </summary>
        internal double ExpectedWindowAfterFirstBankruptcyNW 
        {
            get { return ExpectedIterationWhenShareOfBankruptsNW - ExpectedIterationWhenFirstBankruptcyNW; }
        }
        /// <summary>
        /// Number of nodes having decreasing states.
        /// States are represented by AF
        /// </summary>
        internal double NegativeVelocityShareAF {
            get { return (double)Banks.Count(b => !DeadList.Contains(b.Id) && b.AvailableFunds > 0 && b.AvailableFunds < _previousAFs[b.Id])/(NumberOfNodes-DeadList.Count); }
        }
        /// <summary>
        /// Number of nodes having decreasing states.
        /// States are represented by NW
        /// </summary>
        internal double NegativeVelocityShareNW
        {
            get { return (double)Banks.Count(b=>!DeadList.Contains(b.Id) && b.NetWorth > 0 && b.NetWorth < _previousNWs[b.Id]) / (NumberOfNodes - DeadList.Count); }
        }

        internal double NegativeVelocityShare
        {
            get
            {
                return
                    (double)
                        Banks.Count(
                            b =>
                                !DeadList.Contains(b.Id) && b.AvailableFunds > 0 && b.NetWorth > 0 &&
                                (b.AvailableFunds < _previousAFs[b.Id] || b.NetWorth < _previousNWs[b.Id]))/
                    (NumberOfNodes - DeadList.Count);
            }
        }
        /// <summary>
        /// Number of nodes having negative velocity and 
        /// distance before bankruptcy is less than some fixed value (iterForBankruptcy)
        /// </summary>
        /// <param name="iterForBankruptcy">The upper bound for number of iterations before expected bankruptcy</param>
        /// <returns>Share of such nodes, normalized for number of alive nodes</returns>
        internal double NegativeDangerousVelocityShare(int iterForBankruptcy)
        {
            return
                    (double)
                        Banks.Count(
                            b =>
                                !DeadList.Contains(b.Id) && b.AvailableFunds > 0 && b.NetWorth > 0 &&
                                (
                                    b.AvailableFunds < _previousAFs[b.Id] 
                                        && Math.Abs(b.AvailableFunds / (b.AvailableFunds - _previousAFs[b.Id])) < iterForBankruptcy 
                                    ||
                                    b.NetWorth < _previousNWs[b.Id] 
                                        && Math.Abs(b.NetWorth / (b.NetWorth - _previousNWs[b.Id])) < iterForBankruptcy
                                )
                            )/(NumberOfNodes - DeadList.Count);
        }
        internal double NegativeVelocityShareAFtest
        {
            get { return (double)Banks.Count(b => !DeadList.Contains(b.Id) && b.AvailableFunds < _previousAFs[b.Id]) / (NumberOfNodes - DeadList.Count); }
        }
        /// <summary>
        /// Number of nodes having decreasing states.
        /// States are represented by NW
        /// </summary>
        internal double NegativeVelocityShareNWtest
        {
            get { return (double)Banks.Count(b => !DeadList.Contains(b.Id) && b.NetWorth < _previousNWs[b.Id]) / (NumberOfNodes - DeadList.Count); }
        }

        internal double NegativeVelocityShareTest
        {
            get
            {
                return
                    (double)
                        Banks.Count(
                            b =>
                                !DeadList.Contains(b.Id) &&
                                (b.AvailableFunds < _previousAFs[b.Id] || b.NetWorth < _previousNWs[b.Id])) /
                                (NumberOfNodes - DeadList.Count);
            }
        }

        /// <summary>
        /// Number of banks tending to become "infected"
        /// with expected distances, which are less than questioned
        /// </summary>
        /// <param name="iters">number of iterations from now to iteration of interest</param>
        /// <returns>Share of bankrupts are expected in iters, normalized for initial number of nodes</returns>
        internal int ExpectedBankruptsShare(int iters)
        {
            return ExpectedAbsoluteBankruptsNumber(iters)/NumberOfNodes;
        }
        /// <summary>
        /// Number of banks tending to become "infected"
        /// with expected distances, which are less than questioned
        /// </summary>
        /// <param name="iters">number of iterations from now to iteration of interest</param>
        /// <returns>Share of bankrupts are expected in iters, normalized for number of "alive" nodes</returns>
        internal int ExpectedBankruptsShareAliveNormed(int iters)
        {
            return ExpectedAbsoluteBankruptsNumber(iters)/(NumberOfNodes - DeadList.Count);
        }
        /// <summary>
        /// Absolute number of still "susceptible" banks are expected to become "infected" until given iter.
        /// </summary>
        /// <param name="iters"></param>
        /// <returns></returns>
        private int ExpectedAbsoluteBankruptsNumber(int iters)
        {
            var tmp = Banks.Count(
                b => !DeadList.Contains(b.Id) && b.AvailableFunds > 0 && b.NetWorth > 0 &&
               (
                    (
                        b.AvailableFunds < _previousAFs[b.Id] 
                        && 
                        b.AvailableFunds/(b.AvailableFunds - _previousAFs[b.Id]) < iters
                    ) 
                    ||
                    (
                        b.NetWorth < _previousNWs[b.Id] 
                        && 
                        b.NetWorth/(b.NetWorth - _previousNWs[b.Id]) < iters
                    )
               ));
            return tmp;

        }
#endregion
        /****************************************************
         * NODE-STATE ORIENTED FEATURE
         * **************************************************/

        internal double DynamicsOfStateNodeInteractionAF
        {
            get
            {
                double tmp = 0;
                var    cnt = 0;
                foreach (var bank in Banks.Where(b=>!DeadList.Contains(b.Id)))
                {
                    foreach (var rec in bank.IntAssList)
                    {
                        var velocity1 = bank.AvailableFunds - _previousAFs[bank.Id];
                        if (Math.Abs(velocity1) < 1) continue;
                        var velocity2 = Banks[rec.BankAssignment].AvailableFunds - _previousAFs[rec.BankAssignment];
                        if (Math.Abs(velocity2) < 1) continue;
                        var h1 = Math.Abs(bank.AvailableFunds/velocity1);
                        var h2 = Math.Abs(Banks[rec.BankAssignment].AvailableFunds/velocity2);
                        var weight = rec.InvestmentSize;
                        var length = rec.DateOfRepayment - TimeInModel;
                        var tmpInf = InteractionPotential(velocity1, velocity2, h1, h2, weight, length);
                        //if(tmpInf > 1) throw new Exception();
                        tmp += tmpInf;
                        cnt ++;
                    }
                }    
                return tmp/cnt;
            }
        }
        internal double DynamicsOfStateNodeInteractionNW
        {
            get
            {
                double tmp = 0;
                var    cnt = 0;
                foreach (var bank in Banks.Where(b=>!DeadList.Contains(b.Id)))
                {
                    foreach (var rec in bank.IntAssList)
                    {
                        var velocity1 = bank.NetWorth - _previousNWs[bank.Id];
                        if (Math.Abs(velocity1) < 1) continue;
                        var velocity2 = Banks[rec.BankAssignment].NetWorth - _previousNWs[rec.BankAssignment];
                        if (Math.Abs(velocity2) < 1) continue;
                        var h1 = Math.Abs(                     bank.AvailableFunds / velocity1);
                        var h2 = Math.Abs(Banks[rec.BankAssignment].AvailableFunds / velocity2);
                        var weight = rec.InvestmentSize;
                        var length = rec.DateOfRepayment - TimeInModel;
                        tmp += InteractionPotential(velocity1, velocity2, h1, h2, weight, length);
                        cnt++;
                    }
                }
                return tmp/cnt;
            }
        }
        internal double DynamicOfNodeStatesAF_v
        {
            get
            {
                return Banks.Where(b=>!DeadList.Contains(b.Id)&& b.AvailableFunds>0).Sum(bank => bank.AvailableFunds - _previousAFs[bank.Id])/(NumberOfNodes-DeadList.Count);
            }
        }
        internal double DynamicOfNodeStatesAF_h
        {
            get
            {
                double tmp = 0;
                foreach (var bank in Banks.Where(b => !DeadList.Contains(b.Id) && b.AvailableFunds > 0))
                {
                    var velocity = bank.AvailableFunds - _previousAFs[bank.Id]; // can be negative
                    if (Math.Abs(velocity) < 1)continue;
                    var h = bank.AvailableFunds / Math.Abs(velocity); // can be negative
                    tmp += h;
                }
                return tmp / (NumberOfNodes-DeadList.Count);
            }
        }
        internal double DynamicOfNodeStatesNW_v
        {
            get
            {
                return Banks.Where(b => !DeadList.Contains(b.Id) && b.NetWorth > 0).Sum(bank => bank.NetWorth - _previousNWs[bank.Id]) / (NumberOfNodes-DeadList.Count);
            }
        }
        internal double DynamicOfNodeStatesNW_h
        {
            get
            {
                double tmp = 0;
                foreach (var bank in Banks.Where(b => !DeadList.Contains(b.Id) && b.NetWorth > 0))
                {
                    var velocity = bank.NetWorth - _previousNWs[bank.Id]; // can be negative
                    if (Math.Abs(velocity) < 1) continue;
                    var h = bank.NetWorth / Math.Abs(velocity); // can be negative
                    tmp += h;
                }
                return tmp / (NumberOfNodes-DeadList.Count);
            }
        }

        internal double SumDynamicsAF_v{get { return DynamicsOfStateNodeInteractionAF + DynamicOfNodeStatesAF_v; }}
        internal double SumDynamicsAF_h{get { return DynamicsOfStateNodeInteractionAF + DynamicOfNodeStatesAF_h; }}
        internal double SumDynamicsNW_v{get { return DynamicsOfStateNodeInteractionNW + DynamicOfNodeStatesNW_v; }}
        internal double SumDynamicsNW_h{get { return DynamicsOfStateNodeInteractionNW + DynamicOfNodeStatesNW_h; }}


        private double InteractionPotential(double v1, double v2, double h1, double h2, double weight, int length)
        {
            if (length <= Math.Min(h1, h2))
                return InteractionAddendWhenNoBnkrptExpected(v1, v2, weight, length);
            else
                if (v1 < 0 && v2 < 0)
                    if (h1 < h2) // then v1 is going to bankruptcy first
                        return InteractionAddendWhenBnkrptExpected(v1, v2, h1, weight);
                    else
                        return InteractionAddendWhenBnkrptExpected(v2, v1, h2, weight);
                else return InteractionAddendWhenNoBnkrptExpected(v1, v2, weight, length);   
        }
        /// <summary>
        /// For state-oriented energy. Potential interaction result.
        /// Case: agent reach bnkrptc earlier than the edge disappears.
        /// l > min(h1, h2), h2>h1
        /// </summary>
        /// <param name="v1">velocity of agent going to bankruptcy</param>
        /// <param name="v2">velocity of other agent</param>
        /// <param name="h1">remoteness of agent going to bankruptcy</param>
        /// <param name="weight">edge weight</param>
        /// <returns></returns>
        private double InteractionAddendWhenBnkrptExpected(double v1, double v2, double h1, double weight)
        {
            if (v1 == 0 || v2 == 0) return 0;
            return -(Math.Abs(v1) + weight)/(h1*Math.Abs(v2));
        }

        /// <summary>
        /// For state-oriented energy. Potential interaction result.
        /// Case: the edge disappears before agent reaches bnkrptc.
        /// min(h1, h2) >= l, h2>h1. 
        /// Also the same for: v1, v2>0 => h1, h2>l. 
        /// Also if v1>0, h2>l. Imply immunity,  if v>0
        /// </summary>
        /// <param name="v1">velocity of agent going to bankruptcy</param>
        /// <param name="v2">velocity of other agent</param>
        /// <param name="weight">edge weight</param>
        /// <param name="length">edge length</param>
        /// <returns></returns>
        private double InteractionAddendWhenNoBnkrptExpected(double v1, double v2, double weight, int length)
        {
            if (v1 == 0 || v2 == 0) return 0;
            return weight*(1/Math.Abs(v1) + 1/Math.Abs(v2))/(length + 1);
        }
        /****/
        internal double DynamicsOfStateNodeInteractionAF_n
        {
            get
            {
                double tmp = 0;
                var cnt = 0;
                foreach (var bank in Banks.Where(b => !DeadList.Contains(b.Id)))
                {
                    foreach (var rec in bank.IntAssList)
                    {
                        var velocity1 = bank.AvailableFunds - _previousAFs[bank.Id];
                        if (Math.Abs(velocity1) < 1) continue;
                        var velocity2 = Banks[rec.BankAssignment].AvailableFunds - _previousAFs[rec.BankAssignment];
                        if (Math.Abs(velocity2) < 1) continue;
                        var h1 = Math.Abs(bank.AvailableFunds / velocity1);
                        var h2 = Math.Abs(Banks[rec.BankAssignment].AvailableFunds / velocity2);
                        var weight = rec.InvestmentSize;
                        var length = rec.DateOfRepayment - TimeInModel;
                        var tmpInf = interactionPotential_n(velocity1, velocity2, h1, h2, weight, length);
                        //if(tmpInf > 1) throw new Exception();
                        tmp += tmpInf;
                        cnt++;
                    }
                }
                return tmp / cnt;
            }
        }
        internal double DynamicsOfStateNodeInteractionNW_n
        {
            get
            {
                double tmp = 0;
                var cnt = 0;
                foreach (var bank in Banks.Where(b => !DeadList.Contains(b.Id)))
                {
                    foreach (var rec in bank.IntAssList)
                    {
                        var velocity1 = bank.NetWorth - _previousNWs[bank.Id];
                        if (Math.Abs(velocity1) < 1) continue;
                        var velocity2 = Banks[rec.BankAssignment].NetWorth - _previousNWs[rec.BankAssignment];
                        if (Math.Abs(velocity2) < 1) continue;
                        var h1 = Math.Abs(bank.AvailableFunds / velocity1);
                        var h2 = Math.Abs(Banks[rec.BankAssignment].AvailableFunds / velocity2);
                        var weight = rec.InvestmentSize;
                        var length = rec.DateOfRepayment - TimeInModel;
                        tmp += interactionPotential_n(velocity1, velocity2, h1, h2, weight, length);
                        cnt++;
                    }
                }
                return tmp / cnt;
            }
        }
        private double interactionPotential_n(double v1, double v2, double h1, double h2, double weight, int length)
        {
            if (length <= Math.Min(h1, h2))
                return InteractionAddendWhenNoBnkrptExpected_n(v1, v2, weight, length);
            else
                if (v1 < 0 && v2 < 0)
                    if (h1 < h2) // then v1 is going to bankruptcy first
                        return InteractionAddendWhenBnkrptExpected_n(v1, v2, h1, weight);
                    else
                        return InteractionAddendWhenBnkrptExpected_n(v2, v1, h2, weight);
                else return InteractionAddendWhenNoBnkrptExpected_n(v1, v2, weight, length);
        }
        private double InteractionAddendWhenBnkrptExpected_n(double v1, double v2, double h1, double weight)
        {
            if (v1 == 0 || v2 == 0) return 0;
            return -(Math.Abs(v1) + weight) / (/*h1**/Math.Abs(v2));
        }
        private double InteractionAddendWhenNoBnkrptExpected_n(double v1, double v2, double weight, int length)
        {
            if (v1 == 0 || v2 == 0) return 0;
            return weight * (1 / Math.Abs(v1) + 1 / Math.Abs(v2))/*/(length + 1)*/;
        }

        /*****************************************************************************************************/
        /*                                                                                                   */
        /*****************************************************************************************************/

        /*
        /// <summary>
        /// Independent on the 
        /// среднее по всем банкам от такой штуки: \sum{IA_i/A * P(contagion_i)}
        /// </summary>
        internal double MeanProbabilisticStability
        {
            get
            {
                var bankRiskList = new List<double>(); // список значений риска для каждого банка
                foreach (var bank in Banks.Where(bank => !DeadList.Contains(bank.Id)))
                {
                    var sum = .0;
                    foreach (var rec in bank.IntAssList)
                    {
                        sum += rec.InvestmentSize/bank.Assets*ContagionProbability(Banks[rec.BankAssignment], 3);
                    }
                    bankRiskList.Add(sum);
                }
                var tmpsum = bankRiskList.Sum();
                var tmpres = tmpsum / bankRiskList.Count();
                return tmpres;//bankRiskList.Sum()/bankRiskList.Count();
            }
        }
         */

        /// <summary>
        /// Смотрим рёбра, время истечения которых лежит в срезе [begin, end]
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        internal double MeanProbabilisticStability(int begin, int end)
        {
            var bankRiskList = new List<double>(); // список значений риска для каждого банка
                foreach (var bank in Banks.Where(bank => !DeadList.Contains(bank.Id)))
                {
                    var sum = .0;
                    foreach (var rec in bank.IntAssList.Where(x=>x.DateOfRepayment - TimeInModel >= begin && x.DateOfRepayment - TimeInModel < end))
                    {
                        sum += rec.InvestmentSize/bank.Assets*ContagionProbability(Banks[rec.BankAssignment], 1, begin, end);
                    }
                    bankRiskList.Add(sum);
                }
                var tmpsum = bankRiskList.Sum();
                var tmpres = tmpsum / bankRiskList.Count();
                return tmpres;//bankRiskList.Sum()/bankRiskList.Count();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bank"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private double ContagionProbability(Bank bank, int depth, int begin, int end)
        {
            var sum = .0;
            if (depth == 0)
            {
                sum += bank.IntAssList.Where(x=>x.DateOfRepayment - TimeInModel >= begin && x.DateOfRepayment - TimeInModel < end).Sum(rec => rec.InvestmentSize/bank.Assets);
                return sum/2; // принимаем вероятность падения ссылки за 1/2
            }
            sum +=
                bank.IntAssList.Where(x=>x.DateOfRepayment - TimeInModel >= begin && x.DateOfRepayment - TimeInModel < end).Sum(
                    rec => rec.InvestmentSize/bank.Assets*ContagionProbability(Banks[rec.BankAssignment], depth - 1, begin, end));
            return sum;
        }

        /// <summary>
        /// Run python script evaluating topology features
        /// </summary>
        /// <param name="pathToReadGraph">bla-bla\multiplexPerformance\Layer...\Tim.file</param>
        /// <param name="pathToTopologyFeatures">bla-bla\TopologyFeatures\TiM.file</param>
        internal void WriteTopologyFeatures(string pathToReadGraph, string pathToTopologyFeatures, IEnumerable<string> layerNames)
        {
            if (!Directory.Exists(pathToTopologyFeatures))
                Directory.CreateDirectory(pathToTopologyFeatures); // check path to graph topology features
            if (IsDebugEnabled) Log.Debug("Write topology features");
                    
            Parallel.ForEach(layerNames, layerName =>
            {
                Log.Debug("Run topology feature writing for layer: " + layerName);
                NetworkX.EvalTopologyFeatures(Path.Combine(pathToReadGraph,        layerName, TimeInModel.ToString()+".csv"),
                                              Path.Combine(pathToTopologyFeatures, layerName, TimeInModel.ToString()));
            });
            /*
            foreach (var layerName in layerNames)
                NetworkX.EvalTopologyFeatures(Path.Combine(pathToReadGraph,        layerName, TimeInModel.ToString()),
                                              Path.Combine(pathToTopologyFeatures, layerName, TimeInModel.ToString()));
             */
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathToTopologyFeatures"></param>
        /// <param name="shortNameForAveragesFile"></param>
        /// <param name="iter">Number of iterations (beginning from current) for proposals about future bankruptcies</param>
        internal void WriteFinancialFeatures(string pathToTopologyFeatures, string shortNameForAveragesFile, int iter)
        {
            Log.Debug("Append non-topology features");
            var dirName = Path.Combine(pathToTopologyFeatures, "AggregatedLayerIB" /*layerName*/,
                TimeInModel.ToString());
            Directory.CreateDirectory(dirName);
            if (!File.Exists(Path.Combine(dirName, shortNameForAveragesFile)))
                File.Create(Path.Combine(dirName, shortNameForAveragesFile)).Close();
            using (var writer =
                        File.AppendText(Path.Combine(pathToTopologyFeatures, "AggregatedLayerIB" /*layerName*/,
                            TimeInModel.ToString(), shortNameForAveragesFile)))
            {
                writer.WriteLine("Energy12norm: " + (Energy1norm + Energy2norm));
                // THREATENED NODES
                /*02*/writer.WriteLine("NegativeDangerousVelocityShare(400): " + String.Format("{0:F}", NegativeDangerousVelocityShare(400)));
                /*03*/writer.WriteLine("NegativeDangerousVelocityShare(200): " + String.Format("{0:F}", NegativeDangerousVelocityShare(200)));
                /*04*/writer.WriteLine("NegativeDangerousVelocityShare(50): "  + String.Format("{0:F}", NegativeDangerousVelocityShare(50)));
                /*05*/writer.WriteLine("NegativeDangerousVelocityShare(300): " + String.Format("{0:F}", NegativeDangerousVelocityShare(300)));
                /*06*/writer.WriteLine("NegativeDangerousVelocityShare(100): " + String.Format("{0:F}", NegativeDangerousVelocityShare(100)));
                      writer.WriteLine("SumDynamicsAF_v: " + String.Format("{0:F}", SumDynamicsAF_v));
                      writer.WriteLine("SumDynamicsAF_h: " + String.Format("{0:F}", SumDynamicsAF_h));
                      writer.WriteLine("SumDynamicsNW_v: " + String.Format("{0:F}", SumDynamicsNW_v));
                      writer.WriteLine("SumDynamicsNW_h: " + String.Format("{0:F}", SumDynamicsNW_h));
                      writer.WriteLine("DynamicOfNodeStatesAF_v: " + String.Format("{0:F}", DynamicOfNodeStatesAF_v));
                      writer.WriteLine("DynamicOfNodeStatesAF_h: " + String.Format("{0:F}", DynamicOfNodeStatesAF_h));
                      writer.WriteLine("DynamicOfNodeStatesNW_v: " + String.Format("{0:F}", DynamicOfNodeStatesNW_v));
                      writer.WriteLine("DynamicOfNodeStatesNW_h: " + String.Format("{0:F}", DynamicOfNodeStatesNW_h));
                // POTENTIAL OF INTERACTION
                /*15*/writer.WriteLine("DynamicsOfStateNodeInteractionAF: "   + String.Format("{0:F}", DynamicsOfStateNodeInteractionAF));
                /*16*/writer.WriteLine("DynamicsOfStateNodeInteractionNW: "   + String.Format("{0:F}", DynamicsOfStateNodeInteractionNW));
                /*17*/writer.WriteLine("DynamicsOfStateNodeInteractionAF_n: " + String.Format("{0:F}", DynamicsOfStateNodeInteractionAF_n));
                /*18*/writer.WriteLine("DynamicsOfStateNodeInteractionNW_n: " + String.Format("{0:F}", DynamicsOfStateNodeInteractionNW_n));             
                /*19*/writer.WriteLine("Energy: " + String.Format("{0:F}", Energy));
                /*20*/writer.WriteLine("Entropy: " + String.Format("{0:F8}", Math.Round(Entropy, 8)));
                /*21*/writer.WriteLine("Temperature: " + String.Format("{0:F8}", Math.Round(Temperature,8)));
                //SavePreviousState();// for energy right eval
                writer.WriteLine("MaxNetworth: " + Math.Round(MaxNetworth));
                writer.WriteLine("MaxAssets: " + Math.Round(MaxAssets));
                writer.WriteLine("MinNetworth: " + Math.Round(MinNetworth));
                writer.WriteLine("MinAssets: " + Math.Round(MinAssets));
                writer.WriteLine("AverageNetworth: " + Math.Round(AverageNetworth));
                writer.WriteLine("AverageAssets: " + Math.Round(AverageAssets));
                /*28*/writer.WriteLine("ReborrowsCnt: " + ReborrowCounter); ReborrowCounter = 0;
                /*29*/writer.WriteLine("Bankrupts: " + DeadList.Count);
                writer.WriteLine("Average asset quality: "   + Math.Round(Banks.Average(x => x.AssetsGeneralValue(TimeInModel))));
                writer.WriteLine("Average capital quality: " + Math.Round(Banks.Average(x => x.CapitalGeneralValue(TimeInModel))));
                writer.WriteLine("Average liquidity: "       + Math.Round(Banks.Average(x => x.LiquidityGeneralValue(TimeInModel))));
                writer.WriteLine("Average yield: "           + Math.Round(Banks.Average(x => x.YieldGeneralValue(TimeInModel))));
            }
            
            #region for layers
            /*
            // to averages for whole network --- append to existing average file
            var pIIB = Path.Combine(pathToTopologyFeatures, "LayerInstantIB", TimeInModel.ToString(),
                shortNameForAveragesFile);
            if (File.Exists(pIIB))
                using (var writer = File.AppendText(pIIB))
                {
                    writer.WriteLine("Energy1: " + Energy1);
                    writer.WriteLine("ProbStabCoeff: " +
                                     Math.Round(1000000*MeanProbabilisticStability(0, 2), 2)
                                         .ToString()
                                         .Replace(",", "."));
                }
            if (File.Exists(Path.Combine(pathToTopologyFeatures, "LayerShortTermIB", TimeInModel.ToString(), shortNameForAveragesFile)))
                using (
                    var writer =
                        File.AppendText(Path.Combine(pathToTopologyFeatures, "LayerShortTermIB", TimeInModel.ToString(),
                            shortNameForAveragesFile)))
                {
                    writer.WriteLine("Energy1: " + Energy1);
                    writer.WriteLine("ProbStabCoeff: " +
                                     Math.Round(1000000*MeanProbabilisticStability(2, 30), 2)
                                         .ToString()
                                         .Replace(",", "."));
                }
            if (File.Exists(Path.Combine(pathToTopologyFeatures, "LayerLongTermIB", TimeInModel.ToString(), shortNameForAveragesFile)))
                using (var writer = File.AppendText(Path.Combine(pathToTopologyFeatures, "LayerLongTermIB", TimeInModel.ToString(), shortNameForAveragesFile)))
                {
                    writer.WriteLine("Energy1: " + Energy1);
                    writer.WriteLine("ProbStabCoeff: " + Math.Round(1000000 * MeanProbabilisticStability(30, 500), 2).ToString().Replace(",", "."));
                }
            
             */
            #endregion
            #region FOR SINGLE BANKS
            //////////////// add to (*) for appending source lines with stabCoeffWriter strings
            var nodeFeatFileName = Path.Combine(pathToTopologyFeatures, "AggregatedLayerIB", TimeInModel.ToString(),
                "nodesFeatures");
            if (!File.Exists(nodeFeatFileName))
                File.Create(nodeFeatFileName).Close();
            var resLines = new List<string>();
            var startLines =
                File.ReadAllLines(nodeFeatFileName);
            if (startLines.Count() != 0)
                foreach (var line in startLines)
                {
                    var id = line.Split('\t',' ')[0];
                    var stabCapital = Math.Round(Banks[Int32.Parse(id)].CapitalGeneralValue(TimeInModel), 2);
                    var stabAssets = Math.Round(Banks[Int32.Parse(id)].AssetsGeneralValue(TimeInModel), 2);
                    var stabLiquidity = Math.Round(Banks[Int32.Parse(id)].LiquidityGeneralValue(TimeInModel), 2);
                    var stabYield = Math.Round(Banks[Int32.Parse(id)].YieldGeneralValue(TimeInModel), 2);
                    var a = Math.Round(Banks[Int32.Parse(id)].Assets, 2);
                    var nw = Math.Round(Banks[Int32.Parse(id)].NetWorth, 2);
                    var af = Math.Round(Banks[Int32.Parse(id)].AvailableFunds, 1);
                    resLines.Add(String.Concat(line, "\t" + stabCapital +
                                                     "\t" + stabAssets +
                                                     "\t" + stabLiquidity +
                                                     "\t" + stabYield +
                                                     "\t" + a +
                                                     "\t" + nw +
                                                     "\t" + af));
                }
            else
                resLines.AddRange(from bank in Banks.Where(x=>!DeadList.Contains(x.Id))
                    let id = bank.Id
                    let stabCapital = Math.Round(bank.CapitalGeneralValue(TimeInModel), 2)
                    let stabAssets = Math.Round(bank.AssetsGeneralValue(TimeInModel), 2)
                    let stabLiquidity = Math.Round(bank.LiquidityGeneralValue(TimeInModel), 2)
                    let stabYield = Math.Round(bank.YieldGeneralValue(TimeInModel), 2)
                    let a = Math.Round(bank.Assets, 2)
                    let nw = Math.Round(bank.NetWorth, 2)
                    let af = Math.Round(bank.AvailableFunds, 1)
                    select
                        String.Concat(id,
                            "\t" + stabCapital + "\t" + stabAssets + "\t" + stabLiquidity + "\t" + stabYield + "\t" + a +
                            "\t" + nw + "\t" + af));
            File.WriteAllLines(Path.Combine(pathToTopologyFeatures, "AggregatedLayerIB", TimeInModel.ToString(),
                    "nodesFeatures"), resLines);
            #endregion
            // list of bankrupts
            if (!Directory.Exists("bankrupts\\"))
                Directory.CreateDirectory("bankrupts\\");
            using (var writer = new StreamWriter("bankrupts\\" + TimeInModel))//timeString))
                foreach (var i1 in DeadList)
                    writer.WriteLine(i1);

            // list of success
            if (!Directory.Exists("remoteness\\"))
                Directory.CreateDirectory("remoteness\\");
            using (var writer = new StreamWriter("remoteness\\" + TimeInModel.ToString("D4")))//timeString))
                foreach (var bank in Banks.Where(x=>!DeadList.Contains(x.Id)))
                    writer.WriteLine(bank.Id+"\t"+Math.Min(bank.RemotenessNW,bank.RemotenessAF));
            
        }

        internal void WriteNodeStates(string statesDir)
        {
            Log.Debug("Append non-topology features");
            if (!Directory.Exists(statesDir))
                Directory.CreateDirectory(statesDir);
            var fileName = Path.Combine(statesDir,TimeInModel.ToString("D4"));
            if (!File.Exists(fileName))
                File.Create(fileName).Close();
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var bank in Banks.Where(x => !DeadList.Contains(x.Id)))
                {
                    writer.WriteLine(bank.Id + ";" + bank.NW
                                             + ";" + bank.IA
                                             + ";" + bank.EA
                                             + ";" + bank.IL 
                                             + ";" + bank.EL );
                }    
            }
            
        }
    }
}
