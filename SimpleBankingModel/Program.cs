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
        const int             BankNum = 100;
        private const int     CustNum = 1000;
        static Policy      BankPolicy = Policy.R;
        static Policy  CustomerPolicy = Policy.R;
        const int             MaxIter = 300;
        private const string LaunchDataDir   = @"d:\Valenitna\JExp\res_full_rew_data\";//null_model\";
        private const string PathData        = @"d:\Valenitna\JExp\test_regul_tune";//data2";//res_full_nodes";
        private const string PathAverageData = @"d:\Valenitna\JExp\test_regul_tune_averages";
        // private const string pathBinningData = @"d:\Valenitna\JExp\data_binning";
        private const string EdgesOverSimulationFile = "edges";
        private const string BankBalanceSheetsDir = "balance";


        private static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            if (!Directory.Exists(BankBalanceSheetsDir))
                Directory.CreateDirectory(BankBalanceSheetsDir);
            Launch(BankNum, CustNum, 1);
        }

        /// <summary>
        /// (A set of iterations with some dynamics)
        /// Create banking system with banks and customers, init topology,
        /// make iteration simulation, save system states.
        /// Output simulation results to files after all
        /// </summary>
        /// <param name="bankNum">Initial number of banks in the system</param>
        /// <param name="custNum">Initial number of customers in the system</param>
        /// <param name="runNumber">A number of launch</param>
        static void Launch(int bankNum, int custNum, int runNumber)
        {
            var bSystem          = new BankingSystem(bankNum, custNum);
            var systemStatesData = new List<string>(); // network features over all simulation period
            var nodeStatesData   = new List<string>(); // node-orien features over all simulation period

            for (var i = 0; i < MaxIter; i++)
            {
                if (i == 50) 
                    BankPolicy = Policy.A;
                bSystem.Iteration(BankPolicy, CustomerPolicy);
                bSystem.UpdateProperties();

                // Network and node features
                systemStatesData.Add(bSystem.GetSystemState());// FOR FINAL GENERAL OUTPUT
                var nodeStateLine = bSystem.Banks.Average(x => x.NW); // fulfill node states  
                nodeStatesData.Add(i + ";" + nodeStateLine);// NODE STATES AT CURRENT ITER FOR FINAL GENERAL OUTPUT
                
                // update output files
                OutputDataPerIter(bSystem, i);
            }
            GC.Collect();
        }

        /// <summary>
        /// Network edges and bank balance sheets output
        /// </summary>
        /// <param name="bSystem"></param>
        /// <param name="i"></param>
        static void OutputDataPerIter(BankingSystem bSystem, int i)
        {
            //   a) edges over simulation
            using (var writer = new StreamWriter(EdgesOverSimulationFile, true))
            {
                foreach (var edge in bSystem.AllEdgesOverSimulation.Where(x => x.Created == i))
                    //TODO DEBUG CONDITION (indexes)
                    writer.WriteLine(edge.ToStringKsenia());
            }

            //    b) bank balance sheets
            using (var writer = new StreamWriter(Path.Combine(BankBalanceSheetsDir, i.ToString("D4"))))
            {
                foreach (var bank in bSystem.Banks)
                    writer.WriteLine(bank.ID + ";" + bank.NW
                                     + ";" + bank.IA
                                     + ";" + bank.EA
                                     + ";" + bank.IL
                                     + ";" + bank.EL);
                //+ ";" + bank.Cash);
            }

            //    c) net features todo
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
            var bSystem = new BankingSystem(BankNum, CustNum);
            var systemStatesData = new List<string>();

            for (var i = 0; i < iterToApplyIt; i++)
            {
                // Log.Info("iter=" + i);
                bSystem.Iteration(bankPolicy, customerPolicy/*, defaultMaturity*/);

                bSystem.UpdateProperties();
                systemStatesData.Add(bSystem.GetSystemState());
            }
            for (var i = iterToApplyIt; i < MaxIter; i++)
            {
                // Log.Info("iter=" + i);
                bSystem.Iteration(otherBankPolicy, customerPolicy/*, defaultMaturity*/);

                bSystem.UpdateProperties();
                systemStatesData.Add(bSystem.GetSystemState());
            }
            GC.Collect();
            using (var writer = new StreamWriter(pathToSystemStates))
                foreach (var line in systemStatesData)
                    writer.WriteLine(line);
        }
    }
}