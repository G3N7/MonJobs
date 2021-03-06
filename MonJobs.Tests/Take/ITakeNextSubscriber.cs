﻿using System;
using System.Threading.Tasks;

namespace MonJobs.Tests.Take
{
    public interface ITakeNextSubscriber
    {
        // doc: ITakeNextSubscriber
        Task Subscribe(Func<Job, Task> whatToDo, TakeNextSubscriptionOptions options);
    }
}
