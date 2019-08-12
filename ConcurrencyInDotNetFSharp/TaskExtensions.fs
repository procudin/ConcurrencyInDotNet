namespace ConcurrencyInDotNetFSharp

open System.Runtime.CompilerServices
open System.Threading.Tasks
open System

// Расширение стандартного функционала Task<T> для возможности монадической компоновки
// и использования в linq-нотации 
//
// Пример:
// Task<double> task = from x in Task.Run(() => 10)
//                     from y in Task.Run(() => 20)
//                     from xy in Task.Run(() => x + y)
//                     select Math.Pow(xy, 2)
[<Sealed; Extension>]
type TaskExtensions =    

    [<Extension>]   
    static member Bind(input: Task<'T>, binder: Func<'T, Task<'U>>) : Task<'U> =   
        let tcs = new TaskCompletionSource<'U>()
        input.ContinueWith(fun (task:Task<'T>) ->
            if task.IsFaulted then tcs.SetException(task.Exception.InnerException)
            elif task.IsCanceled then tcs.SetCanceled()
            else 
                try
                    binder.Invoke(task.Result).ContinueWith(fun (nextTask:Task<'U>) ->
                        tcs.SetResult(nextTask.Result)
                    ) |> ignore
                with
                | ex -> tcs.SetException(ex)
            ) |> ignore
        tcs.Task
   
    [<Extension>]
    static member Select(input: Task<'T>, selector: Func<'T, 'U>) : Task<'U> = 
        input.ContinueWith(fun (task:Task<'T>) -> selector.Invoke(task.Result))

    [<Extension>]
    static member SelectMany(input: Task<'T>, selector: Func<'T, Task<'I>>, projection: Func<'T, 'I, 'R>) : Task<'R> = 
        TaskExtensions.Bind(input, fun outer ->
            TaskExtensions.Bind(selector.Invoke(outer), fun inner -> 
                Task.FromResult(projection.Invoke(outer, inner))))
                
    [<Extension>]
    static member SelectMany(input: Task<'T>, selector: Func<'T, Task<'R>>) : Task<'R> = 
        TaskExtensions.Bind(input, fun outer ->
            TaskExtensions.Bind(selector.Invoke(outer), fun inner -> 
                Task.FromResult(inner)))
    
        


