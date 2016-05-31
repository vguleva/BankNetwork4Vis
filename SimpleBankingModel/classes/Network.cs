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
    public static class Network
    {
        public static double AverageDegree(IList<Edge> edges){ throw new NotImplementedException();}
        public static double AverageClustering() { throw new NotImplementedException(); }
        public static double AverageShortestPath() { throw new NotImplementedException(); }
        public static double[] LaplacianSpectrum() { throw new NotImplementedException(); }

        public static double Entropy(IEnumerable<double> serie)
        {
            return -serie.Sum(x => x * Math.Log(x));
        }

        #region THERMODYNAMIC FEATURES
        // TODO

        public static int Energy(ICollection<Edge> edges)
        {
            return edges.Count;
        }
        #endregion
    }
}
