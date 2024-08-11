using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.App;
internal class CancellationTokenHandler
{
    internal CancellationTokenSource Cts { get; private set; } = null;
    internal void Create()
    {
        Cts = new CancellationTokenSource();
    }
    internal void Clear()
    {
        Cts.Dispose();
        Cts = null;
    }
}
