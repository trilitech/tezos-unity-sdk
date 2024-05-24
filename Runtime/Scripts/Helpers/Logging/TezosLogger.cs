using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TezosSDK.Helpers.Logging
{
    
    public static class TezosLogger
    {
        public enum LogLevel
        {
            None,
            Error,
            Warning,
            Info,
            Debug
        }

        private static LogLevel currentLogLevel = LogLevel.Debug;
        
        public static void SetLogLevel(LogLevel logLevel)
        {
            currentLogLevel = logLevel;
        }

        private static void Log(string message, LogLevel logLevel = LogLevel.Info)
        {
            if (logLevel > currentLogLevel)
            {
                return;
            }

            string formattedMessage = FormatMessage(message, logLevel);

            switch (logLevel)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
        }

        private static string FormatMessage(string message, LogLevel logLevel)
        {
            string color;

            switch (logLevel)
            {
                case LogLevel.Debug:
                    color = "olive";
                    break;
                case LogLevel.Info:
                    color = "white";
                    break;
                case LogLevel.Warning:
                    color = "yellow";
                    break;
                case LogLevel.Error:
                    color = "red";
                    break;
                default:
                    color = "white";
                    break;
            }

            return $"<color={color}><b>[TezosSDK]</b></color> {message}";
        }

        public static void LogDebug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        public static void LogInfo(string message)
        {
            Log(message);
        }

        public static void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public static void LogError(string message)
        {
            Log(message, LogLevel.Error);
        }
    }
}