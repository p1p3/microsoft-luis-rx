﻿using System;
using System.Reactive.Concurrency;
using System.Reactive.Observable.Aliases;
using System.Reactive.Linq;
using Luis.Reactive;
using Luis.Reactive.Exceptions;

namespace LUISExample
{
    class Program
    {
        private const string AppId = "f2aa7663-195b-441a-93c5-709f474d84e4";
        private const string SubscriptionKey = "ef1581e766f943f297ad120d03e18e61";

        private static string _endPointUrl =
                "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{appId}?subscription-key={subscriptionKey}&verbose=true&q="
            ;

        static void Main(string[] args)
        {
            HandlersContainer.Config();

            Console.WriteLine("Let me know");
            var userInput = ConsoleInput();

            userInput.FlatMap(ActLuisReactive)
                     .Subscribe(Console.WriteLine);

            while (true);
        }
        
        private static IObservable<string> ConsoleInput()
        {
            return
                Observable
                    .FromAsync(() => Console.In.ReadLineAsync())
                    .Repeat()
                    .Publish()
                    .RefCount()
                    .SubscribeOn(Scheduler.Default);
        }

        static IObservable<Luis.Reactive.Structures.LuisResult> RequestLuisReactive(string question)
        {
            var client = new LuisReactiveClient(AppId, SubscriptionKey);
            return client.Predict(question);
        }

        private static IObservable<string> ActLuisReactive(string question)
        {
            var client = new LuisReactiveClient(AppId, SubscriptionKey);
            return client.PredictAndAct(question)
                         .Catch<string, NotHandlerFoundException>(noHandler => Observable.Return("Can you be more specific?"))
                         .Catch<string, NotIntentFoundException>(noHandler => Observable.Return("I don't understand what you are saying"))
                         .Catch<string, Exception>(noHandler => Observable.Return("Ups something Went wrong!"));
        }
    }
}


