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
            return Banks.Average(x => x.Velocity);
        }
        /// <summary>
        /// \sigma^2=N^{-1}*(\sum_{i}{(average-i)^2})
        /// </summary>
        /// <returns></returns>
        double VarianceVelocity()
        {
            return Banks.Average(x => Math.Pow(x.Velocity - MeanVelocity(), 2));
        }
        double MeanRemoteness()
        {
            return Banks.Average(x => x.Remoteness);
        }
        double VarianceRemoteness()
        {
            return Banks.Average(x => Math.Pow(x.Remoteness - MeanRemoteness(), 2));
        }
        double NegativeVelocityShare()
        {
            return (double)Banks.Count(x => x.Velocity < 0)/Banks.Count;
        }
        double T_ThreatenedSetCardinality(int T)
        {
            return Banks.Count(x => x.NW > Bank.DefaultValueNW && x.Velocity < 0 && x.Remoteness < T);
        }
        #endregion
        #region TOPOLOGY+NODE DYNAMICS
        /// <summary>
        /// Undirected edge feature.
        /// If edge expires before some bank go bankrupt,
        /// imply positive effect, otherwise, negative.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private double EdgePotential(Edge edge)
        {
            var m = edge.Maturity;
            var r1 = Banks.First(x => x.ID == edge.Source).Remoteness;
            var r2 = Banks.First(x => x.ID == edge.Target).Remoteness;
            if (m < r1 && m < r2)
                return 1;
            return -1;
        }
        private double WeightedEdgePotential(Edge edge)
        {
            var m = edge.Maturity;
            var r1 = Banks.First(x => x.ID == edge.Source).Remoteness;
            var r2 = Banks.First(x => x.ID == edge.Target).Remoteness;
            var v1 = Banks.First(x => x.ID == edge.Source).Velocity;
            var v2 = Banks.First(x => x.ID == edge.Target).Velocity;
            var s1 = Banks.First(x => x.ID == edge.Source).NW;
            var s2 = Banks.First(x => x.ID == edge.Target).NW;

            if (m < r1 && m < r2)
                return RelativeGain(edge.Weight,s1, s2);
            if (r1 < r2)
                return RelativeLoss(v1, s2, edge.Weight);
            return RelativeLoss(v2, s1, edge.Weight);
        }
        
        internal double MeanWeightedPotential()
        {
            if (IbNetwork.Count == 0) return 0;
            return IbNetwork.Average(x => WeightedEdgePotential(x));
        }
        internal double VarianceWeightedPotential()
        {
            if (IbNetwork.Count == 0) return 0;
            /*
            var sum = .0;//IbNetwork.Sum(x => Math.Sqrt(EdgePotential(x) - MeanWeightedPotential()));
            foreach (var edge in IbNetwork)
            {
                sum += Math.Pow(EdgePotential(edge) - MeanWeightedPotential(),2);
            }
            return (double)sum/IbNetwork.Count;
             */
            return IbNetwork.Average(x => Math.Pow(EdgePotential(x) - MeanWeightedPotential(), 2));
        }
        /// <summary>
        /// Summarize all potentials of edges and
        /// </summary>
        /// <returns></returns>
        internal double MeanUnwaightedPotential()
        {
            if (IbNetwork.Count == 0) return 0;
            return IbNetwork.Average(x => EdgePotential(x));
        }
        internal double VarianceUnweightedPotential()
        {
            if (IbNetwork.Count == 0) return 0;
            return IbNetwork.Average(x=>Math.Pow(EdgePotential(x)-MeanUnwaightedPotential(), 2));
        }
        
        /// <summary>
        /// Sum all negative potentials, and divide by the number of edges.
        /// </summary>
        /// <returns>Share of negative potentials</returns>
        internal double NegativeUnweightedPotentialShare()
        {
            if (IbNetwork.Any(x => EdgePotential(x) < 0))
                return (double)IbNetwork.Count(x => EdgePotential(x) < 0)/IbNetwork.Count;
            return 0;
        }

        /// <summary>
        /// Possibly positive impact of an edge
        /// to the states of adjacent nodes.
        /// Combined from sum of profits of both nodes.
        /// If states are positive, relative gain is evaluated.
        /// If some state is negative, gain to be taken as an edge weight 
        /// </summary>
        /// <param name="edgeWeight"></param>
        /// <param name="node1State"></param>
        /// <param name="node2State"></param>
        /// <returns>edgeWeight*(1/node1State + 1/node2State)</returns>
        private double RelativeGain(int edgeWeight, int node1State, int node2State)
        {
            int addend1;
            int addend2;
            
            if (node1State <= 0)
                addend1 = edgeWeight;
            else addend1 = edgeWeight/node1State;
            
            if (node2State <= 0)
                addend2 = edgeWeight;
            else addend2 = edgeWeight/node2State;

            return addend1 + addend2;
        }
        /// <summary>
        /// Possibly negative impact of an edge
        /// to the states of adjacent nodes.
        /// It is combined edge weight and expected distance to be undergoed with velocity in 1 iteration.
        /// If state is negative then loss is taken as absolute value of sum of velosity abs and weight.
        /// </summary>
        /// <param name="worseNodeVelocity"></param>
        /// <param name="betterNodeState"></param>
        /// <param name="edgeWeight"></param>
        /// <returns>If node state is positive then (worseNodeVelocity + edgeWeight) / betterNodeState; 
        /// othewise, worseNodeVelocity + edgeWeight</returns>
        private double RelativeLoss(double worseNodeVelocity, double betterNodeState, int edgeWeight)
        {
            if (betterNodeState > 0)
                return (worseNodeVelocity + edgeWeight)/betterNodeState;
            return (worseNodeVelocity + edgeWeight);
        }
        #endregion
        #region TOPOLOGY
        // associated with Network
        #endregion

        internal string GetSystemState()
        {
            IEnumerable<double> features = new List<double>()
            {
                /* 0*/CurIt.ToInt(),
                /* 1*/MeanVelocity(),
                /* 2*/VarianceVelocity(),
                /* 3*/MeanRemoteness(),
                /* 4*/VarianceRemoteness(),
                /* 5*/NegativeVelocityShare(),
                /* 6*/T_ThreatenedSetCardinality(50),
                /* 7*/T_ThreatenedSetCardinality(400),
                /* 8*/MeanUnwaightedPotential(),
                /* 9*/VarianceUnweightedPotential(),
                /*10*/Math.Round(MeanWeightedPotential(), 3),
                /*11*/Math.Round(VarianceWeightedPotential(), 3),
                /*12*/NegativeUnweightedPotentialShare()
            };
            
            return String.Join(";", features);
        }
    }
}
