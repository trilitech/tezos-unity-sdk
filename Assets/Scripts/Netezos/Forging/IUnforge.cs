using System.Collections;
using Netezos.Forging.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Netezos.Forging
{
    public interface IUnforge
    {
        // TODO: Change into coroutine
        Task<(string, IEnumerable<OperationContent>)> UnforgeOperationAsync(byte[] content);
    }
}
