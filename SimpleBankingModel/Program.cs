using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using IronPython.Runtime;
using log4net;
using SimpleBankingModel.classes;
using SimpleBankingModel.model;

namespace SimpleBankingModel
{
    internal enum Policy
    {
        [Description("r")]
        R/*andom*/,
        [Description("p")]
        P/*referential*/,
        [Description("a")]
        A/*ssortative*/
    }
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        const int bankNum = 100;
        private const int custNum = 1000;
        const int maxIter = 300;
        private const string LaunchDataDir  = @"d:\Valenitna\JExp\res_full_rew_data\";//null_model\";
        private const string PathData = @"d:\Valenitna\JExp\test_regul_tune";//data2";//res_full_nodes";
        private const string PathAverageData = @"d:\Valenitna\JExp\test_regul_tune_averages";
        // private const string pathBinningData = @"d:\Valenitna\JExp\data_binning";

        private static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            #region multiple launches for diff policies combinations (enumerate policies)
            //var bankPolicies = new List<Policy>() { Policy.R };//, Policy.P, Policy.A };
            //var custPolicies = new List<Policy>() { Policy.R };//, Policy.P };
            //var maturities = new List<int>() { 1, 5, 9 };
            //for (var i = 30; i < 100; i++)
            //    //var i = 0;
            //    foreach (var bPol in bankPolicies)
            //        foreach (var custPolicy in custPolicies)
            //        {
            //            Log.Info("Launch:" + i + "; bPol:" + bPol + "; cPol:" + custPolicy);
            //            //foreach (var maturity in maturities)
            //            Launch(bPol, custPolicy, 3 /*maturity*/, i);
            //        }
            #endregion

            #region launching changeable policies
            //for (var i = 26; i < 27; i++)
            //{
            //    Log.Info("Launch:" + i + "; PPA, 25");
            //    Launch(Policy.P, Policy.P, 3, i, Policy.A, 25);
            //    Log.Info("Launch:" + i + "; PPA, 50");
            //    Launch(Policy.P, Policy.P, 3, i, Policy.A, 50);
            //    Log.Info("Launch:" + i + "; PPA, 100");
            //    Launch(Policy.P, Policy.P, 3, i, Policy.A, 100);

            //    Log.Info("Launch:" + i + "; PPR, 25");
            //    Launch(Policy.P, Policy.P, 3, i, Policy.R, 25);
            //    Log.Info("Launch:" + i + "; PPR, 50");
            //    Launch(Policy.P, Policy.P, 3, i, Policy.R, 50);
            //    Log.Info("Launch:" + i + "; PPR, 100"); 
            //    Launch(Policy.P, Policy.P, 3, i, Policy.R, 100);

            //    Log.Info("Launch:" + i + "; APR, 25");
            //    Launch(Policy.A, Policy.P, 3, i, Policy.R, 25);
            //    Log.Info("Launch:" + i + "; APR, 50");
            //    Launch(Policy.A, Policy.P, 3, i, Policy.R, 50);
            //    Log.Info("Launch:" + i + "; APR, 100");
            //    Launch(Policy.A, Policy.P, 3, i, Policy.R, 100);
            //}
            #endregion

            #region averaging, binning and plotting results for given data package
            Averager.AverageAllFilesInDir(PathData, PathAverageData);
            Binninger.BinningAllFilesInDir(20, PathAverageData);
            Plotter.CreateAllDat(PathAverageData);
            // Plotter.PlotAllFromDir(pathAverageData);
            #endregion
            //Plotter.PlotRunsForFeatures(DataDir);
        }

        /// <summary>
        /// (A set of iterations with some dynamics)
        /// Create banking system with banks and customers, init topology,
        /// make iteration simulation, save system states.
        /// Output simulation results to files after all
        /// </summary>
        /// <param name="bankPolicy">A type of bank policy</param>
        /// <param name="customerPolicy">A type of customers policy</param>
        /// <param name="defaultMaturity">Maturity value by default</param>
        /// <param name="runNumber">A number of launch</param>
        static void Launch(Policy bankPolicy, Policy customerPolicy, int defaultMaturity, int runNumber)
        {
            string pathToSystemStates =Path.Combine(LaunchDataDir,"testSys6_m") + defaultMaturity.ToString("D2") + bankPolicy + customerPolicy +
                                        "_" + runNumber.ToString("D2");
            string pathToNodeStates = Path.Combine(LaunchDataDir, "nodeStates_m" + defaultMaturity.ToString("D2") + bankPolicy + customerPolicy +
                                        "_" + runNumber.ToString("D2"));
            var bSystem = new BankingSystem(bankNum, custNum);
            var systemStatesData = new List<string>();
            var nodeStatesData = new List<string>();

            for (var i = 0; i < maxIter; i++)
            {
                //Log.Info("iter=" + i);
                bSystem.Iteration(bankPolicy, customerPolicy, defaultMaturity);

                bSystem.UpdateProperties();
                systemStatesData.Add(bSystem.GetSystemState());
                // fulfill node states        
                //var nodeStateLine = bSystem.Banks.Select(x => x.NW).ToList();
                //nodeStateLine.Insert(0, i);
                //nodeStatesData.Add(String.Join(";", nodeStateLine));
                var nodeStateLine = bSystem.Banks.Average(x => x.NW);
                nodeStatesData.Add(i+";"+nodeStateLine);
            }
            GC.Collect();
 
            #region writing edges and system states
            using (var writer = new StreamWriter(pathToNodeStates))
                foreach (var line in nodeStatesData)
                    writer.WriteLine(line);
            //using (var writer = new StreamWriter(Path.Combine(DataDir, "testSys6_edges_m") + defaultMaturity.ToString("D2") + bankPolicy + customerPolicy +
            //                            "_" + runNumber.ToString("D2")))
            //{
            //    foreach (var edge in bSystem.AllEdgesOverModeling)
            //    {
            //        writer.WriteLine(edge.ToString());
            //    }
            //}
            #endregion
        }

        /// <summary>
        /// Type of launch with changeable policy
        /// </summary>
        /// <param name="bankPolicy">start bank policy</param>
        /// <param name="customerPolicy">start customer policy</param>
        /// <param name="defaultMaturity">maturity constant by default</param>
        /// <param name="runNumber">a number of launch</param>
        /// <param name="otherBankPolicy">finish bank (or customer) policy</param>
        /// <param name="iterToApplyIt">the iteration when policy changes </param>
        static void Launch(Policy bankPolicy, Policy customerPolicy, int defaultMaturity, int runNumber, Policy otherBankPolicy, int iterToApplyIt)
        {
            string pathToSystemStates = Path.Combine(LaunchDataDir, "systemStates_m") + defaultMaturity.ToString("D2") +
                                        bankPolicy + customerPolicy + otherBankPolicy + iterToApplyIt.ToString("D3") +
                                        "_" + runNumber.ToString("D2");
            var bSystem = new BankingSystem(bankNum, custNum);
            var systemStatesData = new List<string>();

            for (var i = 0; i < iterToApplyIt; i++)
            {
                // Log.Info("iter=" + i);
                bSystem.Iteration(bankPolicy, customerPolicy, defaultMaturity);

                bSystem.UpdateProperties();
                systemStatesData.Add(bSystem.GetSystemState());
            }
            for (var i = iterToApplyIt; i < maxIter; i++)
            {
                // Log.Info("iter=" + i);
                bSystem.Iteration(otherBankPolicy, customerPolicy, defaultMaturity);

                bSystem.UpdateProperties();
                systemStatesData.Add(bSystem.GetSystemState());
            }
            GC.Collect();
            using (var writer = new StreamWriter(pathToSystemStates))
                foreach (var line in systemStatesData)
                    writer.WriteLine(line);
        }

        #region FOR ENUM ELEMENTS ENTITLING
        //static string GetDescription(Enum en)
        //{

        //    Type type = en.GetType();

        //    MemberInfo[] memInfo = type.GetMember(en.ToString());

        //    if (memInfo != null && memInfo.Length > 0)
        //    {

        //        object[] attrs = memInfo[0].GetCustomAttributes(typeof(Description),
        //                                                        false);

        //        if (attrs != null && attrs.Length > 0)

        //            return ((Description)attrs[0]).Text;

        //    }

        //    return en.ToString();

        //}
        #endregion
    }
    #region FOR ENUM ELEMENTS ENTITLING
    //class Description : Attribute
    //{
    //    public string Text;
    //    public Description(string text)
    //    {
    //        Text = text;
    //    }
    //}
    #endregion
}