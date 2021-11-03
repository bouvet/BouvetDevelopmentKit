using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Internal.Utils
{
    /// <summary>
    /// This class logs the logs from BouvetDevelopmentKit to a TextMeshProUGUI to display them in the Hololens.
    /// </summary>
    public class LogToTextComponent : MonoBehaviour
    {
        [Tooltip("Text component to display log messages")]
        public TextMeshPro TextComponent;

        private readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        [SerializeField]
        [Tooltip("Should messages be divided by a new line?")]
        private bool divideWithNewline = true;

        private List<string> logMessages;

        [SerializeField]
        [Tooltip("Should log messages also appear in console? NB! Does not work in IL2CPP builds.")]
        private bool logToConsole = true;

        [SerializeField]
        [Tooltip("Maximum amount of log messages that should be shown")]
        private int maxLogCount = 10;

        private void Awake()
        {
            if (TextComponent == null)
            {
                Debug.LogError("Please assign a text component to LogToTextComponent.cs");
                enabled = false;
                return;
            }

            Debug.LogWarning("LogToTextComponent.cs is active. This script is not performant and should be disabled in prooduction builds.");

            logMessages = new List<string>();
            TextComponent.text = "";

            BdkLogger.LogMessageEvent += InvokeInUpdate<LogSeverity>(LogMessage);
            BdkLogger.LogExceptionEvent += InvokeInUpdate<Exception>(LogException);
            BdkLogger.LogToConsole = logToConsole;
        }

        private void LogException(string message, Exception exception)
        {
            message = message.Insert(0, "Exception: ") + $" - {exception.Message}";
            LogMessage(message, LogSeverity.Error);
        }

        private void LogMessage(string message, LogSeverity severity)
        {
            if (logMessages.Count == maxLogCount)
            {
                logMessages.RemoveAt(0);
            }

            message = BdkLogger.ColorCodeLog(message, severity, true);
            logMessages.Add(message);

            StringBuilder sb = new StringBuilder();
            TextComponent.text = "";

            for (int i = 0; i < logMessages.Count; i++)
            {
                sb.Append(logMessages[i] + (i < logMessages.Count - 1 ? divideWithNewline ? "\n" : " " : ""));
            }

            TextComponent.text = sb.ToString();
        }

        private void Update()
        {
            while (mainThreadQueue.TryDequeue(out Action action))
            {
                action();
            }
        }

        /// <summary>
        /// Helper function
        /// </summary>
        private Action<string, T> InvokeInUpdate<T>(Action<string, T> func)
        {
            return (msg, severity) => mainThreadQueue.Enqueue(() => func?.Invoke(msg, severity));
        }
    }
}