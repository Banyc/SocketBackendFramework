using System.Collections.Generic;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Protocols.Kcp.Models
{
    // https://stackoverflow.com/a/34323188/9920172
    public class UniqueQueue<T> : IEnumerable<T>
    {
        private readonly HashSet<T> hashSet;
        private readonly Queue<T> queue;


        public UniqueQueue()
        {
            hashSet = new HashSet<T>();
            queue = new Queue<T>();
        }


        public int Count
        {
            get
            {
                return hashSet.Count;
            }
        }

        public void Clear()
        {
            hashSet.Clear();
            queue.Clear();
        }


        public bool Contains(T item)
        {
            return hashSet.Contains(item);
        }


        public void Enqueue(T item)
        {
            if (hashSet.Add(item))
            {
                queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            T item = queue.Dequeue();
            hashSet.Remove(item);
            return item;
        }


        public T Peek()
        {
            return queue.Peek();
        }


        public IEnumerator<T> GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return queue.GetEnumerator();
        }
    }
}
