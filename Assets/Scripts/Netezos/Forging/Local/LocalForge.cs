using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Netezos.Encoding;
using Netezos.Forging.Models;
using Netezos.Utils;

namespace Netezos.Forging
{
    public partial class LocalForge : IForge, IUnforge
    {
        public IEnumerator ForgeOperationAsync(string branch, OperationContent content)
        {
            var branchBytes = Base58.Parse(branch, Prefix.B.Length);
            var contentBytes = ForgeOperation(content);

            yield return branchBytes.Concat(contentBytes);
        }

        public IEnumerator ForgeOperationGroupAsync(string branch, IEnumerable<ManagerOperationContent> contents)
        {
            var branchBytes = Base58.Parse(branch, Prefix.B.Length);
            var contentBytes = Bytes.Concat(contents.Select(ForgeOperation).ToArray());

            yield return branchBytes.Concat(contentBytes);
        }

        // Todo: Change into coroutine
        public Task<(string, IEnumerable<OperationContent>)> UnforgeOperationAsync(byte[] bytes)
        {
            using (var reader = new ForgedReader(bytes))
            {
                var branch = reader.ReadBase58(Lengths.B.Decoded, Prefix.B);
                var content = new List<OperationContent>();

                while (!reader.EndOfStream)
                {
                    content.Add(UnforgeOperation(reader));
                }

                return Task.FromResult((branch, (IEnumerable<OperationContent>)content));
            }
        }
    }
}
