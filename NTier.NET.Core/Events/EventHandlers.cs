using NTier.NET.Core.Models;
using System.Threading.Tasks;

namespace NTier.NET.Core.Events
{
    public delegate Task NTierMessageEventHandler(object sender, INTierMessageDTO message);
}
