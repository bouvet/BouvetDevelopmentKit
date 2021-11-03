using Bouvet.DevelopmentKit.Internal.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bouvet.DevelopmentKit.Input.Voice
{
#pragma warning disable CS0649
    /// <summary>
    /// This class is set up by InputManagerInternal if voice recognizion is enabled in the InputSettings. This class sets up the internal event functions for dealing with voice recognizion input.
    /// It also sets up the class that gets the voice recognizion information from Unity. 
    /// </summary>
    internal class KeywordListenerInternal
    {
        // Events
        public event Action<InputSource> OnPhraseRecognized;

        // Private variables
        private KeywordListener keywordListener;
        private ConcurrentDictionary<string, Action> keywordActionDictionary = new ConcurrentDictionary<string, Action>();
        private InputSettings inputSettings;

        // Constructor
        public KeywordListenerInternal(InputSettings newInputSettings)
        {
            inputSettings = newInputSettings;
        }

        private bool inputManagerSetupComplete;
        private Task<bool> setupInputManagerInternalTask;

        /// <summary>
        /// Initializes the task for the setup of the different listeners in BouvetDevelopmentKit
        /// </summary>
        /// <param name="token"></param>
        internal async Task InitializeAsync(CancellationToken token)
        {
            await SetupKeywordsListenerInternalAsync();
        }

        /// <summary>
        /// Method for setting up the asynconous initialization of KeywordListenerInternal
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupKeywordsListenerInternalAsync(CancellationToken token = default)
        {
            if (inputManagerSetupComplete)
            {
                return true;
            }

            if (setupInputManagerInternalTask != null)
            {
                return await setupInputManagerInternalTask;
            }

            // Create setup task
            setupInputManagerInternalTask = SetupKeywordListenerInternal(token);
            bool result = await setupInputManagerInternalTask;
            setupInputManagerInternalTask = null;
            return result;
        }

        /// <summary>
        /// Internal method for setting up InputManagerInternal
        /// </summary>
        private async Task<bool> SetupKeywordListenerInternal(CancellationToken token)
        {
            try
            {
                if (keywordListener == null)
                {
                    keywordListener = new KeywordListener(inputSettings);
                }

                await keywordListener.InitializeAsync(keywordActionDictionary, token);
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up KeywordListenerInternal", e);
                return false;
            }

            keywordListener.OnPhraseRecognized += inputSource => OnPhraseRecognized?.Invoke(inputSource);

            inputManagerSetupComplete = true;

            BdkLogger.Log("KeywordListenerInternal setup succesfully", LogSeverity.Info);

            return true;
        }

        /// <summary>
        /// This function takes an input phrase and system action and generates a new voice recognizion command out of it. This function is called by InputManagerInternal.
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="action"></param>
        /// <returns>True if successful, false otherwise</returns>
        internal async Task<bool> AddPhraseForVoiceRecognizion(string phrase, Action action)
        {
            if (!inputManagerSetupComplete)
            {
                await setupInputManagerInternalTask;
            }

            keywordActionDictionary.TryAdd(phrase, action);
            BdkLogger.Log("Phrase added succesfully: " + phrase, LogSeverity.Info);
            return keywordListener.UpdatePhraseRecognizion(keywordActionDictionary);
        }
    }
#pragma warning restore CS0649
}