using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orleans2GettingStarted
{
    interface IOperator: IGrainWithIntegerKey
    {
        Task<bool> Init(List<IOperator> out_nodes, bool paused = false);

        Task DoWork(object in_obj);

        Task<bool> RegisterOutNode(IOperator node);

        Task<bool> Pause();

        Task<bool> Resume();

        Task<string> GetName();

        Task<List<object>> GetPartialWorkLoad(float percentage);

        Task<bool> ReceivePartialWorkLoad(List<object> workload);

        Task<List<IOperator>> GetOutNodes();

    }
}
