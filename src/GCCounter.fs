module Counter

open System
let outstandingRequestCount = ref 0
let mutable lastRequestCount = 0
let mutable GCRun = false


let syncTimer = new System.Timers.Timer(1000.0)
syncTimer.Elapsed.Add(fun arg ->
    if (lastRequestCount > 0 && lastRequestCount = outstandingRequestCount.Value)
    then
        if not GCRun
        then
            Console.WriteLine("Running GC {0} {1}", lastRequestCount, outstandingRequestCount.Value)
            GCRun <- true
            GC.Collect(2)
    else
        GCRun <- false
    lastRequestCount <- outstandingRequestCount.Value
)
syncTimer.AutoReset <- true
syncTimer.Enabled <- true
syncTimer.Start()