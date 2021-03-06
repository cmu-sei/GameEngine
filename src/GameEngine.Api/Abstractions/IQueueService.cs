// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;

namespace GameEngine.Abstractions
{
    public interface IQueueService<T>
        where T : IClientProblem
    {
        Task Enqueue(T item);
    }
}

