using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orleans2GettingStarted
{
    class Controller:Grain,IController
    {
        protected IController left = null;
        protected List<ILayer> children = new List<ILayer>();
        const int max_children_num= 1;

        public async Task<bool> PauseAllPrevNodes(ILayer cur)
        {
            if (cur != null)
                Console.WriteLine(await GetName() + "Pausing all nodes prev Layer " + cur.GetPrimaryKeyLong());
            if (left != null)
                await left.PauseAllPrevNodes(null);
            for (int i=0;i<children.Count;++i)
            {
                if (children[i] == cur)
                    break;
               await children[i].Pause();
            }
            return true;
        }

        public async Task<bool> ResumeAllPrevNodes(ILayer cur)
        {
            if (cur != null)
                Console.WriteLine(await GetName() + "Resuming all nodes prev Layer " + cur.GetPrimaryKeyLong());
            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i] == cur)
                    break;
                await children[i].Resume();
            }
            if (left != null)
                await left.ResumeAllPrevNodes(null);
            return true;
        }

        public async Task<bool> AddNodeNotify(ILayer cur)
        {
            Console.WriteLine(await GetName() + "Received request of adding new node from Layer " + cur.GetPrimaryKeyLong());
            await PauseAllPrevNodes(cur);
            await cur.AddInNode();
            await ResumeAllPrevNodes(cur);
            return true;
        }

        public Task<string> GetName()
        {
            return Task.FromResult("Controller [ID: " + this.GetPrimaryKeyLong() + "]: ");
        }

        public Task<bool> Init(List<ILayer> children, IController left = null)
        {
            this.children = children;
            this.left = left;
            return Task.FromResult(true);
        }
    }

}
