using System.Threading.Tasks;

namespace Glasswall.Core.Engine.Common
{
    public interface IByteReader
    {
        Task<byte[]> GetBytes();
    }
}