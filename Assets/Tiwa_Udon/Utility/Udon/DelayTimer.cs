
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon
{
    public class DelayTimer : UdonSharpBehaviour
    {
        int capacity;

        UdonSharpBehaviour[] targetUdons;
        float[] delayTimes;
        float[] startTimes;
        string[] eventNames;

        public void SetTimerCapacity(int _capacity)
        {
            capacity = _capacity;
            targetUdons = new UdonSharpBehaviour[capacity];
            eventNames = new string[capacity];
            delayTimes = new float[capacity];
            startTimes = new float[capacity];
            for (int i = 0; i < capacity; i++)
            {
                startTimes[i] = -1;
            }
        }

        public void StartTimer(int index, UdonSharpBehaviour targetUdon, string eventName, float delay)
        {
            targetUdons[index] = targetUdon;
            eventNames[index] = eventName;
            startTimes[index] = Time.time;
            delayTimes[index] = delay;
        }

        public void StopTimer(int index)
        {
            startTimes[index] = -1;
        }

        private void Update()
        {
            for (int i = 0; i < capacity; i++)
            {
                if (startTimes[i] < 0) continue;

                if (delayTimes[i] <= Time.time - startTimes[i])
                {
                    targetUdons[i].SendCustomEvent(eventNames[i]);
                    startTimes[i] = -1;
                }
            }
        }
    }
}
