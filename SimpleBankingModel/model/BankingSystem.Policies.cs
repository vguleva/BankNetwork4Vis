using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankingModel.model
{
    partial class BankingSystem
    {
        private static readonly Random ForBankChoose = new Random();
        internal int ChooseBank()
        {
            return ForBankChoose.Next(0, Banks.Count);
        }

        int ChooseWeight()
        {
            return EdgeWeight;
        }
        int ChooseMaturity()
        {
            return Maturity;
        }
    }
}// todo encapsuate bank policy as a single parameter; encapsulate the balance sheet as a class either


/*        public static int ReturnTerm()
        {
            // eval term
            int tmpTerm;
            var termKind = _forKindOfTerm.Next(1, 4);
            if (termKind == 1)
                tmpTerm = 1;
            else if (termKind == 2)
                tmpTerm = _forTerm.Next(2, 30);
            else tmpTerm = _forTerm.Next(31, 360);
            return tmpTerm;
        }
         */