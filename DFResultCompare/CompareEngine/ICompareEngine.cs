using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFResultCompare.CompareEngine
{
    interface ICompareEngine
    {
        bool IsResultCorrect(string resultFile, string goldenStandard);

        Dictionary<string, int> GetCompareStatics();
    }
}
