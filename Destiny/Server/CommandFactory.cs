﻿using Destiny.Game.Characters;
using System;
using System.Collections.ObjectModel;
using System.Reflection;

// CREDITS: Loki
namespace Destiny.Server
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract string Parameters { get; }
        public abstract GmLevel RequiredLevel { get; }

        public abstract void Execute(Character caller, string[] args);

        public void ShowSyntax(Character caller)
        {
            caller.Notify(string.Format("!{0} {1}", this.Name, this.Parameters));
        }

        public string CombineArgs(string[] args, int start = 0)
        {
            string result = string.Empty;

            for (int i = start; i < args.Length; i++)
            {
                result += args[i] + ' ';
            }

            return result.Trim();
        }

        public string CombineArgs(string[] args, int start, int length)
        {
            string result = string.Empty;

            for (int i = start; i < length; i++)
            {
                result += args[i] + ' ';
            }

            return result.Trim();
        }
    }

    public sealed class CommandFactory : KeyedCollection<string, Command>
    {
        public CommandFactory() : base() { }

        public void Initialize()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(Command)))
                {
                    this.Add((Command)Activator.CreateInstance(type));
                }
            }
        }

        public void Execute(Character caller, string text)
        {
            string[] splitted = text.Split(' ');

            splitted[0] = splitted[0].ToLower();

            string commandName = splitted[0].TrimStart(Constants.CommandIndiciator);

            string[] args = new string[splitted.Length - 1];

            for (int i = 1; i < splitted.Length; i++)
            {
                args[i - 1] = splitted[i];
            }

            if (this.Contains(commandName))
            {
                Command command = this[commandName];

                if (caller.Client.Account.GmLevel >= command.RequiredLevel)
                {
                    try
                    {
                        command.Execute(caller, args);
                    }
                    catch (Exception e)
                    {
                        caller.Notify("[Command] Unknown error: " + e.Message);
                    }
                }
                else
                {
                    caller.Notify("[Command] Restricted command.");
                }
            }
            else
            {
                caller.Notify("[Command] Invalid command.");
            }
        }

        protected override string GetKeyForItem(Command item)
        {
            return item.Name;
        }
    }
}
