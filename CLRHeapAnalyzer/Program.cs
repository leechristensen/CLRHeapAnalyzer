using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRHeapAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("Usage: CLRHeapAnalyzer <dumpfile>");
                return;
            }

            try
            {
                using (DataTarget dataTarget = DataTarget.LoadCrashDump(args[0]))
                {
                    ClrInfo version = dataTarget.ClrVersions[0];
                    ClrRuntime runtime = version.CreateRuntime();
                    ClrHeap heap = runtime.GetHeap();

                    if (!heap.CanWalkHeap)
                    {
                        string err = "Error: Cannot walk the heap!";
                        Console.WriteLine(err);
                        throw new Exception(err);
                    }

                    var stats = from o in heap.EnumerateObjects()
                                let t = heap.GetObjectType(o)
                                group o by t into g
                                let size = g.Sum(o => (uint)g.Key.GetSize(o))
                                orderby size
                                select new
                                {
                                    Name = g.Key.Name,
                                    Size = size,
                                    Count = g.Count()
                                };

                    foreach (var item in stats)
                        Console.WriteLine("{0,12:n0} {1,12:n0} {2}", item.Size, item.Count, item.Name);
                }
            } catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
