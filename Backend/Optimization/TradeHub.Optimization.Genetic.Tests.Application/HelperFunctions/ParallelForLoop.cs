/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TradeHub.Optimization.Genetic.Tests.Application.HelperFunctions
{
    public static class ParallelForLoop
    {
        public static void ContiguousParallelFor(int begin, int end, Action<int> body, int threads)
        {
            if ((end - begin) < threads) // can't split this work up anymore
                threads = end;

            int chunkSize = (end - begin) / threads;
            int chunkRemainder = (end - begin) % threads;

            CountDown latch = new CountDown(threads);

            for (int i = 0; i < threads; i++)
            {
                ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    try
                    {
                        int currentChunk = (int)o;

                        int start = begin + currentChunk * chunkSize + Math.Min(currentChunk, chunkRemainder);
                        int finish = start + chunkSize + (currentChunk < chunkRemainder ? 1 : 0);

                        for (int j = start; j < finish; j++)
                        {
                            body(j);
                        }
                    }
                    finally
                    {
                        latch.Signal();
                    }
                }, i);
            }
            latch.Wait();
        }
    }

    public class CountDown
    {
        int _initialCount;
        int _currentCount;

        AutoResetEvent mainAre = new AutoResetEvent(false);

        public int InitialCount
        {
            get { return _initialCount; }
        }

        public CountDown(int count)
        {
            _initialCount = count;
        }

        public void Wait()
        {
            while (mainAre.WaitOne())
            {
                if (_currentCount >= _initialCount)
                    break;
            }
        }

        public void Signal()
        {
            Interlocked.Increment(ref _currentCount);

            mainAre.Set();
        }

        public void Reset(bool triggerWait)
        {
            Reset(triggerWait, _initialCount);
        }

        public void Reset(bool triggerWait, int newCount)
        {
            Interlocked.Exchange(ref _currentCount, _initialCount + 1); // just in case
            mainAre.Set(); // trigger wait

            Reset(newCount);
        }

        public void Reset(int newCount)
        {
            _initialCount = newCount;
            Reset();
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _currentCount, 0);
        }
    }
}
