namespace ConcurrencyInDotNetFSharp

open System.Collections.Generic

module private DictionaryAgentMessageModule =
    type DictionaryAgentMessage<'k, 'v> =
        | AddIfNotExists of key:'k * value:'v
        | RemoveIfExists of key:'k

// Агент для потокобезопасной записи/удаления записей из словаря (решение проблемы производителя-потребителя без использования иммутабельных/конкуррентных коллекций)
// Используется ассинхронная очередь для аккумулирования сообщений, поступающих из конкуррнтной среды, для их дальнейшей обработки
type DictionaryAgent<'k, 'v> when 'k: equality () =
    let agent = MailboxProcessor<DictionaryAgentMessageModule.DictionaryAgentMessage<'k, 'v>>.Start(fun inbox ->
        let dict = Dictionary<'k, 'v>()
        let rec loop() = async {
            let! msg = inbox.Receive()
            match msg with
            | DictionaryAgentMessageModule.AddIfNotExists(key, value) ->
                let exists, _ = dict.TryGetValue(key)
                if not exists then
                    dict.Add(key, value)                       
            | DictionaryAgentMessageModule.RemoveIfExists(key) ->
                let exists, _ = dict.TryGetValue(key)
                if (exists) then
                    dict.Remove(key) |> ignore        
            return! loop()        
        }
        loop()
    )     
    member this.AddIfNotExists(key, value) = agent.Post(DictionaryAgentMessageModule.AddIfNotExists(key, value))
    member this.RemoveIfExists(key) = agent.Post(DictionaryAgentMessageModule.RemoveIfExists(key))     
    

