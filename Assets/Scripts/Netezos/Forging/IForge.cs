using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Netezos.Forging.Models;

namespace Netezos.Forging
{
    public interface IForge
    {
        IEnumerator ForgeOperationAsync(string branch, OperationContent content);

        IEnumerator ForgeOperationGroupAsync(string branch, IEnumerable<ManagerOperationContent> contents);
    }
}
