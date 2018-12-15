using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans2GettingStarted
{
    class Operator : Grain, IOperator
    {
        protected List<IOperator> out_nodes = new List<IOperator>();
        protected int round_robin_idx = 0;
        protected bool paused = false;
        protected List<object> stashed = new List<object>();

        public Task DoWork(object in_obj)
        {
            Thread.Sleep(1000);
            if (paused)
            {
                Console.WriteLine(GetName().Result + "Operator has been paused! Stashing message " + in_obj);
                stashed.Add(in_obj);
            }
            else
            {
                Console.WriteLine(GetName().Result + "Process message: " + in_obj);
                if (out_nodes.Count > 0)
                {
                    out_nodes[round_robin_idx++].DoWork(in_obj);
                    round_robin_idx %= out_nodes.Count;
                }
            }
            return Task.CompletedTask;
        }

        public Task<bool> RegisterOutNode(IOperator node)
        {
            out_nodes.Add(node);
            Console.WriteLine(GetName().Result + "Node " + node.GetPrimaryKeyLong() + " registered! Now have " + out_nodes.Count + " out nodes");
            return Task.FromResult(true);
        }
        public Task<bool> Pause()
        {
            paused = true;
            Console.WriteLine(GetName().Result + "Paused!");
            return Task.FromResult(true);
        }

        public Task<bool> Resume()
        {
            paused = false;
            Console.WriteLine(GetName().Result + "Resumed!");
            ProcessStashed();
            return Task.FromResult(true);
        }

        protected void ProcessStashed()
        {
            if (stashed.Count > 0)
            {
                Console.WriteLine(GetName().Result + "Process stashed messages");
                var self = GrainFactory.GetGrain<IOperator>(this.GetPrimaryKeyLong());
                for (int i = stashed.Count - 1; i >= 0; --i)
                    self.DoWork(stashed[i]);
                stashed.Clear();
            }
        }

        public Task<string> GetName()
        {
            return Task.FromResult("Base Grain [ID: " + this.GetPrimaryKeyLong() + "]: ");
        }

        public Task<bool> Init(List<IOperator> out_nodes,bool paused=false)
        {
            this.paused = paused;
            this.out_nodes = out_nodes;
            return Task.FromResult(true);
        }

        public Task<List<object>> GetPartialWorkLoad(float percentage)
        {
            int to_return = (int)(stashed.Count * percentage);
            if (to_return > 0)
            {
                var res=stashed.GetRange(0, to_return);
                stashed.RemoveRange(0, to_return);
                return Task.FromResult(res);
            }
            return Task.FromResult(new List<object>());
        }

        public Task<bool> ReceivePartialWorkLoad(List<object> workload)
        {
            if (workload.Count > 0)
            {
                Console.WriteLine(GetName().Result + "Received Workload of "+workload.Count+" objects");
                stashed.AddRange(workload);
            }
            return Task.FromResult(true);
        }

        public Task<List<IOperator>> GetOutNodes()
        {
            return Task.FromResult(out_nodes);
        }
    }
}
