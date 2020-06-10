using System;
using System.Collections.Generic;
using System.Text;

namespace NTier.NET.Server.Models
{
    public interface INTierServerParams
    {
        int Port { get; set; }
        string ConnectionSuccessString { get; set; }
    }
}
