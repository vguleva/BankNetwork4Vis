using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            /*
            var tarBank = ForBankChoose.Next(0, Banks.Count);
            while (tarBank == curBank)
                tarBank = ForBankChoose.Next(0, Banks.Count);
            return tarBank;
             */
        }

        /// <summary>
        /// Split segment [0:1] in accordance to assets, 
        /// so probability of choing 'i' is proportional to A_i/A
        /// </summary>
        /// <param name="curBank">The id of a bank we are choosing a partner for</param>
        /// <returns>index of bank, -1 if random wasn't in range</returns>
        internal int ChooseBank_PreferentiallyAssets(string curBank)
        {
            var random = ForBankChoose.NextDouble();
            if (random < 0 || random > 1) throw new Exception("ChooseBankPreferentiallyAssets: input random is out of range");
            var ratingArray = new double[Banks.Count];
            var allBanksAssets = Banks.Where(x=>x.ID != curBank).Sum(x => x.GetA());
            if (!(allBanksAssets > 0)) return ChooseBank();
            foreach (var bank in Banks)
                ratingArray[Int32.Parse(bank.ID.Substring(1))] = bank.ID != curBank ? (double)bank.GetA() / allBanksAssets : 0;
            var rightBound = ratingArray[0];
            for (var i = 0; i < ratingArray.Length; i++)
                if (random < rightBound)
                    return i;
                else if (i + 1 < ratingArray.Length) rightBound += ratingArray[i + 1];
            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curBank">"b"+int</param>
        /// <returns></returns>
        internal int ChooseBank_AssortativeAssets(string curBank)
        {
            if(curBank==" ") throw new Exception("The method probably was called by a customer");
            var curBankA = Banks.First(x => x.ID == curBank).GetA();
            var minDist = Int32.MaxValue;
            var minIndex = "";
            foreach (var bank in Banks.Where(x=>x.ID != curBank).Where(bank => Math.Abs(bank.GetA() - curBankA) < minDist))
            {
                minDist = Math.Abs(bank.GetA() - curBankA);
                minIndex = bank.ID;
            }
            return Int32.Parse(minIndex.Substring(1));
        }

        internal int ChooseBank_PreferentiallyNW(string curBank)
        {
            var random = ForBankChoose.NextDouble();
            if (random < 0 || random > 1) throw new Exception("ChooseBankPreferentiallyNWs: input random is out of range");
            var ratingArray = new double[Banks.Count];
            var allBanksNWs = Banks.Where(x=>x.NW>0 && x.ID != curBank).Sum(x => x.NW);
            if (!(allBanksNWs > 0)) return ChooseBank();
            foreach (var bank in Banks)
                ratingArray[Int32.Parse(bank.ID.Substring(1))] = (double)bank.NW>0 && bank.ID != curBank ? (double)bank.NW / allBanksNWs : 0;
            var rightBound = ratingArray[0];
            for (var i = 0; i < ratingArray.Length; i++)
                if (random < rightBound)
                    return i;
                else if (i + 1 < ratingArray.Length) rightBound += ratingArray[i + 1];
            return -1;
        }

        int ChooseWeight()
        {
            return EdgeWeight;
        }
        int ChooseMaturity()
        {
            return Maturity;
        }

        int ChooseMaturity(int defaultMaturity)
        {
            
            return defaultMaturity;
        }
        

        #region REWIRING POLICY (COMPARISON)
        /*
        public class CompareEdgesByBankAssetsAscendingHelper:IComparer<Edge>
        {
            
            public int Compare(Edge x, Edge y)
            {
                throw new NotImplementedException();
            }
        }
        */
        #endregion
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