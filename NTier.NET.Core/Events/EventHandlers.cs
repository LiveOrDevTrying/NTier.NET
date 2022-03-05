using NTier.NET.Core.Models;
using System.Threading.Tasks;

namespace NTier.NET.Core.Events
{
    public delegate void MessageEventHandler(object sender, IMessage message);
}
