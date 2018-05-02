using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DougKlassen.Revit.AutoOptions.DomainModels;

namespace DougKlassen.Revit.AutoOptions.ConfigRepo
{
    public interface IAutoOptionsRepository
    {
        AutoOptionsSettings LoadAutoOptions();
        void WriteAutoOptions(AutoOptionsSettings autoOptionsListParam);
    }
}
