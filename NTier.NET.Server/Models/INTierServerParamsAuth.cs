using System;
using System.Collections.Generic;
using System.Text;

namespace NTier.NET.Server.Models
{
    public interface INTierServerParamsAuth : INTierServerParams
    {
        string ConnectionUnauthorizedString { get; set; }
    }
}
