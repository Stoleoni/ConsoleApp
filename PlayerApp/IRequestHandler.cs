using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerApp
{
    public interface IRequestHandler
    {
        string GetPlayers(string url);
    }
}
