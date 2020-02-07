using System;
namespace BehaviorTree
{
	public interface ITreeRoot
	{
		float Time { get; }
        object UserState { get; }
		void Chanage(Composite cur);
        void SetInt(string key, int value);
        int GetInt(string key);
		bool IsDebug { get; }
	}

}

