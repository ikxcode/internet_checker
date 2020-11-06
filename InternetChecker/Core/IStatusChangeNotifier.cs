using System;
using System.Collections.Generic;
using System.Text;

namespace InternetChecker.Core
{
    public interface IStatusChangeNotifier
    {
        void Notify(bool isAllGood);
    }
}
