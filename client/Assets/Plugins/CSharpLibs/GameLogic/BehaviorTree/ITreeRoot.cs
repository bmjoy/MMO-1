using System;
namespace BehaviorTree
{
	public interface ITreeRoot
	{
		float Time { get; }
        object UserState { get; }
		void Chanage(Composite cur);
		bool IsDebug { get; }
	}

}

