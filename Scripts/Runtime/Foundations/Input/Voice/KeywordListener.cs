using Bouvet.DevelopmentKit.Internal.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if WINDOWS_UWP
using UnityEngine.Windows.Speech;
#endif

namespace Bouvet.DevelopmentKit.Input.Voice
{
#pragma warning disable CS0649
    /// <summary>
    /// This class is set up by KeywordListenerInternal if voice recognizion is enabled in the InputSettings. This class calls the internal event functions for dealing with voice input.
    /// </summary>
    public class KeywordListener
    {
        // Events
        public event Action<InputSource> OnPhraseRecognized;

        // Private variables
#if WINDOWS_UWP
        private KeywordRecognizer keywordRecognizer;
#endif
        private ConcurrentDictionary<string, Action> keywordActionDictionary = new ConcurrentDictionary<string, Action>();
        private InputSettings inputSettings;
        private bool alreadyInitialized;

        private InputSource inputSource;

        // Constructor
        public KeywordListener(InputSettings newInputSettings)
        {
            inputSettings = newInputSettings;
        }

#if WINDOWS_UWP
        /// <summary>
        /// Event handler that is called when a phrase is recognized. 
        /// </summary>
        /// <param name="args"></param>
        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            foreach (string phrase in keywordActionDictionary.Keys.ToArray())
            {
                if (phrase.Equals(args.text))
                {
                    keywordActionDictionary[phrase]?.Invoke();
                    inputSource.message = phrase;
                    OnPhraseRecognized.Invoke(inputSource);
                }
            }
        }
#endif
        private bool keywordListenerInitialized;
        private Task<bool> setupInternalListenersTask;

        /// <summary>
        /// Initializes the task for the setup of the keyword listener in BouvetDevelopmentKit
        /// </summary>
        /// <param name="newKeywordActionDictionary"></param>
        /// <param name="token"></param>
        internal async Task InitializeAsync(ConcurrentDictionary<string, Action> newKeywordActionDictionary, CancellationToken token)
        {
            await SetupInternalListenersAsync(token, newKeywordActionDictionary);
        }

        /// <summary>
        /// Method for setting up the asynconous initialization of KeywordListener
        /// </summary>
        /// <param name="token"></param>
        /// <param name="newKeywordActionDictionary"></param>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupInternalListenersAsync(CancellationToken token, ConcurrentDictionary<string, Action> newKeywordActionDictionary)
        {
            if (keywordListenerInitialized)
            {
                return true;
            }

            if (setupInternalListenersTask != null)
            {
                return await setupInternalListenersTask;
            }

            // Create setup task
            setupInternalListenersTask = SetupKeywordListener(token, newKeywordActionDictionary);
            bool result = await setupInternalListenersTask;
            setupInternalListenersTask = null;
            return result;
        }

        /// <summary>
        /// Internal method for setting up KeywordListener
        /// </summary>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupKeywordListener(CancellationToken token, ConcurrentDictionary<string, Action> newKeywordActionDictionary)
        {
            try
            {
                await Task.Delay(10);

                inputSource = new InputSource();
                inputSource.inputSourceKind = InputSourceKind.Voice;

                if (newKeywordActionDictionary.Count == 0)
                {
                    return true;
                }

                UpdatePhraseRecognizion(keywordActionDictionary);
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up KeywordListener", e);
                return false;
            }

            keywordListenerInitialized = true;

            BdkLogger.Log("KeywordListener setup succesfully", LogSeverity.Info);

            return true;
        }

        /// <summary>
        /// Method that updates the voice recognizion dictionary.
        /// </summary>
        /// <param name="newKeywordActionDictionary"></param>
        /// <returns></returns>
        internal bool UpdatePhraseRecognizion(ConcurrentDictionary<string, Action> newKeywordActionDictionary)
        {
#if WINDOWS_UWP
            if (alreadyInitialized)
            {
                keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;
                keywordRecognizer.Stop();
                keywordRecognizer.Dispose();
            }

            keywordActionDictionary = newKeywordActionDictionary;
            keywordRecognizer = new KeywordRecognizer(newKeywordActionDictionary.Keys.ToArray(), ConfidenceLevel.High);
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
            alreadyInitialized = true;
#endif
            return true;
        }
    }
#pragma warning restore CS0649
}