using SimpleBankingModel.model;

namespace SimpleBankingModel{ class Program{
    static void Main(string[] args)
    {
        int bankNum = 100;
        var custNum = 10000;
        var maxIter = 1000;

        var bSystem = new BankingSystem(bankNum, custNum);
        for (var i= 0; i < maxIter; i++)
            bSystem.Iteration();
            // TODO FIX CHANGES, RECORDING etc.

    }}}