using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    /// <summary>
    /// 并行执行所有子节点 直到有一个子节点返回success
    /// 所有子节点都返回failure则返回failure
    /// </summary>
    public class ParallelPrioritySelector : GroupComposite
    {
        public ParallelPrioritySelector(params Composite[] children)
            : base(children)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            foreach (var i in Children)
            {
                i.Start(context);
            }

            var status = RunStatus.Running;
            while (status == RunStatus.Running)
            {
                //默认执行完成
                status = RunStatus.Failure;
                foreach (var i in Children)
                {
                    //如果已经执行完跳过
                    if (i.LastStatus.HasValue && i.LastStatus.Value != RunStatus.Running) continue;
                    if (i.Tick(context) == RunStatus.Success)
                    {
                        status = RunStatus.Success;
                        break;
                    }
                    status = RunStatus.Running;
                }
                yield return status;
                if (status != RunStatus.Running) yield break;
            }

            yield return  RunStatus.Failure;
        }


	}
}