using System;
using System.Collections.Generic;
using System.Threading;

namespace BigFileGenerator
{
    public class CBlockingQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly Int32 _maxItems = 10000;
        
        public void Enqueue(T value)
        {
            lock (_queue)
            {
                while (_queue.Count >= _maxItems)
                    Monitor.Wait(_queue);

                Monitor.PulseAll(_queue);
                _queue.Enqueue(value);
            }
        }

        public T Dequeue()
        {
            lock (_queue)
            {
                while (_queue.Count == 0)
                    Monitor.Wait(_queue);

                Monitor.PulseAll(_queue);
                return _queue.Dequeue();
            }
        }

        public void Clear()
        {
            lock (_queue)
            {
                _queue.Clear();
                Console.WriteLine($"{nameof(CBlockingQueue<T>)}.{nameof(Clear)}: queue cleared.");
                Monitor.PulseAll(_queue);
            }
        }
    }
}