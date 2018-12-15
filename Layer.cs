using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orleans2GettingStarted
{
    class Layer:Grain,ILayer
    {
        protected List<IOperator> children = new List<IOperator>();
        protected ILayer prev = null;
        protected IController up = null;
        public async Task<bool> AddInNode()
        {
            Console.WriteLine(await GetName() + "Add new nodes");
            var new_key = ++Program.num_of_operators;
            var new_grain = GrainFactory.GetGrain<IOperator>(new_key);
            var out_nodes=await children[0].GetOutNodes();
            await new_grain.Init(out_nodes, true);
            float percentage = 1.0f / (children.Count + 1);
            for(int i=0;i<children.Count;++i)
            {
                await new_grain.ReceivePartialWorkLoad(await children[i].GetPartialWorkLoad(percentage));
            }
            children.Add(new_grain);
            if (prev!=null)
                await prev.AddOutNode(new_grain);
            return true;
        }

        public async Task<bool> AddOutNode(IOperator node)
        {
            Console.WriteLine(await GetName() + "Add out nodes");
            for (int i=0;i<children.Count;++i)
            {
                await children[i].RegisterOutNode(node);
            }
            return true;
        }

        public async Task<bool> AllocateNode()
        {
            Console.WriteLine(GetName().Result + "Need allocate more nodes");
            if (up != null)
                await up.AddNodeNotify(this);
            else
            {
                Console.WriteLine(GetName().Result + "But no controller found!!!");
                return false;
            }
            return true;
        }

        public async Task<bool> Pause()
        {
            for (int i = 0; i < children.Count; ++i)
            {
                while (true)
                {
                    var res = await children[i].Pause();
                    if (res == false)
                        Console.WriteLine(await GetName() + "Failed to pause node" + children[i].GetPrimaryKeyLong());
                    else
                        break;
                }
            }
            Console.WriteLine(await GetName() + "Paused!");
            return true;
        }

        public async Task<bool> Resume()
        {
            for (int i = 0; i < children.Count; ++i)
            {
                while (true)
                {
                    var res = await children[i].Resume();
                    if (res == false)
                        Console.WriteLine(await GetName() + "Failed to resume node" + children[i].GetPrimaryKeyLong());
                    else
                        break;
                }
            }
            Console.WriteLine(await GetName() + "Resumed!");
            return true;
        }

        public Task<string> GetName()
        {
            return Task.FromResult("Layer [ID: " + this.GetPrimaryKeyLong() + "]: ");
        }

        public Task<bool> Init(ILayer prev, List<IOperator> children, IController up)
        {
            this.prev = prev;
            this.children = children;
            this.up = up;
            return Task.FromResult(true);
        }
    }
}
