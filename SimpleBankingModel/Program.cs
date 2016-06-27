using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        const int custNum = 1000;
        const int maxIter = 400;
        private const string DataDir  = @"d:\Valenitna\JExp\data\";//null_model\";
        private const string pathData = @"d:\Valenitna\JExp\data";
        private const string pathAverageData = @"d:\Valenitna\JExp\data_averages";
        // private const string pathBinningData = @"d:\Valenitna\JExp\data_binning";

        private static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            //var bankPolicies = new List<Policy>() { Policy.R, Policy.P, Policy.A };
            //var custPolicies = new List<Policy>() { Policy.R, Policy.P };
            //var maturities = new List<int>() { 1, 5, 9 };
            //for (var i = 16; i < 50; i++)
            //    foreach (var bPol in bankPolicies)
            //        foreach (var custPolicy in custPolicies)
            //        {
            //            Log.Info("Launch:" + i + "; bPol:" + bPol + "; cPol:" + custPolicy);
            //            //foreach (var maturity in maturities)
            //            Launch(bPol, custPolicy, 3 /*maturity*/, i);
            //        }
            for (var i = 26; i < 27; i++)
            {
                Log.Info("Launch:" + i + "; PPA, 25");
                Launch(Policy.P, Policy.P, 3, i, Policy.A, 25);
                Log.Info("Launch:" + i + "; PPA, 50");
                Launch(Policy.P, Policy.P, 3, i, Policy.A, 50);
                Log.Info("Launch:" + i + "; PPA, 100");
                Launch(Policy.P, Policy.P, 3, i, Policy.A, 100);

                Log.Info("Launch:" + i + "; PPR, 25");
                Launch(Policy.P, Policy.P, 3, i, Policy.R, 25);
                Log.Info("Launch:" + i + "; PPR, 50");
                Launch(Policy.P, Policy.P, 3, i, Policy.R, 50);
                Log.Info("Launch:" + i + "; PPR, 100"); 
                Launch(Policy.P, Policy.P, 3, i, Policy.R, 100);

                Log.Info("Launch:" + i + "; APR, 25");
                Launch(Policy.A, Policy.P, 3, i, Policy.R, 25);
                Log.Info("Launch:" + i + "; APR, 50");
                Launch(Policy.A, Policy.P, 3, i, Policy.R, 50);
                Log.Info("Launch:" + i + "; APR, 100");
                Launch(Policy.A, Policy.P, 3, i, Policy.R, 100);

               // GC.Collect();
            }

            Averager.AverageAllFilesInDir(pathData, pathAverageData);
            Binninger.BinningAllFilesInDir(20, pathAverageData);
            Plotter.CreateAllDat(pathAverageData);
            Plotter.PlotAllFromDir(pathAverageData);
            //Plotter.PlotRunsForFeatures(DataDir);
        }

        static void Launch(Policy bankPolicy, Policy customerPolicy, int defaultMaturity, int runNumber)
        {
            string pathToSystemStates =Path.Combine(DataDir,"systemStates_m") + defaultMaturity.ToString("D2") + bankPolicy + customerPolicy +
                                        "_" + runNumber.ToString("D2");
            var bSystem = new BankingSystem(bankNum, custNum);
            var systemStatesData = new List<string>();

            for (var i = 0; i < maxIter; i++)
            {
                //Log.Info("iter=" + i);
                bSystem.Iteration(bankPolicy, customerPolicy, defaultMaturity);

                bSystem.UpdateProperties();
                systemStatesData.Add(bSystem.GetSystemState());
            }
            GC.Collect();
            using (var writer = new StreamWriter(pathToSystemStates))
                foreach (var line in systemStatesData)
                    writer.WriteLine(line);
        }

        static void Launch(Policy bankPolicy, Policy customerPolicy, int defaultMaturity, int runNumber, Policy otherBankPolicy, int iterToApplyIt)
        {
            string pathToSystemStates = Path.Combine(DataDir, "systemStates_m") + defaultMaturity.ToString("D2") +
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
    }
    //class Description : Attribute
    //{
    //    public string Text;
    //    public Description(string text)
    //    {
    //        Text = text;
    //    }
    //}
}