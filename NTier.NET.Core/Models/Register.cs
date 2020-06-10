using NTier.NET.Core.Enums;

namespace NTier.NET.Core.Models
{
    public class Register : NTierBase, IRegister
    {
        public RegisterType RegisterType { get; set; }
    }
}
