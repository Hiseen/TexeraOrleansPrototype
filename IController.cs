using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orleans2GettingStarted
{
    interface IController:IGrainWithIntegerKey
    {

        Task<bool> Init(List<ILayer> children, IController left = null);

        Task<bool> PauseAllPrevNodes(ILayer cur);

        Task<bool> ResumeAllPrevNodes(ILayer cur);

        Task<bool> AddNodeNotify(ILayer cur);

        Task<string> GetName();
    }
}
