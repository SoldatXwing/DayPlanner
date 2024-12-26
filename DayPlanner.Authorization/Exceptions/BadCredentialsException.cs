using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Authorization.Exceptions
{
    public class BadCredentialsException(string message) : Exception(message)
    {
    }
}
