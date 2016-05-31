using System.Collections.Generic;
using System.IO;
using SimpleBankingModel.model;

namespace SimpleBankingModel
{
    internal class Program
    {
        private static void Main()
        {
            const int bankNum = 100;
            const int custNum = 10000;
            const int maxIter = 1000;
            const string pathToSystemStates = "systemStates";

            var bSystem = new BankingSystem(bankNum, custNum);
            var systemStatesData = new List<string>();
            try
            {
                for (var i = 0; i < maxIter; i++)
                {
                    bSystem.Iteration();
                    systemStatesData.Add(bSystem.GetSystemState());
                }
            }
            finally
            {
                using (var writer = new StreamWriter(pathToSystemStates))
                    foreach (var line in systemStatesData)
                        writer.WriteLine(line);
            }
        }
    }
}