using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orleans2GettingStarted
{
    interface ILayer: IGrainWithIntegerKey
    {

        Task<bool> Init(ILayer prev, List<IOperator> children, IController up);

        Task<bool> AddInNode();

        Task<bool> AddOutNode(IOperator node);

        Task<bool> Pause();

        Task<bool> Resume();

        Task<bool> AllocateNode();

        Task<string> GetName();
    }
}
