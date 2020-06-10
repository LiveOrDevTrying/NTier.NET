using NTier.NET.Core.Models;
using System.Threading.Tasks;

namespace NTier.NET.Core.Events
{
    public delegate Task MessageEventHandler(object sender, IMessage message);
}
