using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankingModel.model
{
    partial class BankingSystem
    {
        #region NODES STATES DYNAMICS

        /// <summary>
        /// N^{-1}*\sum{i}
        /// </summary>
        /// <returns></returns>
        double MeanVelocity()
        {
            foreach (var bank in Banks)
            {
                
            }
            return Banks.Average(x => x.Velocity);
        }
        /// <summary>
        /// \sigma^2=N^{-1}*(\sum_{i}{(average-i)^2})
        /// </summary>
        /// <returns></returns>
        double VarianceVelocity(){}

        #endregion
        #region TOPOLOGY+NODE DYNAMICS

        private double EdgePotential(Edge edge)
        {
            
        }

        internal double AverageWeightedPotential()
        {
            
        }
        internal double AverageUnwaightedPotential(){}
        
        #endregion
        #region TOPOLOGY
        double AverageDegree(){}
        double AverageClustering(){}
        double AverageShortestPath(){}
        double[] LaplacianSpectrum(){}

        double Entropy(IEnumerable<double> serie)
        {
            return - serie.Sum(x => x*Math.Log(x));
        }
        #endregion

        /// <summary>
        /// Potential profit and risk of edge.
        /// Eval velocity and remoteness of both nodes,
        /// and potential positive or negative influence of edge presence
        /// </summary>
        internal double PotentialOfInteraction
        {
            get
            {
                double tmp = 0;
                var cnt = 0;
                foreach (var rec in IbNetwork)
                {
                    var velocity1 = bank.NetWorth - _previousNWs[bank.Id];
                    if (Math.Abs(velocity1) < 1) continue;
                    var velocity2 = Banks[rec.BankAssignment].NetWorth - _previousNWs[rec.BankAssignment];
                    if (Math.Abs(velocity2) < 1) continue;
                    var h1 = Math.Abs(bank.AvailableFunds/velocity1);
                    var h2 = Math.Abs(Banks[rec.BankAssignment].AvailableFunds/velocity2);
                    var weight = rec.InvestmentSize;
                    var length = rec.DateOfRepayment - TimeInModel;
                    tmp += InteractionPotential(velocity1, velocity2, h1, h2, weight, length);
                    cnt++;
                }

                return tmp / cnt;
            }
        }
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
        
        internal double DynamicOfNodeStatesNW_v
        {
            get
            {
                return Banks.Where(b => !DeadList.Contains(b.Id) && b.NetWorth > 0).Sum(bank => bank.NetWorth - _previousNWs[bank.Id]) / (NumberOfNodes - DeadList.Count);
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
                return tmp / (NumberOfNodes - DeadList.Count);
            }
        }

    }
}
