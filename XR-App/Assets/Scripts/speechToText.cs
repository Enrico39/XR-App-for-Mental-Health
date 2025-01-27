using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.IO;

public class SpeechToTextTMP : MonoBehaviour
{
    public TMP_InputField inputField; // Campo di input TMP
    private AudioClip recordedClip;
    private string serverUrl = "http://192.168.1.89:5000/speech-to-text/";
    private bool isRecording = false; // Stato della registrazione
    private string currentDevice; // Nome del dispositivo attivo

    // Metodo chiamato dal bottone
    public void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    private void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("Nessun microfono disponibile!");
            return;
        }

        if (isRecording)
        {
            Debug.LogWarning("Registrazione già in corso!");
            return;
        }

        currentDevice = Microphone.devices[0]; // Usa il primo dispositivo disponibile
        recordedClip = Microphone.Start(currentDevice, false, 10, 44100); // Registra per 10 secondi
        isRecording = true;
        Debug.Log("Registrazione iniziata.");

        // Avvia un controllo per monitorare il termine automatico della registrazione
        StartCoroutine(CheckRecordingStatus());
    }

    private void StopRecording()
    {
        if (!isRecording || !Microphone.IsRecording(currentDevice))
        {
            Debug.LogWarning("Nessuna registrazione in corso da fermare.");
            return;
        }

        Microphone.End(currentDevice);
        isRecording = false;
        Debug.Log("Registrazione terminata manualmente.");
        StartCoroutine(SendAudioToServer());
    }

    private IEnumerator CheckRecordingStatus()
    {
        while (isRecording)
        {
            // Controlla periodicamente se la registrazione è terminata automaticamente
            if (!Microphone.IsRecording(currentDevice))
            {
                Debug.Log("Registrazione terminata automaticamente.");
                isRecording = false;

                // Procedi con l'invio al server
                if (recordedClip != null)
                {
                    StartCoroutine(SendAudioToServer());
                }
                else
                {
                    Debug.LogError("Il clip audio non è valido!");
                }

                yield break; // Esci dal ciclo
            }

            yield return null; // Aspetta un frame
        }
    }


private byte[] ConvertToWav(AudioClip clip)
{
    using (MemoryStream stream = new MemoryStream())
    {
        int headerSize = 44; // Dimensione dell'header WAV
        int fileSize = headerSize + clip.samples * clip.channels * 2; // Calcolo del file size
        int samplesPerSec = 44100;

        // Scrittura dell'header WAV
        stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(fileSize - 8), 0, 4);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4); // Dimensione chunk formato
        stream.Write(BitConverter.GetBytes((ushort)1), 0, 2); // Audio format (PCM)
        stream.Write(BitConverter.GetBytes((ushort)clip.channels), 0, 2);
        stream.Write(BitConverter.GetBytes(samplesPerSec), 0, 4);
        stream.Write(BitConverter.GetBytes(samplesPerSec * clip.channels * 2), 0, 4); // Byte rate
        stream.Write(BitConverter.GetBytes((ushort)(clip.channels * 2)), 0, 2); // Block align
        stream.Write(BitConverter.GetBytes((ushort)16), 0, 2); // Bit per sample
        stream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(clip.samples * clip.channels * 2), 0, 4);

        // Scrittura dei campioni audio
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        foreach (float sample in samples)
        {
            short intSample = (short)(sample * 32767);
            stream.Write(BitConverter.GetBytes(intSample), 0, 2);
        }

        return stream.ToArray();
    }
}



    private IEnumerator SendAudioToServer()
    {
        Debug.Log("Preparazione dell'audio da inviare al server...");

 float[] samples = new float[recordedClip.samples * recordedClip.channels];
recordedClip.GetData(samples, 0);

// Converte i dati grezzi in byte array (opzionale, se vuoi vedere i dati grezzi prima della conversione in WAV)
byte[] rawAudioBytes = new byte[samples.Length * sizeof(float)];
System.Buffer.BlockCopy(samples, 0, rawAudioBytes, 0, rawAudioBytes.Length);

Debug.Log($"Dimensione audio grezzo: {rawAudioBytes.Length} byte");

// Converte l'audio in formato WAV
byte[] audioBytes = ConvertToWav(recordedClip);

Debug.Log($"Dimensione audio WAV: {audioBytes.Length} byte");


        // Crea una richiesta HTTP POST
        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(audioBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/octet-stream");

            Debug.Log("Invio dell'audio al server...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Risposta ricevuta dal server.");
                string response = request.downloadHandler.text;
                inputField.text = response; // Aggiorna il campo input
            }
            else
            {
                Debug.LogError($"Errore durante l'invio: {request.error}");
            }
        }
    }
}
