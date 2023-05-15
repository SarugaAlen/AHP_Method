using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHP_Method.Model
{
    internal class ParameterPair
    {
        public Parameter FirstParameter { get; set; }
        public Parameter SecondParameter { get; set; }

        public ParameterPair()
        {
        }
        public ParameterPair(Parameter firstParameter, Parameter secondParameter)
        {
            FirstParameter = firstParameter;
            SecondParameter = secondParameter;
        }
    }
}
