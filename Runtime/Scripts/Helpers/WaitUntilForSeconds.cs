using System;
using UnityEngine;

namespace TezosSDK.Helpers
{
    public class WaitUntilForSeconds: CustomYieldInstruction
    {
        float pauseTime;
        float timer;
        bool waitingForFirst;
        Func<bool> myChecker;
        Action<float> onInterrupt;
        bool alwaysTrue;

        public WaitUntilForSeconds(Func<bool> myChecker, float pauseTime, 
            Action<float> onInterrupt = null)
        {
            this.myChecker = myChecker;
            this.pauseTime = pauseTime;
            this.onInterrupt = onInterrupt;

            waitingForFirst = true;
        }

        public override bool keepWaiting
        {
            get
            {
                bool checkThisTurn = myChecker();
                if (waitingForFirst) 
                {
                    if (checkThisTurn)
                    {
                        waitingForFirst = false;
                        timer = pauseTime;
                        alwaysTrue = true;
                    }
                }
                else
                {
                    timer -= Time.deltaTime;

                    if (onInterrupt != null && !checkThisTurn && alwaysTrue)
                    {
                        onInterrupt(timer);
                    }
                    alwaysTrue &= checkThisTurn;

                    // Alternate version: Interrupt the timer on false, 
                    // and restart the wait
                    // if (!alwaysTrue || timer <= 0)

                    if (timer <= 0)
                    {
                        if (alwaysTrue)
                        {
                            return false;
                        }
                        else 
                        {
                            waitingForFirst = true;
                        }
                    }
                }

                return true;
            }
        }
    }
}