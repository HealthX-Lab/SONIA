using System;
using System.Runtime.InteropServices;
using UnityToolbag;

namespace TextToSpeechApi
{
    public struct VoiceToken
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Id;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Name;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string VoiceName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string LanguageCode;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Gender;
    }

    public struct SpeechStream
    {
        public IntPtr Data;
        public int Size;
        public int Id;
    }

    public class TextToSpeech
    {
        public readonly int samplerate = 22050;
        private struct VoiceTokens
        {
            public IntPtr Data;
            public int Size;
        }

        [DllImport("text-to-speech")]
        private static extern void StartTextToSpeech();
        [DllImport("text-to-speech")]
        private static extern VoiceTokens ListAvailableVoices();
        [DllImport("text-to-speech")]
        private static extern SpeechStream TextToSpeechStream([MarshalAs(UnmanagedType.LPWStr)] string text);
        [DllImport("text-to-speech")]
        private static extern void SetSpeechVoice([MarshalAs(UnmanagedType.LPWStr)] string voiceTokenId);
        [DllImport("text-to-speech")]
        private static extern void SetSpeechSpeed(int speed);
        [DllImport("text-to-speech")]
        private static extern void StopTextToSpeech();
        [DllImport("text-to-speech")]
        private static extern void ClearStream(int streamId);

        /// <summary>
        /// Starts Text To Speech.
        /// </summary>
        public void Init()
        {
            StartTextToSpeech();
        }

        /// <summary>
        /// Stops Text To Speech.
        /// </summary>
        public void Stop()
        {
            StopTextToSpeech();
        }

        /// <summary>
        /// Lists all available text to speech voices.
        /// </summary>
        public VoiceToken[] GetSpeechVoices()
        {
            VoiceTokens voiceTokens = ListAvailableVoices();
            int dataSize = Marshal.SizeOf(voiceTokens.Data);
            VoiceToken[] voices;
            voices = new VoiceToken[voiceTokens.Size];
            for (int i = 0; i < voiceTokens.Size; i++)
            {
                IntPtr ins = new IntPtr(voiceTokens.Data.ToInt64() + i * 5 * dataSize);
                voices[i] = (VoiceToken)Marshal.PtrToStructure(ins, typeof(VoiceToken));
            }
            return voices;
        }

        /// <summary>
        /// Speaks a text using Windows Text To Speech API
        /// </summary>
        /// <param name="text">Text to speech</param>
        public Future<float[]> SpeechText(string text)
        {
            Future<float[]> future = new Future<float[]>();
            // Create a thread since long texts can block the scene
            future.Process(() => {
                // Read the speech stream data and convert it into a float array
                SpeechStream textToSpeechStream = TextToSpeechStream(text);
                byte[] byteStreamArray = new byte[textToSpeechStream.Size];
                Marshal.Copy(textToSpeechStream.Data, byteStreamArray, 0, textToSpeechStream.Size);
                float[] floatArray = ConvertByteStreamArrayToFloatStreamArray(byteStreamArray);
                // Clear the stream from memory
                ClearStream(textToSpeechStream.Id);
                return floatArray;
            });
            return future;
        }

        /// <summary>
        /// Sets a new speech speed.
        /// (-10=slowest, 0=normal, 10=fastest)
        /// </summary>
        /// <param name="newSpeed">New speed to set</param>
        public void SetNewSpeechSpeed(int newSpeed)
        {
            SetSpeechSpeed(newSpeed);
        }

        /// <summary>
        /// Sets a new speech voice by its id.
        /// </summary>
        /// <param name="voiceTokenId">New voice id to set</param>
        public void SetNewSpeechVoice(string voiceTokenId)
        {
            SetSpeechVoice(voiceTokenId);
        }

        /// <summary>
        /// Converts a byte array to a float array.
        /// </summary>
        /// <param name="array">Byte array to convert to float array</param>
        private float[] ConvertByteStreamArrayToFloatStreamArray(byte[] array)
        {
            float[] floatArr = new float[array.Length / 2];
            for (int i = 0; i < floatArr.Length; i++)
            {
                floatArr[i] = (float)(BitConverter.ToInt16(array, i * 2) / 32768.0);
            }
            return floatArr;
        }
    }
}
